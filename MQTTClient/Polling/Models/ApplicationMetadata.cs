namespace MQTTClient.Polling.Models
{
    public class ApplicationMetadata:IApplicationMetadata
    {
        public string ApplicationName { get; }
        public bool IsInstalled { get; }

        public ApplicationMetadata(string applicationName, bool isInstalled)
        {
            ApplicationName = applicationName;
            IsInstalled = isInstalled;
        }
    }
}