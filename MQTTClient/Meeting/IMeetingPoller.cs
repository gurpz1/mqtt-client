namespace MQTTClient.Meeting
{
    public interface IMeetingPoller
    {
        string ApplicationName { get; }
        State State { get; }
    }
}