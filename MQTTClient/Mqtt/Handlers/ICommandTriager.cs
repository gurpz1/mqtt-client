namespace MQTTClient.Mqtt.Handlers
{
    public interface ICommandTriager
    {
        string BaseTopic { get; }
        void OnReceive(MqttMessage message);
    }
}