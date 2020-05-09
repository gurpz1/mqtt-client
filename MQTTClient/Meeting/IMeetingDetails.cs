using System.ComponentModel;

namespace MQTTClient.Meeting
{
    public interface IMeetingDetails:INotifyPropertyChanged
    {
        State State { get; set; }
    }
}