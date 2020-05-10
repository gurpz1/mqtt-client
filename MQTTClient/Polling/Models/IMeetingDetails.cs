using System.ComponentModel;

namespace MQTTClient.Polling.Models
{
    public interface IMeetingDetails:INotifyPropertyChanged
    {
        MeetingState MeetingState { get; set; }
    }
}