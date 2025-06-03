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

            try
            {
                bool isResponse = new Saml2PostBinding().IsResponse(genericHttpRequest) ||
                    new Saml2RedirectBinding().IsResponse(genericHttpRequest);

                var saml2Config = GetNewSaml2Configuration();

                saml2Config.SingleLogoutDestination = isResponse
                    ? new Uri(SingleLogoutDestination)
                    : new Uri(SingleLogoutDestinationReturn);

                var logoutRequest = new Saml2LogoutRequest(saml2Config, User);

                Saml2Binding binding;
                if (new Saml2PostBinding().IsRequest(genericHttpRequest) || new Saml2PostBinding().IsResponse(genericHttpRequest)){
                    binding = new Saml2PostBinding();
                }
                else if (new Saml2RedirectBinding().IsRequest(genericHttpRequest) || new Saml2RedirectBinding().IsResponse(genericHttpRequest)){
                    binding = new Saml2RedirectBinding();
                }
                else
                {
                    Log.Error("No se pudo determinar el binding SAML.");
                    return BadRequest("Binding SAML no soportado.");
                }

                binding.Unbind(genericHttpRequest, logoutRequest);
                await logoutRequest.DeleteSession(HttpContext);

                var logoutResponse = new Saml2LogoutResponse(saml2Config)
                {
                    InResponseToAsString = logoutRequest.IdAsString,
                    Status = Saml2StatusCodes.Success
                };

                binding.RelayState = binding.RelayState;

                if (binding is Saml2PostBinding postBinding){return postBinding.Bind(logoutResponse).ToActionResult();}
                else if (binding is Saml2RedirectBinding redirectBinding){return redirectBinding.Bind(logoutResponse).ToActionResult();}
                else{
                    Log.Error("No se pudo determinar el binding para enviar la respuesta.");
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception Auth/SingleLogout");
                return BadRequest();
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
