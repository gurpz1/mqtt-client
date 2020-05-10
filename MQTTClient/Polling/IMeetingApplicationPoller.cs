using MQTTClient.Polling.Models;

namespace MQTTClient.Polling
{
    public interface IMeetingApplicationPoller:IApplicationPoller
    {
        IMeetingDetails MeetingDetails { get; }
    }
}