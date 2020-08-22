using MQTTClient.Mqtt.Payloads;

namespace MQTTClient.Mqtt.Messages
{
    public class StringMessage:MqttMessage
    {
        public StringMessage (string topic, StringPayload payload)
        {
            Topic = topic;
            Payload = payload;
        }
    }
}