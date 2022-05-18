using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xamariners.RestClient.Test
{
    public static class Startup
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static AppSettings Config
        {
            get;
            private set;
        }
        
        static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            
        }

        public static void Init()
        {
            Config = GetApplicationConfiguration();
        }

        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static AppSettings GetApplicationConfiguration()
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();

            var iConfig = GetIConfigurationRoot(currentDirectory);

            var configuration = new AppSettings();
            iConfig
                .GetSection("AppSettings")
                .Bind(configuration);


            return configuration;
        }
    }
}
