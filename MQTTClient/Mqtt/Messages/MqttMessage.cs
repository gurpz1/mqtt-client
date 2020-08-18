using MQTTClient.Mqtt.Messages;

namespace MQTTClient.Mqtt
{
    public abstract class MqttMessage:IMqttMessage
    {
        public string Topic { get; protected set; }
        public string Payload { get; protected set; }

        protected MqttMessage()
        {
        }

        public MqttMessage(string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }
        
        public override string ToString()
        {
            return $"{Topic}: {Payload}";
        }
    }
}