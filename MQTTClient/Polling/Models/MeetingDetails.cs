using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MQTTClient.Polling.Models
{
    public class MeetingDetails:IMeetingDetails
    {
        private Availability _availability;
        public Availability Availability
        {
            get { return _availability;}
            set
            {
                _availability = value;
                OnPropertyChanged();
            } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}