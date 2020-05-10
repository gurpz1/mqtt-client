using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace MQTTClient.Mqtt
{
    public class ManagedClientFacade: IManagedClientFacade
    {
        private ConnectionSettings _connectionSettings;
        private ILogger<ManagedClientFacade> _logger;

        public IManagedMqttClient MqttClient { get; }
        public string BrokerUrl => _connectionSettings.BrokerURL;
        public IManagedMqttClientOptions ClientOptions { get; }

        public ManagedClientFacade(ILogger<ManagedClientFacade> logger, IOptions<ConnectionSettings> connectionSettings)
        {
            _connectionSettings = connectionSettings.Value;
            _logger = logger;
            
            ClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(_connectionSettings.AutoReconnectDelaySeconds))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(_connectionSettings.ClientID)
                    .WithTcpServer(_connectionSettings.BrokerURL)
                    .WithCredentials(_connectionSettings.Username, _connectionSettings.Password)
                    .WithCleanSession()
                    .Build()
                ).Build();

            MqttClient = new MqttFactory().CreateManagedMqttClient();
        }
    }
}