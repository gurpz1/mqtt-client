namespace MQTTClient.Polling.Models
{
    public interface IApplicationMetadata
    {
        string ApplicationName { get; }
        bool IsInstalled { get; }
    }
}