using MQTTClient.Polling.Models;

namespace MQTTClient.Mqtt
{
    public class MeetingMessage:MqttMessage
    {
        public override string Topic => ConstructTopic();

        private IApplicationMetadata _applicationMetadata;
        private IMeetingDetails _meetingDetails;
        public MeetingMessage(IApplicationMetadata applicationMetadata, IMeetingDetails meetingDetails, string clientId) 
            : base(Activity.Meeting, clientId)
        {
            _applicationMetadata = applicationMetadata;
            _meetingDetails = meetingDetails;
        }
        
        private string ConstructTopic()
        {
            return $"stat/{_clientId}/{Activity}/{_applicationMetadata.ApplicationName}";
        }
        
        
    }
}