namespace ApplicationPoller
{
    public class Application:IApplication
    {
        public string ApplicationName { get; }
        public bool IsInstalled { get; }

        public Application(string applicationName, bool isInstalled)
        {
            ApplicationName = applicationName;
            IsInstalled = isInstalled;
        }
    }
}