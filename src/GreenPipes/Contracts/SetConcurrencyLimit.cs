namespace GreenPipes.Contracts
{
    public interface SetConcurrencyLimit
    {
        /// <summary>
        /// The new concurrency limit for the filter
        /// </summary>
        int ConcurrencyLimit { get; }
    }
}
