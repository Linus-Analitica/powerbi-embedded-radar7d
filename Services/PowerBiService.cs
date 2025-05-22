
using TemplateAngularCoreSAML.Models.Dtos;
using TemplateAngularCoreSAML.Common;
using System.Threading.Tasks;

namespace TemplateAngularCoreSAML.Services
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