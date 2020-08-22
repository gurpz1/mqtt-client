using ApplicationPoller;
using ApplicationPoller.Meeting;
using MQTTClient.Mqtt.Payloads;

namespace MQTTClient.Mqtt.Messages
{
    public class MeetingStatusMessage:MqttMessage
    {
        public MeetingStatusMessage(string clientId, IApplication application, IMeetingDetails meetingDetails)
        {
            Topic = $"stat/{clientId}/meeting/{application.ApplicationName.ToLower()}";
            Payload = new MeetingStatusMessagePayload(meetingDetails.Availability);
        }
    }
}