using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTClient.Meeting;
using MQTTClient.Mqtt;
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
                var logger = services.GetService<ILogger<Program>>();
                logger.LogDebug("Application Launching...");

                var x = services.GetService<MqttPublisher>();

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
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables(prefix:"MQTTClient_")
                .AddCommandLine(args)
                .Build();
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            
            
            // Initialise services
            serviceCollection.AddSingleton(configuration);
            serviceCollection.Configure<ConnectionSettings>(configuration.GetSection("ConnectionSettings"));
            serviceCollection.AddScoped<MQTTClientContext>();
            
            // Initialise MQTT Stuff
            serviceCollection.AddScoped<MqttPublisher>();
            
            // Initialise meeting apps
            serviceCollection.AddSingleton<Webex>();
            serviceCollection.AddSingleton<SkypeForBusiness>();
            
            
            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            });
        }
    }
}
