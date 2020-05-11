using System.Text.Json;
using System.Text.Json.Serialization;
using MQTTClient.Polling.Models;

namespace MQTTClient.Mqtt
{
    public class MeetingMqttMessagePayload
    {
        public Availability Availability { get; set; }

        public string ToJson()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return JsonSerializer.Serialize(this, options);
        }
    }
    
}