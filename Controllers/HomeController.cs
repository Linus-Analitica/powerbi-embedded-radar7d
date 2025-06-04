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
using TemplateAngularCoreSAML.Common;
using TemplateAngularCoreSAML.Models.Common;
using TemplateAngularCoreSAML.Models.Dtos;
using TemplateAngularCoreSAML.Services;

namespace TemplateAngularCoreSAML.Controllers
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
                PersonID = userProfile.Profiles,
            };

            /*var json = JsonSerializer.Serialize(userProfileDto);
            var encrypterData = CryptographycFunctions.Encrypt(json, Configuration["ConnectionApi:ClientSecret"]);
            if (!encrypterData.Key) { return new Models.Common.Response<UserClaims>("Error al enviar la información al servidor"); }
            
            SendEncryptDto sendEncryptDto = new(encrypterData.Value);

            var jsonData = JsonSerializer.Serialize(sendEncryptDto);

            var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
*/
            return new Models.Common.Response<UserClaims>(userProfile);
        }

    }
}
