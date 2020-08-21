﻿using System;
using System.Text;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;
using MQTTClient.Mqtt.Messages;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

namespace MQTTClient.Mqtt
{
    public class MqttClientFacade: IMqttClientFacade
    {
        public ConnectionSettings ConnectionSettings { get; }
        private ILogger<MqttClientFacade> _logger;
        
        private IManagedMqttClientOptions _clientOptions;
        private IManagedMqttClient _mqttClient;

        private MqttApplicationMessage _lwtMessage;

        public MqttClientFacade(ILogger<MqttClientFacade> logger,
            IOptions<ConnectionSettings> connectionSettings)
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
        }

        public void Stop()
        {
            _mqttClient.PublishAsync(_lwtMessage).Wait();
            _mqttClient.StopAsync().Wait();
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

        public void OnMessage(string topic, Action<MqttMessage> action)
        {
            _logger.LogInformation($"Subscribing for messages on {topic}.");
            _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                var rawMessage = e.ApplicationMessage;
                var message = new BasicMessage(rawMessage.Topic, Encoding.UTF8.GetString(rawMessage.Payload));
                _logger.LogDebug($"Received message {message.Topic}: {message.Payload}");
                action(message);
            });
        }

        public void Dispose()
        {
            Stop();
        }
    }
}