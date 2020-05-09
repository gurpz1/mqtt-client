using System;

namespace MQTTClient.Meeting
{
    
    public class NotInstalledException : Exception
    {
        public NotInstalledException(string s):base(s)
        {
        }
        public NotInstalledException(string s, Exception inner):base(s, inner)
        {
        }
    }
}