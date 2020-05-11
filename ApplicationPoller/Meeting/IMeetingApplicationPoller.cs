namespace ApplicationPoller.Meeting
{
    public interface IMeetingApplicationPoller:IApplicationPoller
    {
        IMeetingDetails MeetingDetails { get; }
    }
}