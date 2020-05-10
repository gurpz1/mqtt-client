using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MQTTClient.Polling.Models
{
    public class MeetingDetails:IMeetingDetails
    {
        private MeetingState _meetingState;
        public MeetingState MeetingState
        {
            get { return _meetingState;}
            set
            {
                _meetingState = value;
                OnPropertyChanged();
            } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}