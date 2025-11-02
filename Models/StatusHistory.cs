using System.ComponentModel.DataAnnotations;
/// <summary>
/// Represents a historical status change for a job application,
/// including the date of change and optional comments.
/// </summary>
public class StatusHistory
{

    [Key]
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public StatusHistory.ApplicationStatus Status { get; set; }
    public DateTime Date { get; set; }
    public string Comment { get; set; }

    public enum ApplicationStatus
    {
        [Display(Name = "Initial Screening")]
        InitialScreening,

        [Display(Name = "Interview with HR")]
        InterviewWithHR,
        [Display(Name = "Interview with Manager")]
        InterviewWithManager,

        [Display(Name = "Second Opinion Required")]
        SecondOpinion,

        [Display(Name = "Accepted - Contract Offered")]
        Accepted,

        [Display(Name = "Training Phase")]
        TrainingPhase,

        [Display(Name = "Rejected")]
        Rejected
    }

}
