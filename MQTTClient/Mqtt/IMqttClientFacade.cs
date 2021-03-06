﻿using System;
using MQTTClient.Config;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

namespace MQTTClient.Mqtt
{
    public interface IMqttClientFacade: IDisposable
    {
        public ConnectionSettings ConnectionSettings { get; }
        
        void Start();

        void Stop();

        void Publish(MqttMessage message);

        void OnConnected(Action action);
        
        void OnDisconnected(Action action);

        void OnMessage(string topic, Action<MqttMessage> action);

    }
}