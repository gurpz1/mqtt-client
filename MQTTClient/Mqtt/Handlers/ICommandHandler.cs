namespace MQTTClient.Mqtt.Handlers
{
    public interface ICommandHandler
    {
        string CommandTopic { get; }
        void OnReceive(MqttMessage message);
    }
}