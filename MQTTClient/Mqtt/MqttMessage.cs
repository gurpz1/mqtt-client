namespace MQTTClient.Mqtt
{
    public abstract class MqttMessage
    {
        public Activity Activity { get; }
        public MqttMessage(Activity activity)
        {
            Activity = activity;
        }
    }
}