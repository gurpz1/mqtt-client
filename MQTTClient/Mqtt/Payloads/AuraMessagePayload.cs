using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using AuraLight.Models;

namespace MQTTClient.Mqtt.Payloads
{
    public class AuraMessagePayload:IPayload
    {
        public IList<LED> Leds { get;}
        
        public AuraMessagePayload(IList<LED> leds)
        {
            Leds = leds;
        }

        public string ToStringJson()
        {
            IList<int[]> l = new List<int[]>(Leds.Count);
            foreach (var led in Leds)
            {
                l.Add(new[]{(int)led.R, (int)led.G, (int) led.B});
            }
            var options = new JsonSerializerOptions(); 
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return  JsonSerializer.Serialize(new{Leds=l}, options);
        }
    }
}