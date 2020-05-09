using MQTTnet;

namespace MQTTClient.Mqtt
{
    public interface IPublisherFacade
    {
        void Connect();
        void Disconnect();
        void SendMessage(MqttApplicationMessage message);
    }
}