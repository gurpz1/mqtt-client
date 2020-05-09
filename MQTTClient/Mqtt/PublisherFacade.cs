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
    public class PublisherFacade
    {
        private IManagedMqttClient _managedPublisher;
        private IManagedMqttClientOptions _clientOptions;
        private ILogger<PublisherFacade> _logger;
        private IConnectionSettings _connectionSettings;

        public PublisherFacade(ILogger<PublisherFacade> logger, IOptions<ConnectionSettings> connectionSettings)
        {
            _connectionSettings = connectionSettings.Value;
            _logger = logger;
            
            _clientOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(_connectionSettings.ClientID)
                    .WithTcpServer(_connectionSettings.BrokerURL)
                    .WithCredentials(_connectionSettings.Username, _connectionSettings.Password)
                    .WithCleanSession()
                    .Build()
                ).Build();
            
            _managedPublisher= new MqttFactory().CreateManagedMqttClient();
            
            // Register base events
            _managedPublisher.ConnectedHandler = new MqttClientConnectedHandlerDelegate(a =>
            {
                _logger.LogInformation($"Connected to {_connectionSettings.BrokerURL}");
            });
            _managedPublisher.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(a =>
            {
                _logger.LogInformation($"Disconnected to {_connectionSettings.BrokerURL}");
            });
            _managedPublisher.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(a =>
            {
                _logger.LogWarning($"Connection to {_connectionSettings.BrokerURL} failed.");
            });
        }

        public void Connect()
        {
            _managedPublisher.StartAsync(_clientOptions);
        }

        public void Disconnect()
        {
            _managedPublisher.StopAsync();
        }

        public void SendMessage(MqttApplicationMessage message)
        {
            // var message = new MqttApplicationMessageBuilder()
            //     .WithTopic("stat/hello/world")
            //     .WithPayload(Encoding.UTF8.GetBytes("FREE"))
            //     .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            //     .Build();
            _managedPublisher.PublishAsync(message);
        }
    }
}