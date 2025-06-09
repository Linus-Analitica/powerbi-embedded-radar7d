
using Radar7D.Models.Dtos;
using Radar7D.Common;
using System.Threading.Tasks;

namespace Radar7D.Services
{
    public interface IPowerBiService
    {
        Task<PbEmbedDto> GetTokenReporteAsync(string reportId, string workspaceId);
    }

    public class PowerBiService : IPowerBiService
    {
        private readonly ITokenHelper _tokenHelper;

        public PowerBiService(ITokenHelper tokenHelper)
        {
            _tokenHelper = tokenHelper;
        }

        public async Task<PbEmbedDto> GetTokenReporteAsync(string reportId, string workspaceId)
        {
            var accessToken = await _tokenHelper.GetPowerBiAccessTokenAsync();
            var embedToken = await _tokenHelper.GenerateEmbedTokenAsync(reportId, workspaceId, accessToken);

            return new PbEmbedDto
            {
                EmbedToken = embedToken.Token,
                EmbedUrl = embedToken.EmbedUrl,
                ReportId = reportId
            };
        }
    }
}