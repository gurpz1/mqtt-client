namespace MQTTClient.Mqtt.Payloads
{
    public class StringPayload:IPayload
    {
        private string Payload { get; }
        
        public StringPayload(string payload)
        {
            Payload = payload;
        }
        public string ToStringJson()
        {
            return Payload;
        }
    }
}