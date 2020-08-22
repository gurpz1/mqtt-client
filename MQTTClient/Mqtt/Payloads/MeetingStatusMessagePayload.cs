using System.Text.Json;
using System.Text.Json.Serialization;

using ApplicationPoller.Meeting;

namespace MQTTClient.Mqtt.Payloads
{
    public class MeetingStatusMessagePayload:IPayload
    {
        public Availability Availability { get;}

        public MeetingStatusMessagePayload(Availability availability)
        {
            Availability = availability;
        }
        
        public string ToStringJson()
        {
            var options = new JsonSerializerOptions(); 
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return  JsonSerializer.Serialize(this, options);
        }
    }
}