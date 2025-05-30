using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using TemplateAngularCoreSAML.Models.Common;

using TemplateAngularCoreSAML.Models.Common;

namespace TemplateAngularCoreSAML.Common
{

    public interface ITokenHelper
    {
        Task<string> GetPowerBiAccessTokenAsync();
        Task<(string Token, string EmbedUrl)> GenerateEmbedTokenAsync(string reportId, string workspaceId, string accessToken);
    }


    public class TokenHelper : ITokenHelper
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly AuthHelper AuthHelper;

        public TokenHelper(IMemoryCache cache, IConfiguration config)
        {
            AuthHelper = new AuthHelper(configuration, environment, httpContextAccessor);
            _cache = cache;
            _config = config;
        }

        public async Task<string> GetPowerBiAccessTokenAsync()
        {
            if (_cache.TryGetValue("PowerBiAccessToken", out string token))
                return token;

            var clientApp = ConfidentialClientApplicationBuilder
                .Create(_config["ConnectionPowerBi:ClientId"])
                .WithClientSecret(_config["ConnectionPowerBi:ClientSecret"])
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config["ConnectionPowerBi:TenantId"]}"))
                .Build();

            var result = await clientApp.AcquireTokenForClient(new[] { "https://analysis.windows.net/powerbi/api/.default" }).ExecuteAsync();

            _cache.Set("PowerBiAccessToken", result.AccessToken, TimeSpan.FromMinutes(50)); // o result.ExpiresIn
            Log.Error($"EL ACCESS TOKEN ES {result.AccessToken}");
            return result.AccessToken;

        }

        public async Task<(string Token, string EmbedUrl)> GenerateEmbedTokenAsync(string reportId, string workspaceId, string accessToken)
        {
            UserClaims userProfile = AuthHelper.GetClaims();
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");
            using var client = new PowerBIClient(new Uri("https://api.powerbi.com/"), tokenCredentials);

            var report = await client.Reports.GetReportInGroupAsync(Guid.Parse(workspaceId), Guid.Parse(reportId));
            var generateTokenRequestParameters = new GenerateTokenRequestV2
            {
                Reports = new List<GenerateTokenRequestV2Report>{
                    new GenerateTokenRequestV2Report{Id = Guid.Parse(reportId)}
                },
                Datasets = new List<GenerateTokenRequestV2Dataset>{
                    new GenerateTokenRequestV2Dataset { Id = report.DatasetId }
                },
                TargetWorkspaces = new List<GenerateTokenRequestV2TargetWorkspace>{
                    new GenerateTokenRequestV2TargetWorkspace { Id = Guid.Parse(workspaceId) }
                },
                Identities = new List<EffectiveIdentity>{
                    new EffectiveIdentity{
                        username = userProfile.PayrollID,
                        datasets = new List<string> { report.DatasetId },
                        roles = new List<string> { report.UserType }
                    }
                }
            };

            var embedToken = await client.EmbedToken.GenerateTokenAsync(generateTokenRequestParameters);

            return (embedToken.Token, report.EmbedUrl);
        }
    }
}
