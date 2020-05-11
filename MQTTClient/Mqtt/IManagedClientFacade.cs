using System;
using MQTTClient.Config;
using MQTTnet.Extensions.ManagedClient;

namespace MQTTClient.Mqtt
{
    public interface IManagedClientFacade: IDisposable
    {
        public ConnectionSettings ConnectionSettings { get; }
        void Start();
        void Stop();

        void Publish(MqttMessage message);

        void OnConnected(Action action);
        
        void OnDisconnected(Action action);

    }
}