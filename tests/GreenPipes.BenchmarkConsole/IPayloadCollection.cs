namespace GreenPipes.BenchmarkConsole
{
    /// <summary>
    /// Supports the reading and writing of the property cache
    /// </summary>
    public interface IPayloadCollection :
        IReadOnlyPayloadCollection
    {
        IPayloadCollection Add(IPayloadValue payload);
    }
}
