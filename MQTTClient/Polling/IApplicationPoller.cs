using System;
using MQTTClient.Polling.Models;

namespace MQTTClient.Polling
{
    public interface IApplicationPoller:IDisposable
    {
        IApplicationMetadata ApplicationMetadata { get; }
        int PollingFrequency { get; }
        void StartPolling();
        void StopPolling();
    }
}