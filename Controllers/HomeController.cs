using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using Radar7D.Common;
using Radar7D.Models.Common;
using Radar7D.Models.Dtos;
using Radar7D.Services;

namespace Radar7D.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
#if !DEBUG // Con esta condicíón, solo se va a ejecutar este código cuando se ejecute en modo Release.
        [Authorize] // Este decorador es el que hace que se ejecute la configuración de la federación y muestre la pantalla del login. 
#endif
    [EnableCors("CorsPolicy")]
    public class HomeController : ControllerBase
    {
        private readonly AuthHelper AuthHelper;
        private readonly IConfiguration Configuration;
        private readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

        public HomeController(IConfiguration configuration, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            AuthHelper = new AuthHelper(configuration, environment, httpContextAccessor);
            Configuration = configuration;
        }

        [HttpGet]
        public async Task<Models.Common.Response<UserClaims>> GetUserClaims()
        {
            UserClaims userProfile = AuthHelper.GetClaims();

            UserProfileDto userProfileDto = new()
            {
                Email = userProfile.Email,
                PayrollID = userProfile.PayrollID,
                PersonID = userProfile.PersonID,
            };
            return new Models.Common.Response<UserClaims>(userProfile);
        }

    }
}
