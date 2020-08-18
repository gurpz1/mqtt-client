using System.Collections.Generic;
using AuraLight.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;
using MQTTClient.Mqtt.Messages;

namespace MQTTClient.Mqtt.Handlers
{
    public class AuraCommandHandler: ICommandHandler
    {
        private ILogger<AuraCommandHandler> _logger;
        private string _clientId;
        
        private IList<LED> _led;
        
        public string CommandTopic { get; }

        public AuraCommandHandler(ILogger<AuraCommandHandler> logger,
            IOptions<ConnectionSettings> connectionSettings)
        {
            _logger = logger;
            _clientId = connectionSettings.Value.ClientID;
            CommandTopic = $"cmd/{_clientId}/aura";
        }
        
        public void OnReceive(MqttMessage message)
        {
            _logger.LogDebug($"Received: {message}");
        }
    }
}