using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Text;
using TemplateAngularCoreSAML.Models.Common;

namespace TemplateAngularCoreSAML.Common
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
                if (Environment.IsDevelopment()) // Esta sección es para que funcione localmente sin redirigir a la federación.
                    Configuration.GetSection("UserImpersonation").Bind(userClaims);
                else
                {
                    if (HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                    {
                        var claims = HttpContextAccessor.HttpContext.User.Identities.First().Claims.ToList();

                        /* Con el siguiente código se recorre cada uno de los claims y se escriben en el Log */
                        // StringBuilder message = new();
                        // claims.ForEach(claim => { message.AppendFormat($"[ {claim.Type} - {claim.Value} ]", "\t"); });
                        // Log.Information($"Claims | {message}");
                        Log.Information($"read Claims");
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

            /* Registra en el log, los atributos del usuario que inicio sesión. */
            // Log.Information($"Usuario Inicio Sesión: [ Nómina: {userClaims.PayrollID} ] [ Email: {userClaims.Email} ] [ ID Persona: {userClaims.PersonID} ] [ Tipo Usuario: {userClaims.UserType} ] [ Perfiles: {userClaims.Profiles} ]");

            return userClaims;
        }
    }
}
