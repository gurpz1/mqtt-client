using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;

namespace MQTTClient.Mqtt.Handlers
{
    public class CommandTriager: ICommandTriager
    {
        private ILogger<CommandTriager> _logger;
        private string _clientId;
        public string BaseTopic { get; }

        private Dictionary<string, Action<MqttMessage>> _allHandlers;
        
        public CommandTriager(ILogger<CommandTriager> logger,
            IOptions<ConnectionSettings> connectionSettings,
            IEnumerable<ICommandHandler> commandHandlers)
        {
            _logger = logger;
            _clientId = connectionSettings.Value.ClientID;
            _allHandlers = new Dictionary<string, Action<MqttMessage>>();

            BaseTopic = $"cmd/{_clientId}/#";
            foreach (var commandHandler in commandHandlers)
            {
                _allHandlers.Add(commandHandler.CommandTopic, commandHandler.OnReceive);
            }
        }
        public void OnReceive(MqttMessage message)
        {
            if (_allHandlers.ContainsKey(message.Topic))
            {
                _allHandlers[message.Topic](message);
            }
            else
            {
                _logger.LogWarning($"Not sure how to handle {message.Topic}");
            }
        }
    }
}