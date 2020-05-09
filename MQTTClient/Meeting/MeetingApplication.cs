namespace MQTTClient.Meeting
{
    public class MeetingApplication:IMeetingApplication
    {
        public string ApplicationName { get; }
        public bool IsInstalled { get; }

        public MeetingApplication(string applicationName, bool isInstalled)
        {
            ApplicationName = applicationName;
            IsInstalled = isInstalled;
        }
    }
}