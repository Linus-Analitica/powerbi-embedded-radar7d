using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Serilog;
using System;
using TemplateAngularCoreSAML.Common;
using TemplateAngularCoreSAML.Services;
using TemplateAngularCoreSAML.Models.Dtos;

namespace TemplateAngularCoreSAML.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
#if !DEBUG // Con esta condicíón, solo se va a ejecutar este código cuando se ejecute en modo Release.
    [Authorize] // Este decorador es el que hace que se ejecute la configuración de la federación y muestre la pantalla del login. 
#endif
    [EnableCors("CorsPolicy")]

    public class PowerBIController : ControllerBase
    {
        private readonly IPowerBiService _powerBiService;
        private readonly IConfiguration _configuration;

        public PowerBIController(IPowerBiService powerBiService, IConfiguration configuration)
        {
            _powerBiService = powerBiService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task< Models.Common.Response<PbEmbedDto>> loadCurrentReport()
        {
                var ReportId = _configuration.GetSection("ConnectionPowerBi:ReportCurrentId").Get<string>();
                var WorkspaceId = _configuration.GetSection("ConnectionPowerBi:WorkspaceId").Get<string>();
                var result = await _powerBiService.GetTokenReporteAsync(ReportId, WorkspaceId);
                return new Models.Common.Response<PbEmbedDto>(result);

        }
        public async Task< Models.Common.Response<PbEmbedDto>> loadArchivedReport()
        {
                var ReportId = _configuration.GetSection("ConnectionPowerBi:ReportArchivedId").Get<string>();
                var WorkspaceId = _configuration.GetSection("ConnectionPowerBi:WorkspaceId").Get<string>();
                var result = await _powerBiService.GetTokenReporteAsync(ReportId, WorkspaceId);
                return new Models.Common.Response<PbEmbedDto>(result);

        }
    }
}