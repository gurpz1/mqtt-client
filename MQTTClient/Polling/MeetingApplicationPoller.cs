using Microsoft.Extensions.Logging;
using MQTTClient.Polling.Models;

namespace MQTTClient.Polling
{
    public abstract class MeetingApplicationPoller:ApplicationPoller, IMeetingApplicationPoller
    {
        public IMeetingDetails MeetingDetails { get; }
        
        protected MeetingApplicationPoller(ILogger logger, string applicationName, int pollingFrequency, MeetingState initialMeetingMeetingState) : base(logger, applicationName, pollingFrequency)
        {
            MeetingDetails = new MeetingDetails()
            {
                MeetingState = initialMeetingMeetingState
            };
        }
    }
}