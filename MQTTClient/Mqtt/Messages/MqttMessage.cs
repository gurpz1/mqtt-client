using MQTTClient.Mqtt.Messages;
using MQTTClient.Mqtt.Payloads;

namespace MQTTClient.Mqtt
{
    public abstract class MqttMessage :IMqttMessage
    {
        public string Topic { get; protected set; }
        public IPayload Payload { get; protected set; }

        protected MqttMessage() {}
        
        public MqttMessage(string topic, IPayload payload)
        {
            Topic = topic;
            Payload = payload;
        }
        
        public override string ToString()
        {
            return $"{Topic}: {Payload.ToStringJson()}";
        }
    }
}