using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using TemplateAngularCoreSAML.Common;

namespace TemplateAngularCoreSAML.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    [EnableCors("CorsPolicy")]
    public class AuthController : ControllerBase
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly Saml2Configuration Saml2Config;
        private readonly IConfiguration Configuration;
        private readonly string SingleLogoutDestination;
        private readonly string SingleLogoutDestinationReturn;

        public AuthController(IOptions<Saml2Configuration> saml2Config, IConfiguration configuration)
        {
            Saml2Config = saml2Config.Value;
            Configuration = configuration;
            SingleLogoutDestination = configuration.GetSection("Saml2:SingleLogoutDestination").Get<string>();
            SingleLogoutDestinationReturn = configuration.GetSection("Saml2:SingleLogoutDestinationReturn").Get<string>();
        }

        public IActionResult Login(string returnUrl = null)
        {
            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });
            return binding.Bind(new Saml2AuthnRequest(Saml2Config)).ToActionResult();
        }

        public async Task<IActionResult> AssertionConsumerService()
        {
            try
            {
                var binding = new Saml2PostBinding();
                var saml2AuthnResponse = new Saml2AuthnResponse(Saml2Config);

                binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);

                if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
                    throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");

                binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
                await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

                var relayStateQuery = binding.GetRelayStateQuery();
                var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");

                return Redirect(returnUrl);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception Auth/AssertionConsumerService");
                return BadRequest();
            }
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                var binding = new Saml2PostBinding();
                var saml2LogoutRequest = await new Saml2LogoutRequest(Saml2Config, User).DeleteSession(HttpContext);
                return binding.Bind(saml2LogoutRequest).ToActionResult();
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception Auth/Logout");
                return BadRequest();
            }
        }

        public IActionResult LoggedOut()
        {
            var binding = new Saml2PostBinding();
            binding.Unbind(Request.ToGenericHttpRequest(), new Saml2LogoutResponse(Saml2Config));
            return Redirect(Url.Content("~/"));
        }

        public async Task<IActionResult> SingleLogout()
        {
            var genericHttpRequest = Request.ToGenericHttpRequest();

            if (new Saml2PostBinding().IsResponse(genericHttpRequest) || new Saml2RedirectBinding().IsResponse(genericHttpRequest))
            {
                try
                {                                    
                    // Flujo del Single Logout desde el Service Provider (SP). Lo dispara esta misma aplicación.
                    var saml2ConfigSP = GetNewSaml2Configuration();
                    // En este escenario es necesario que el Single Logout Destination sea: https://amfsdevl.tec.mx/nidp/saml2/slo
                    saml2ConfigSP.SingleLogoutDestination = new Uri(SingleLogoutDestination);

                    //Log.Information($"Ingreso a Auth/SingleLogout, dentro del If | Logout desde el Service Provider (SP). " +
                    //                $"El usuario esta autenticado: {User.Identity.IsAuthenticated} | " +
                    //                $"SingleLogoutDestination. {saml2ConfigSP.SingleLogoutDestination.AbsoluteUri}.");

                    var logoutRequestSP = new Saml2LogoutRequest(saml2ConfigSP, User);
                    var requestBindingSP = new Saml2PostBinding();
                    requestBindingSP.Unbind(Request.ToGenericHttpRequest(), logoutRequestSP);
                    await logoutRequestSP.DeleteSession(HttpContext);

                    var responsebindingSP = new Saml2PostBinding
                    {
                        RelayState = requestBindingSP.RelayState
                    };
                    var saml2LogoutResponse = new Saml2LogoutResponse(saml2ConfigSP)
                    {
                        InResponseToAsString = logoutRequestSP.IdAsString,
                        Status = Saml2StatusCodes.Success
                    };

                    return requestBindingSP.Bind(saml2LogoutResponse).ToActionResult();

                }
                catch (Exception e)
                {
                    Log.Error(e, "Exception Auth/SingleLogout | dentro del If | Logout desde el Service Provider (SP).");
                    return BadRequest();
                }
            }
            else
            {
                try
                {
                    // Flujo del Single Logout desde el Identity Provider (IdP). Lo dispara NAM posterior al Logout desde otra aplicación.
                    var saml2ConfigIdP = GetNewSaml2Configuration();
                    // En este escenario es necesario que el Single Logout Destination sea: https://amfsdevl.tec.mx/nidp/saml2/slo_return
                    saml2ConfigIdP.SingleLogoutDestination = new Uri(SingleLogoutDestinationReturn);

                    //Log.Information($"Ingreso a Auth/SingleLogout, dentro del Else | Logout desde el Identity Provider (IdP). " +
                    //                $"El usuario esta autenticado: {User.Identity.IsAuthenticated} | " +
                    //                $"SingleLogoutDestination. {saml2ConfigIdP.SingleLogoutDestination.AbsoluteUri}.");

                    var logoutRequestIdP = new Saml2LogoutRequest(saml2ConfigIdP, User);
                    var requestBindingIdP = new Saml2PostBinding();
                    requestBindingIdP.Unbind(Request.ToGenericHttpRequest(), logoutRequestIdP);
                    await logoutRequestIdP.DeleteSession(HttpContext);

                    var responsebindingSP = new Saml2PostBinding
                    {
                        RelayState = requestBindingIdP.RelayState
                    };
                    var saml2LogoutResponseIdP = new Saml2LogoutResponse(saml2ConfigIdP)
                    {
                        InResponseToAsString = logoutRequestIdP.IdAsString,
                        Status = Saml2StatusCodes.Success
                    };

                    return requestBindingIdP.Bind(saml2LogoutResponseIdP).ToActionResult();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Exception Auth/SingleLogout | dentro del Else | Logout desde el Identity Provider (IdP).");
                    return BadRequest();
                }              
            }
        }

        private Saml2Configuration GetNewSaml2Configuration()
        {
            try
            {
                var certBase64 = Configuration["Saml2:SigningCertificate"];
                var certPassword = Configuration["Saml2:SigningCertificatePassword"];

                var saml2Configuration = new Saml2Configuration
                {
                    Issuer = Configuration["Saml2:Issuer"],
                    SingleSignOnDestination = new Uri(Configuration["Saml2:SingleSignOnDestination"]),
                    SingleLogoutDestination = new Uri(Configuration["Saml2:SingleLogoutDestination"]),
                    SignatureAlgorithm = Configuration["Saml2:SignatureAlgorithm"],
                    SignAuthnRequest = Convert.ToBoolean(Configuration["Saml2:SignAuthnRequest"]),
                    SigningCertificate = new X509Certificate2(
                        Convert.FromBase64String(Configuration["Saml2:SigningCertificate"]),
                        Configuration["Saml2:SigningCertificatePassword"],
                        X509KeyStorageFlags.Exportable |
                        X509KeyStorageFlags.MachineKeySet |
                        X509KeyStorageFlags.PersistKeySet
                    ),
                    CertificateValidationMode = (X509CertificateValidationMode)Enum.Parse(typeof(X509CertificateValidationMode), Configuration["Saml2:CertificateValidationMode"]),
                    RevocationMode = (X509RevocationMode)Enum.Parse(typeof(X509RevocationMode), Configuration["Saml2:RevocationMode"])
                };
                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
                var entityDescriptor = new EntityDescriptor();
                entityDescriptor.ReadIdPSsoDescriptorFromUrl(new Uri(Configuration["Saml2:IdPMetadata"]));
                
                if (entityDescriptor.IdPSsoDescriptor != null)
                {
                    saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
                    saml2Configuration.SingleLogoutDestination = entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
                    saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);
                }
                else
                    Log.Error("No se cargó el IdPSsoDescriptor del metadata");

                return saml2Configuration;
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception Auth/GetNewSaml2Configuration");
                throw;
            }
        }

    }
}
