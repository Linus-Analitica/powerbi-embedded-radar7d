using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using Radar7D.Common;
using Radar7D.Services;

namespace Radar7D
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                Debug.Print(msg);
                Debugger.Break();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.Configure<Saml2Configuration>(Configuration.GetSection("Saml2"));
            services.Configure<Saml2Configuration>(saml2Configuration =>
            {
                try
                {
                    saml2Configuration.Issuer = Configuration["Saml2:Issuer"];
                    saml2Configuration.SingleSignOnDestination = new Uri(Configuration["Saml2:SingleSignOnDestination"]);
                    saml2Configuration.SingleLogoutDestination = new Uri(Configuration["Saml2:SingleLogoutDestination"]);
                    saml2Configuration.SignatureAlgorithm = Configuration["Saml2:SignatureAlgorithm"];
                    saml2Configuration.SignAuthnRequest = Convert.ToBoolean(Configuration["Saml2:SignAuthnRequest"]);
                    saml2Configuration.SigningCertificate = new X509Certificate2(
                        Convert.FromBase64String(Configuration["Saml2:SigningCertificate"]),
                        Configuration["Saml2:SigningCertificatePassword"],
                        X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                    );
                    saml2Configuration.CertificateValidationMode = (X509CertificateValidationMode)Enum.Parse(typeof(X509CertificateValidationMode), Configuration["Saml2:CertificateValidationMode"]);
                    saml2Configuration.RevocationMode = (X509RevocationMode)Enum.Parse(typeof(X509RevocationMode), Configuration["Saml2:RevocationMode"]);
                    saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
                    var entityDescriptor = new EntityDescriptor();
                    entityDescriptor.ReadIdPSsoDescriptorFromUrl(new Uri(Configuration["Saml2:IdPMetadata"]));
                    //entityDescriptor.ReadIdPSsoDescriptorFromUrlAsync(_httpClientFactory, new Uri(Configuration["Saml2:IdPMetadata"])).GetAwaiter().GetResult();
                    if (entityDescriptor.IdPSsoDescriptor != null)
                    {
                        saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
                        saml2Configuration.SingleLogoutDestination = entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
                        saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);
                    }
                    else
                        throw new Exception("No se carg� el IdPSsoDescriptor del metadata");
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message);
                    throw;
                }
            });

            services.AddSaml2(slidingExpiration: true);

            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
            services.AddScoped<IPowerBiService, PowerBiService>();
            services.AddScoped<ITokenHelper, TokenHelper>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowedOrigins",
                builder =>
                {
                    builder.WithOrigins(Configuration.GetSection("AllowedOrigins").Get<string[]>());
                    builder.AllowCredentials();
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                });
            });
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });



        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log)
        {
            IdentityModelEventSource.ShowPII = true; // Esto muestra el detalle de las posibles fallas en la federaci�n.

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            log.AddSerilog();
            app.UseStaticFiles();

            if (!env.IsDevelopment())
                app.UseSpaStaticFiles();

            app.UseRouting();
#if !DEBUG // Con esta condic��n, solo se va a ejecutar este c�digo cuando se ejecute en modo Release. Esto permite que se ejecute la configuraci�n de la federaci�n solo en los ambientes de publicaci�n.
            app.UseSaml2();
            app.MapWhen(
                context =>
                {
                    return !context.User.Identity.IsAuthenticated && context.Request.Path.Value.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
                },
                config =>
                {
                    config.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await Task.FromResult(string.Empty);
                    });
                }
            );
            app.UseAuthorization();
#endif
            app.UseCors("AllowedOrigins");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });
#if !DEBUG
            app.Use(async (context, next) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync(Saml2Constants.AuthenticationScheme);
                }
                else
                {
                    await next();
                }
            });
#endif
            app.UseSpa(spa =>
            {
                spa.Options.StartupTimeout = new System.TimeSpan(0, 3, 0);
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                    spa.UseAngularCliServer(npmScript: "start");
                    //spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            });
        }

    }
}
