using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using Microsoft.Graph.Users.Item.SendMail;

namespace Stageproject_ATS_AP2025Q2.Services
{
    public class GraphEmailService
    {
        private readonly GraphServiceClient _graphClient;
        private readonly string _organizerUserId;

        public GraphEmailService(IConfiguration config)
        {
            var tenantId = config["Graph:TenantId"];
            var clientId = config["Graph:ClientId"];
            var clientSecret = config["Graph:ClientSecret"];
            _organizerUserId = config["Graph:OrganizerUserId"];

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credential);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentNullException(nameof(toEmail));

            var message = new Microsoft.Graph.Models.Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = htmlMessage
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = toEmail
                        }
                    }
                },
                IsDeliveryReceiptRequested = true,
                IsReadReceiptRequested = true
            };

            var sendMailBody = new SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            };

            await _graphClient.Users[_organizerUserId]
                .SendMail
                .PostAsync(sendMailBody);
        }
    }
}
