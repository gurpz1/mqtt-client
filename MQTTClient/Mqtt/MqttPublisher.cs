using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

namespace MQTTClient.Mqtt
{
    public class MqttPublisher
    {
        private IManagedMqttClient _managedPublisher;
        private ILogger<MqttPublisher> _logger;
        private IConnectionSettings _connectionSettings;

        public MqttPublisher(ILogger<MqttPublisher> logger, IOptions<ConnectionSettings> connectionSettings)
        {
            _connectionSettings = connectionSettings.Value;
            _logger = logger;


            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(_connectionSettings.ClientID)
                    .WithTcpServer(_connectionSettings.BrokerURL)
                    .WithCredentials(_connectionSettings.Username, _connectionSettings.Password)
                    .WithCleanSession()
                    .Build()
                ).Build();
            
            _managedPublisher= new MqttFactory().CreateManagedMqttClient();
            _managedPublisher.ConnectedHandler = new MqttClientConnectedHandlerDelegate(a =>
            {
                _logger.LogInformation($"Connected to {_connectionSettings.BrokerURL}");
            });
            _managedPublisher.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(a =>
            {
                _logger.LogInformation($"Disconnected to {_connectionSettings.BrokerURL}");
            });
            
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("cmnd/IOT_7E7DBC/POWER2")
                .WithPayload(Encoding.UTF8.GetBytes("ON"))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            _managedPublisher.StartAsync(options);

            _managedPublisher.PublishAsync(message);

        }
    }
}