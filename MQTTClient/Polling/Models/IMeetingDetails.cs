using System.ComponentModel;

namespace MQTTClient.Polling.Models
{
    public interface IMeetingDetails:INotifyPropertyChanged
    {
        Availability Availability { get; set; }
    }
}