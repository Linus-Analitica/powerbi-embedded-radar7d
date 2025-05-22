using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;

using TemplateAngularCoreSAML.Services;


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

        [HttpGet("radar7")]
        public async Task<IActionResult> loadRadar7()
        {
            var ReportId = _configuration.GetSection("ConnectionPowerBi:ReportId").Get<string>();
            var WorkspaceId = _configuration.GetSection("ConnectionPowerBi:WorkspaceId").Get<string>();

            var result = await _powerBiService.GetTokenReporteAsync(ReportId, WorkspaceId);
            return Ok(result);
        }
    }
}