namespace ApplicationPoller
{
    public interface IApplication
    {
        string ApplicationName { get; }
        bool IsInstalled { get; }
    }
}