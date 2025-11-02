using Azure.Identity;
using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace Stageproject_ATS_AP2025Q2.Services
{
    public class GraphTokenTester
    {
        private readonly IConfiguration _config;

        public GraphTokenTester(IConfiguration config)
        {
            _config = config;
        }

        public string GetToken()
        {
            var tenantId = _config["Graph:TenantId"];
            var clientId = _config["Graph:ClientId"];
            var clientSecret = _config["Graph:ClientSecret"];

            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentNullException("Azure AD config values missing!");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // Haal token
            var token = credential.GetToken(
                new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" })
            );

            return token.Token;
        }
    }
}
