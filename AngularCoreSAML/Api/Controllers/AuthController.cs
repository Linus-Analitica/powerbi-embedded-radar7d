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
using System.Linq;
using System.Net.Http;
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
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string SingleLogoutDestination;
        private readonly string SingleLogoutDestinationReturn;

        public AuthController(IOptions<Saml2Configuration> saml2Config, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            Saml2Config = saml2Config.Value;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            SingleLogoutDestination = configuration.GetSection("Saml2:SingleLogoutDestination").Get<string>();
            SingleLogoutDestinationReturn = configuration.GetSection("Saml2:SingleLogoutDestinationReturn").Get<string>();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            try
            {
                var binding = new Saml2RedirectBinding();
                binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });
                return binding.Bind(new Saml2AuthnRequest(Saml2Config)).ToActionResult();
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception Auth/Login");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssertionConsumerService()
        {
            try
            {
                var binding = new Saml2PostBinding();
                var saml2AuthnResponse = new Saml2AuthnResponse(Saml2Config);

                binding.ReadSamlResponse(await Request.ToGenericHttpRequestAsync(), saml2AuthnResponse);

                if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
                    throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");

                binding.Unbind(await Request.ToGenericHttpRequestAsync(), saml2AuthnResponse);
                await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: ClaimsTransform.Transform);

                var relayStateQuery = binding.GetRelayStateQuery();
                var returnUrl = relayStateQuery.TryGetValue(relayStateReturnUrl, out string value) ? value : Url.Content("~/");

                return Redirect(returnUrl);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception Auth/AssertionConsumerService");
                throw;
            }
        }

        [HttpPost]
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
                throw;
            }
        }

        [HttpGet]
        public IActionResult LoggedOut()
        {
            var binding = new Saml2PostBinding();
            binding.Unbind(Request.ToGenericHttpRequest(), new Saml2LogoutResponse(Saml2Config));
            return Redirect(Url.Content("~/"));
        }

        [HttpPost]
        public async Task<IActionResult> SingleLogout()
        {
            var genericHttpRequest = await Request.ToGenericHttpRequestAsync();

            if (new Saml2PostBinding().IsResponse(genericHttpRequest) || new Saml2RedirectBinding().IsResponse(genericHttpRequest))
            {
                try
                {
                    // Flujo del Single Logout desde el Service Provider (SP). Lo dispara esta misma aplicación.
                    var saml2ConfigSP = GetNewSaml2Configuration();
                    // En este escenario es necesario que el Single Logout Destination sea: https://amfsdevl.tec.mx/nidp/saml2/slo
                    saml2ConfigSP.SingleLogoutDestination = new Uri(SingleLogoutDestination);

                    var logoutRequestSP = new Saml2LogoutRequest(saml2ConfigSP, User);
                    var requestBindingSP = new Saml2PostBinding();
                    requestBindingSP.Unbind(await Request.ToGenericHttpRequestAsync(), logoutRequestSP);
                    await logoutRequestSP.DeleteSession(HttpContext);

                    _ = new Saml2PostBinding
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
                    throw;
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

                    var logoutRequestIdP = new Saml2LogoutRequest(saml2ConfigIdP, User);
                    var requestBindingIdP = new Saml2PostBinding();
                    requestBindingIdP.Unbind(await Request.ToGenericHttpRequestAsync(), logoutRequestIdP);
                    await logoutRequestIdP.DeleteSession(HttpContext);

                    _ = new Saml2PostBinding
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
                    throw;
                }
            }
        }

        private Saml2Configuration GetNewSaml2Configuration()
        {
            try
            {
                var saml2Configuration = new Saml2Configuration
                {
                    Issuer = _configuration["Saml2:Issuer"],
                    SingleSignOnDestination = new Uri(_configuration["Saml2:SingleSignOnDestination"]),
                    SingleLogoutDestination = new Uri(_configuration["Saml2:SingleLogoutDestination"]),
                    SignatureAlgorithm = _configuration["Saml2:SignatureAlgorithm"],
                    SignAuthnRequest = Convert.ToBoolean(_configuration["Saml2:SignAuthnRequest"]),
                    SigningCertificate = CertificateUtil.Load(
                        CommonHelper.GetCertificateAbsolutePath(_configuration["Saml2:SigningCertificateFile"]),
                        _configuration["Saml2:SigningCertificatePassword"],
                        X509KeyStorageFlags.MachineKeySet | 
                        X509KeyStorageFlags.PersistKeySet | 
                        X509KeyStorageFlags.Exportable
                    ),
                    CertificateValidationMode = Enum.Parse<X509CertificateValidationMode>(_configuration["Saml2:CertificateValidationMode"]),
                    RevocationMode = Enum.Parse<X509RevocationMode>(_configuration["Saml2:RevocationMode"])
                };
                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
                var entityDescriptor = new EntityDescriptor();
                entityDescriptor.ReadIdPSsoDescriptorFromUrlAsync(_httpClientFactory, new Uri(_configuration["Saml2:IdPMetadata"])).Wait();

                if (entityDescriptor.IdPSsoDescriptor != null)
                {
                    saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
                    saml2Configuration.SingleLogoutDestination = entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
                    saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);
                }
                else
                {
                    Log.Error("No se cargó el IdPSsoDescriptor del metadata");
                }

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
