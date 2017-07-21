namespace SharePointDeployOTron.Logic
{
    public interface IProcessor
    {
        string TargetWeb { get; }
        string TargetLibrary { get; }
        string User { get; }
        string Password { get; }
    }
}
