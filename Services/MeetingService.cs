using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Stageproject_ATS_AP2025Q2.Models;
using System.Net.Mail;

using Microsoft.Graph.Users.Item.SendMail;

namespace Stageproject_ATS_AP2025Q2.Services
{
    public class MeetingService
    {
        private readonly InterviewNoteService _noteService;
        private readonly ApplicationService _applicationService;
        private readonly TemplateService _templateService;
        private readonly GraphServiceClient _graphClient;
        private readonly GraphEmailService _graphEmailService;

        private readonly string _organizerUserId;

        public MeetingService(
            InterviewNoteService noteService,
            ApplicationService applicationService,
            TemplateService templateService,
            GraphEmailService graphEmailService,
            IConfiguration config)
        {
            _noteService = noteService;
            _applicationService = applicationService;
            _graphEmailService = graphEmailService;
             _templateService = templateService;

            var tenantId = config["Graph:TenantId"];
            var clientId = config["Graph:ClientId"];
            var clientSecret = config["Graph:ClientSecret"];
            _organizerUserId = config["Graph:OrganizerUserId"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException("Graph configuration values missing in appsettings.json");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credential);
        }
public async Task CreateMeetingAsync(InterviewNote note)
{
    if (note == null) throw new ArgumentNullException(nameof(note));
    if (note.InterviewDate == null) throw new ArgumentException("InterviewDate cannot be null");
    if (note.InterviewTime == null) throw new ArgumentException("InterviewTime cannot be null");

    // 1️⃣ Save interview note
    await _noteService.AddNoteAsync(note);

    // 2️⃣ Get related application + candidate email
    var application = await _applicationService.GetByIdAsync(note.ApplicationId);
    var user = application?.User;
    var candidateEmail = user?.Email;
    var vacancyTitle = application?.Vacancy?.Title ?? "Vacancy";
    string candidateFullName = user != null ? $"{user.FirstName} {user.LastName}" : note.FullName;

    // 3️⃣ Prepare calendar event
    var start = note.InterviewDate.ToDateTime(TimeOnly.FromTimeSpan(note.InterviewTime));
    var end = start.AddMinutes(30);

    var @event = new Microsoft.Graph.Models.Event
    {
        Subject = $"Interview {vacancyTitle} - {candidateFullName}",
        Body = new ItemBody
        {
            ContentType = BodyType.Html,
            Content = $"Dear {candidateFullName},<br><br>You have an interview scheduled for the position <b>{vacancyTitle}</b>.<br><br>Join via Microsoft Teams when the meeting starts."
        },
        Start = new DateTimeTimeZone
        {
            DateTime = start.ToString("yyyy-MM-ddTHH:mm:ss"),
            TimeZone = "Europe/Brussels"
        },
        End = new DateTimeTimeZone
        {
            DateTime = end.ToString("yyyy-MM-ddTHH:mm:ss"),
            TimeZone = "Europe/Brussels"
        },
        Location = new Location { DisplayName = "Online (Microsoft Teams)" },
        IsOnlineMeeting = true,
        OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness
    };

    // 4️⃣ Determine if candidate has Microsoft account
    bool isCandidateMicrosoftAccount = !string.IsNullOrWhiteSpace(candidateEmail) &&
        (candidateEmail.EndsWith("@outlook.com", StringComparison.OrdinalIgnoreCase) ||
         candidateEmail.EndsWith("@hotmail.com", StringComparison.OrdinalIgnoreCase) ||
         candidateEmail.EndsWith("@live.com", StringComparison.OrdinalIgnoreCase) ||
         candidateEmail.EndsWith("@office365.com", StringComparison.OrdinalIgnoreCase));

    string teamsLink = "Teams link will be provided later";

    var attendees = new List<Attendee>();

    // 5️⃣ Add candidate if Microsoft account
    if (isCandidateMicrosoftAccount)
    {
        attendees.Add(new Attendee
        {
            EmailAddress = new EmailAddress { Address = candidateEmail, Name = candidateFullName },
            Type = AttendeeType.Required
        });
    }

    // 6️⃣ Add planner (always as attendee)
    if (!string.IsNullOrWhiteSpace(note.CreatedBy))
    {
        attendees.Add(new Attendee
        {
            EmailAddress = new EmailAddress { Address = note.CreatedBy, Name = "Planner" },
            Type = AttendeeType.Optional
        });
    }

    // 7️⃣ Add extra internal users (admins/managers) as attendees
    if (note.InvitedUserEmails != null && note.InvitedUserEmails.Any())
    {
        foreach (var email in note.InvitedUserEmails)
        {
            attendees.Add(new Attendee
            {
                EmailAddress = new EmailAddress { Address = email, Name = email },
                Type = AttendeeType.Optional
            });
        }
    }

    // 8️⃣ Assign attendees and create Graph event
    @event.Attendees = attendees;
    var calendarEvent = await _graphClient.Users[_organizerUserId].Events.PostAsync(@event);
    teamsLink = calendarEvent?.OnlineMeeting?.JoinUrl ?? teamsLink;

    // 9️⃣ Send email template only if candidate has no Microsoft account
    if (!isCandidateMicrosoftAccount && !string.IsNullOrWhiteSpace(candidateEmail))
    {
        string candidateMessage =
            $"Dear {candidateFullName},<br><br>Your interview for <b>{vacancyTitle}</b> is scheduled on {note.InterviewDate:dddd, dd MMMM yyyy} at {note.InterviewTime:hh\\:mm}.<br>" +
            $"Join via Microsoft Teams using this link: <a href='{teamsLink}'>Join Teams Meeting</a><br><br>Best regards,<br>ATS Team";

        await _graphEmailService.SendEmailAsync(candidateEmail, $"Interview Scheduled – {vacancyTitle}", candidateMessage);
    }

}

public async Task DeleteMeetingAsync(int noteId)
{
    var note = await _noteService.GetNoteByIdAsync(noteId);
    if (note == null)
        throw new Exception("Interview note not found.");

    var application = await _applicationService.GetByIdAsync(note.ApplicationId);
    var user = application?.User;
    if (user == null)
        throw new Exception("Candidate user not found.");

    string candidateFullName = $"{user.FirstName} {user.LastName}";
    string candidateEmail = user.Email;
    string vacancyTitle = application?.Vacancy?.Title ?? "Vacancy";

    var events = await _graphClient.Users[_organizerUserId].Events
        .GetAsync(req => req.QueryParameters.Top = 50);

    var startDateTime = note.InterviewDate.ToDateTime(TimeOnly.FromTimeSpan(note.InterviewTime));

    var graphEvent = events?.Value?.FirstOrDefault(e =>
        e.Subject == $"Interview {vacancyTitle} - {candidateFullName}" &&
        DateTime.Parse(e.Start.DateTime) == startDateTime);

    if (graphEvent != null)
    {
        if (graphEvent.Attendees != null)
        {
            foreach (var attendee in graphEvent.Attendees)
            {
                try
                {
                    await _graphClient.Users[attendee.EmailAddress.Address].Events[graphEvent.Id].DeleteAsync();
                }
                catch
                {
                }
            }
        }

        await _graphClient.Users[_organizerUserId].Events[graphEvent.Id].DeleteAsync();
    }

    // Stuur annulatie-mail naar kandidaat
    if (!string.IsNullOrWhiteSpace(candidateEmail))
    {
        var template = await _templateService.GetTemplateByNameAsync("MeetingCancelledEmail");
        if (template != null)
        {
            string message = ReplacePlaceholders(template.HtmlContent, user, note, vacancyTitle, note.CreatedBy, "");
            await _graphEmailService.SendEmailAsync(candidateEmail, $"Interview Cancelled – {vacancyTitle}", message);
        }
    }

    // Verwijder note uit database
    await _noteService.DeleteNoteAsync(noteId);
}


        private string ReplacePlaceholders(string template, AppUser user, InterviewNote note, string vacancyTitle, string plannerName, string teamsLink)
        {
            return template
                .Replace("{FullName}", $"{user.FirstName} {user.LastName}")
                .Replace("{FirstName}", user.FirstName ?? "")
                .Replace("{LastName}", user.LastName ?? "")
                .Replace("{CandidateName}", $"{user.FirstName} {user.LastName}")
                .Replace("{PlannerName}", plannerName)
                .Replace("{Date}", note.InterviewDate.ToString("dddd, dd MMMM yyyy"))
                .Replace("{Time}", note.InterviewTime.ToString(@"hh\:mm"))
                .Replace("{Subject}", vacancyTitle)
                .Replace("{TeamsLink}", teamsLink)
                .Replace("{Year}", DateTime.Now.Year.ToString());
        }

        private string ReplacePlannerPlaceholders(string template, string candidateFullName, InterviewNote note, string vacancyTitle, string teamsLink)
        {
            return template
                .Replace("{FullName}", candidateFullName)
                .Replace("{CandidateName}", candidateFullName)
                .Replace("{CreatedBy}", note.CreatedBy)
                .Replace("{InterviewDate}", note.InterviewDate.ToString("dddd, dd MMMM yyyy"))
                .Replace("{InterviewTime}", note.InterviewTime.ToString(@"hh\:mm"))
                .Replace("{VacancyTitle}", vacancyTitle)
                .Replace("{TeamsLink}", $"<a href='{teamsLink}'>Join Teams Meeting</a>")
                .Replace("{Year}", DateTime.Now.Year.ToString());
        }
     
}

    }
