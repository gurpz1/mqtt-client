using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using ApplicationPoller.Meeting;
using ApplicationPoller.Meeting.Apps;
using AuraLight.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;
using MQTTClient.Mqtt;
using MQTTClient.Mqtt.Handlers;
using MQTTClient.Mqtt.Messages;
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
            // Random guid to identify application to prevent multiple instances running by accident
            Mutex mutex = new Mutex(true, "475065FE-BA19-4850-B501-9B582D25D256");
            
            // Setup DI
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, args);
            
            // Launch
            using (var services = serviceCollection.BuildServiceProvider())
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogDebug("Application Launching...");

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                if (mutex.WaitOne(TimeSpan.FromSeconds(1), true))
                {
                    try
                    {
                        var mqttContext = services.GetService<MQTTClientContext>();
                        Application.Run(mqttContext);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                else
                {
                    MessageBox.Show("An instance of MQTTClient is already running.");
                }
            }
        }

        /// <summary>
        /// Registers all DI services
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="args"></param>
        private static void ConfigureServices(IServiceCollection serviceCollection, string[] args)
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
            
            // Initialise MQTT command handlers
            serviceCollection.AddTransient<ICommandHandler, AuraCommandHandler>();
            serviceCollection.AddSingleton<ICommandTriager, CommandTriager>();
            
            // Initialise MQTT Client
            serviceCollection.AddSingleton<IMqttClientFacade, MqttClientFacade>();
            
            // Initialise Meeting Apps
            serviceCollection.AddSingleton<IMeetingApplicationPoller, Lync>(services =>
                new Lync(services.GetRequiredService<ILogger<Lync>>(), GetSettingsForApplication(services, "Lync")));
            serviceCollection.AddSingleton<IMeetingApplicationPoller, Webex>(services =>
                new Webex(services.GetRequiredService<ILogger<Webex>>(), GetSettingsForApplication(services, "Webex")));
            serviceCollection.AddSingleton<IMeetingApplicationPoller, Zoom>(services =>
                new Zoom(services.GetRequiredService<ILogger<Zoom>>(), GetSettingsForApplication(services, "Zoom")));
            
            // Initialise main form
            serviceCollection.AddScoped<MQTTClientContext>();
        }
        

        private static MeetingApplicationSettings GetSettingsForApplication(IServiceProvider serviceProvider, string applicationName)
        {
            var settings = serviceProvider.GetRequiredService<IOptions<Dictionary<string, MeetingApplicationSettings>>>();
            return settings.Value.GetValueOrDefault(applicationName,new MeetingApplicationSettings());
        }
    }
}
