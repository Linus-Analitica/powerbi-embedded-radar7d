using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Text;
using Radar7D.Models.Common;

namespace Radar7D.Common
{
    public class AuthHelper
    {
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment Environment;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public AuthHelper(IConfiguration configuration, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            Configuration = configuration;
            Environment = environment;
            HttpContextAccessor = httpContextAccessor;
        }

        public UserClaims GetClaims()
        {
            UserClaims userClaims = new();

            try
            {
                if (Environment.IsDevelopment())
                    Configuration.GetSection("UserImpersonation").Bind(userClaims);
                else
                {
                    if (HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                    {
                        var claims = HttpContextAccessor.HttpContext.User.Identities.First().Claims.ToList();
                        userClaims.PersonID = claims?.FirstOrDefault(x => x.Type.ToLower().Contains("IDPersona".ToLower()))?.Value;
                        userClaims.UserType = claims?.FirstOrDefault(x => x.Type.ToLower().Contains("TipoUsuario".ToLower()))?.Value;
                        userClaims.PayrollID = claims?.FirstOrDefault(x => x.Type.ToLower().Contains("nameidentifier".ToLower()))?.Value;
                        userClaims.Email = claims?.FirstOrDefault(x => x.Type.ToLower().Contains("NAM_upn".ToLower()))?.Value;
                        userClaims.Profiles = claims?.FirstOrDefault(x => x.Type.ToLower().Contains("perfiles".ToLower()))?.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Ocurrió un error en el método GetUserClaims() en la clase Authentication | {e.Message}");
            }
            return userClaims;
        }
    }
}
