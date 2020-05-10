using MQTTnet.Extensions.ManagedClient;

namespace MQTTClient.Mqtt
{
    public interface IManagedClientFacade
    {
        IManagedMqttClient MqttClient { get; }
        string BrokerUrl { get; }

        IManagedMqttClientOptions ClientOptions { get; }
    }
}