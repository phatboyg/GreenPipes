namespace GreenPipes
{
    public interface IPipeConfigurationResult
    {
        bool ContainsFailure { get; }
        string GetMessage(string header);
        bool Any();
    }
}
