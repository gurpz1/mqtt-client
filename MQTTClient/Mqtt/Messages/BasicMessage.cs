namespace MQTTClient.Mqtt.Messages
{
    public class BasicMessage:MqttMessage
    {
        public BasicMessage (string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }
    }
}