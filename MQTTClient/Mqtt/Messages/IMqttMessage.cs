namespace MQTTClient.Mqtt.Messages
{
    public interface IMqttMessage
    {
        string Topic { get; }
        string Payload { get; }
    }
}