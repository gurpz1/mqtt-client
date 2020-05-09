using MQTTClient.Meeting;

namespace MQTTClient.Mqtt
{
    public class MeetingMessage:MqttMessage
    {
        public MeetingMessage(IMeetingApplication meetingApplication, IMeetingDetails meetingDetails) : base(Activity.Meeting)
        {
            
        }
    }
}