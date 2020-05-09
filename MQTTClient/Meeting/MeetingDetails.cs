﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MQTTClient.Meeting
{
    public class MeetingDetails:IMeetingDetails
    {
        private State _state;
        public State State
        {
            get { return _state;}
            set
            {
                _state = value;
                OnPropertyChanged();
            } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}