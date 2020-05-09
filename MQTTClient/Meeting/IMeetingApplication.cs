namespace MQTTClient.Meeting
{
    public interface IMeetingApplication
    {
        string ApplicationName { get; }
        bool IsInstalled { get; }
    }
}