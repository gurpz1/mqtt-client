namespace MQTTClient.Config
{
    public interface IConnectionSettings
    {
        string ClientID { get; }
        string BrokerURL { get; }
        int Port { get;  }
        string Username { get; }
        string Password { get; }

        int AutoReconnectDelaySeconds { get; }
    }
}