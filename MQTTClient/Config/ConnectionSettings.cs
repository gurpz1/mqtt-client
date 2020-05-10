using System;

namespace MQTTClient.Config
{
    public class ConnectionSettings:IConnectionSettings
    {
        public string ClientID { get; set; } = Environment.MachineName;
        public string BrokerURL { get; set; }
        public int Port { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public int AutoReconnectDelaySeconds { get; set; } = 30;
    }
}