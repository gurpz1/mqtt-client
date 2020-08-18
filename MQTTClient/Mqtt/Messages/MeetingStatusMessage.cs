using System.Text.Json;
using System.Text.Json.Serialization;
using ApplicationPoller;
using ApplicationPoller.Meeting;

namespace MQTTClient.Mqtt.Messages
{
    public class MeetingStatusMessage:MqttMessage
    {
        public MeetingStatusMessage(string clientId, IApplication application, IMeetingDetails meetingDetails)
        {
            var payload = new
            {
                Availability = meetingDetails.Availability
            };
            
            var options = new JsonSerializerOptions(); 
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            
            Topic = $"stat/{clientId}/meeting/{application.ApplicationName.ToLower()}";
            Payload = JsonSerializer.Serialize(payload, options);
        }
    }
}