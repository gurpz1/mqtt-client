using System.Collections.Generic;
using AuraLight.Models;
using MQTTClient.Mqtt.Payloads;

namespace MQTTClient.Mqtt.Messages
{
    public class AuraMessage:MqttMessage
    {
        public AuraMessage(string clientId, int deviceId, IList<LED> leds)
        {
            Topic = $"stat/{clientId}/aura/{deviceId}/leds";
            Payload = new AuraMessagePayload(leds);
        }
    }
}