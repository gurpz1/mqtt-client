namespace MQTTClient.Config
{
    public class MeetingApplicationSettings:IMeetingApplicationSettings
    {
        public int PollingFrequencySeconds { get; set; } = 30;
    }
}