using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace TemplateAngularCoreSAML
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    var builtConfig = config.Build();

                    if (env.IsProduction())
                    {
                        var keyVaultUri = builtConfig["KeyVaultUri"];
                        if (!string.IsNullOrEmpty(keyVaultUri))
                        {
                            config.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
                        }
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
