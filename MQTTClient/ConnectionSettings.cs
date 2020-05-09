namespace MQTTClient
{
    public class ConnectionSettings:IConnectionSettings
    {
        public string ClientID { get; set; }
        public string BrokerURL { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}