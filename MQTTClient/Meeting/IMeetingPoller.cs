using System;

namespace MQTTClient.Meeting
{
    public interface IMeetingPoller:IDisposable
    {
        IMeetingApplication MeetingApplication { get; }
        IMeetingDetails MeetingDetails { get; }
        int PollingFrequency { get; }
    }
}