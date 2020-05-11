using Microsoft.Extensions.Logging;

namespace ApplicationPoller.Meeting
{
    public abstract class MeetingApplicationPoller:ApplicationPoller, IMeetingApplicationPoller
    {
        public IMeetingDetails MeetingDetails { get; }
        
        protected MeetingApplicationPoller(ILogger logger, string applicationName, int pollingFrequency, Availability initialMeetingAvailability) : base(logger, applicationName, pollingFrequency)
        {
            MeetingDetails = new MeetingDetails()
            {
                Availability = initialMeetingAvailability
            };
        }

        public override void Dispose()
        {
            _logger.LogInformation($"Stopping {Application.ApplicationName} polling");
            if (Application.IsInstalled) MeetingDetails.Availability = Availability.FREE;
            StopPolling();
        }
    }
}