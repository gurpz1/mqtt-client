using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;
using MQTTClient.Mqtt;
using MQTTClient.Polling;
using Serilog;

namespace MQTTClient
{
    class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Setup DI
            IServiceCollection serviceCollection = new ServiceCollection();
            configureServices(serviceCollection, args);
            
            // Launch
            using (var services = serviceCollection.BuildServiceProvider())
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogDebug("Application Launching...");

                
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mqttContext = services.GetService<MQTTClientContext>(); 
                Application.Run(mqttContext);   
            }
        }

        /// <summary>
        /// Registers all DI services
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="args"></param>
        private static void configureServices(IServiceCollection serviceCollection, string[] args)
        {
            // Read configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables(prefix:"MQTTClient_")
                .AddCommandLine(args)
                .Build();

            // Intialise Logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            
            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            });
            
            // Initialise Config
            serviceCollection.AddSingleton(configuration);
            serviceCollection.Configure<ConnectionSettings>(configuration.GetSection("ConnectionSettings"));
            serviceCollection.Configure<Dictionary<string, MeetingApplicationSettings>>(
                configuration.GetSection("MeetingApplicationSettings"));

            // Initialise MQTT Stuff
            serviceCollection.AddScoped<ManagedClientFacade>();
            
            // Initialise Meeting Applications
            serviceCollection.AddSingleton<Webex>();
            serviceCollection.AddSingleton<Lync>();
            serviceCollection.AddSingleton<Zoom>();
            
            // Initialise main form
            serviceCollection.AddScoped<MQTTClientContext>();
        }
    }
}
