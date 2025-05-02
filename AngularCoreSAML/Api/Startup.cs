using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using TemplateAngularCoreSAML.Common;

namespace TemplateAngularCoreSAML
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
            services.AddSingleton<IConfigureOptions<Saml2Configuration>, ConfigureSaml2Options>();
            services.AddSaml2(slidingExpiration: true);
            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
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

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log)
        {
            IdentityModelEventSource.ShowPII = true; // Esto muestra el detalle de las posibles fallas en la federación.
            
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
#if !DEBUG // Con esta condicíón, solo se va a ejecutar este código cuando se ejecute en modo Release. Esto permite que se ejecute la configuración de la federación solo en los ambientes de publicación.
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
            });
        }
       
    }
}
