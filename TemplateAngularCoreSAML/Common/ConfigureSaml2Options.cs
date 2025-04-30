using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace TemplateAngularCoreSAML.Common
{
    public class ConfigureSaml2Options : IConfigureOptions<Saml2Configuration>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfigureSaml2Options(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public void Configure(Saml2Configuration saml2Configuration)
        {
            try
            {
                saml2Configuration.Issuer = _configuration["Saml2:Issuer"];
                saml2Configuration.SingleSignOnDestination = new Uri(_configuration["Saml2:SingleSignOnDestination"]);
                saml2Configuration.SingleLogoutDestination = new Uri(_configuration["Saml2:SingleLogoutDestination"]);
                saml2Configuration.SignatureAlgorithm = _configuration["Saml2:SignatureAlgorithm"];
                saml2Configuration.SignAuthnRequest = Convert.ToBoolean(_configuration["Saml2:SignAuthnRequest"]);
                // Se recupera el certificado almacenado en la Azure Web App.
                X509Store certStore = new(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, _configuration["Saml2:Thumbprint"], false);
                // Obtiene el primer certificado con el Thumbprint especificado.
                if (certCollection.Count > 0)
                {
                    X509Certificate2 cert = certCollection[0];
                    // Usa el Certificado.
                    Console.WriteLine(cert.FriendlyName);
                    saml2Configuration.SigningCertificate = cert;
                }
                certStore.Close();
                saml2Configuration.CertificateValidationMode = Enum.Parse<X509CertificateValidationMode>(_configuration["Saml2:CertificateValidationMode"]);
                saml2Configuration.RevocationMode = Enum.Parse<X509RevocationMode>(_configuration["Saml2:RevocationMode"]);
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
                    Log.Error("No se cargó el IdPSsoDescriptor del metadata");
            }
            catch (Exception e)
            {
                Log.Error(e, "Ocurrió un error al configurar SAML2 options");
            }
        }
    }
}
