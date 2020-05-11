using ApplicationPoller;
using ApplicationPoller.Meeting;

namespace MQTTClient.Mqtt
{
    public class MqttMessage
    {
        public string Topic { get; }
        public string Payload { get; }

        public MqttMessage(string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }
        
        public override string ToString()
        {
            return $"{Topic}: {Payload}";
        }

        public static MqttMessage GenerateForMeetingStatus(string clientId, IApplication application,
            IMeetingDetails meetingDetails)
        {
            var topic = $"stat/{clientId}/meeting/{application.ApplicationName.ToLower()}";
            var payload = new MeetingMqttMessagePayload()
            {
                Availability = meetingDetails.Availability
            };
            return new MqttMessage(topic, payload.ToJson());
        }
        
    }
}