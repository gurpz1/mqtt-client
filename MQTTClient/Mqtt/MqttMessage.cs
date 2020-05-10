namespace MQTTClient.Mqtt
{
    public abstract class MqttMessage
    {
        public abstract string Topic { get; }
        public Activity Activity { get; }
        protected string _clientId;

        public MqttMessage(Activity activity, string clientId)
        {
            Activity = activity;
            _clientId = clientId;
        }
    }
}