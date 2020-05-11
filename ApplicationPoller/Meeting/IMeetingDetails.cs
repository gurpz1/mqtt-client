using System.ComponentModel;

namespace ApplicationPoller.Meeting
{
    public interface IMeetingDetails:INotifyPropertyChanged
    {
        Availability Availability { get; set; }
    }
}