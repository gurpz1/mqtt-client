using MQTTClient.Mqtt.Payloads;

namespace MQTTClient.Mqtt.Messages
{
    public interface IMqttMessage
    {
        string Topic { get; }
        IPayload Payload { get; }
    }
}