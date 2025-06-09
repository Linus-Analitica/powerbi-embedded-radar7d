using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Serilog;
using System;
using Radar7D.Common;
using Radar7D.Services;
using Radar7D.Models.Dtos;

namespace Radar7D.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
#if !DEBUG
    [Authorize]
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