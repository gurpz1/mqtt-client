using System;
using System.Text;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace MQTTClient.Mqtt
{
    public class MqttClientFacade: IMqttClientFacade
    {
        public ConnectionSettings ConnectionSettings { get; }
        private ILogger<MqttClientFacade> _logger;
        private IManagedMqttClientOptions _clientOptions;
        private IManagedMqttClient _mqttClient;

        private MqttApplicationMessage _lwtMessage;

        private Timer _onlineMessageSendTimer;
        private int _onlineMessageSendInterval = 5000;
        private MqttApplicationMessage _onlineMessage;
        
        public MqttClientFacade(ILogger<MqttClientFacade> logger, IOptions<ConnectionSettings> connectionSettings)
        {
            ConnectionSettings = connectionSettings.Value;
            _logger = logger;
            
            // Build LWT message
            _lwtMessage = new MqttApplicationMessageBuilder()
                .WithPayload(Encoding.UTF8.GetBytes("Offline"))
                .WithTopic($"stat/{ConnectionSettings.ClientID}/LWT")
                .WithRetainFlag()
                .WithAtLeastOnceQoS()
                .Build();

            // Build Online message
            _onlineMessage = new MqttApplicationMessageBuilder()
                .WithPayload(Encoding.UTF8.GetBytes("Online"))
                .WithTopic($"stat/{ConnectionSettings.ClientID}/Online")
                .WithAtLeastOnceQoS()
                .Build();
            
            // Create Client
            _clientOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(ConnectionSettings.AutoReconnectDelaySeconds))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(ConnectionSettings.ClientID)
                    .WithTcpServer(ConnectionSettings.BrokerURL)
                    .WithWillMessage(_lwtMessage)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(10))
                    .WithCommunicationTimeout(TimeSpan.FromMinutes(5))
                    .WithCredentials(ConnectionSettings.Username, ConnectionSettings.Password)
                    .WithCleanSession()
                    .Build()
                ).Build();

            _mqttClient = new MqttFactory().CreateManagedMqttClient();
        }

        public void Start()
        {
            _logger.LogDebug("Starting up client");
            _mqttClient.StartAsync(_clientOptions).Wait();

            var onStartMessage = new MqttApplicationMessageBuilder()
                .WithPayload(Encoding.UTF8.GetBytes("Online"))
                .WithTopic(_lwtMessage.Topic)
                .WithRetainFlag()
                .WithAtLeastOnceQoS()
                .Build();

            _mqttClient.PublishAsync(onStartMessage);
            
            // When the client is started you want to continuously send an online message
            _logger.LogInformation($"Sending online message every {_onlineMessageSendInterval/1000}s");
            _onlineMessageSendTimer = new Timer(_onlineMessageSendInterval);
            _onlineMessageSendTimer.Elapsed += SendOnlineMessage;
            _onlineMessageSendTimer.AutoReset = true;
            _onlineMessageSendTimer.Enabled = true;
        }

        public void Stop()
        {
            _onlineMessage.Payload = Encoding.UTF8.GetBytes("Offline");
            _mqttClient.PublishAsync(_onlineMessage).Wait();
            _onlineMessageSendTimer.Stop();
            _onlineMessageSendTimer.Dispose();
            
            _mqttClient.PublishAsync(_lwtMessage).Wait();
            _mqttClient.StopAsync();
            _logger.LogDebug("Stopped client");
            _mqttClient.Dispose();
        }

        public void Publish(MqttMessage message)
        {
            MqttApplicationMessage clientMessage = new MqttApplicationMessage();
            clientMessage.QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce;
            
            clientMessage.Payload = Encoding.UTF8.GetBytes(message.Payload);
            clientMessage.Topic = message.Topic;
            
            _mqttClient.PublishAsync(clientMessage);
        }

        public void OnConnected(Action action)
        {
            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(e =>
            {
                _logger.LogInformation($"Connected to {ConnectionSettings.BrokerURL} as {ConnectionSettings.ClientID} with {ConnectionSettings.Username}");
                action();
            });
        }

        public void OnDisconnected(Action action)
        {
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(e =>
            {
                _logger.LogInformation($"Disconnected from {ConnectionSettings.BrokerURL}");
                action();
            });
        }
        private void SendOnlineMessage(object sender, ElapsedEventArgs e)
        {
            _mqttClient.PublishAsync(_onlineMessage);
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}