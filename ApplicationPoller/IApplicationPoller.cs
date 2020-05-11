using System;

namespace ApplicationPoller
{
    public interface IApplicationPoller:IDisposable
    {
        IApplication Application { get; }
        int PollingFrequency { get; }
        void StartPolling();
        void StopPolling();
    }
}