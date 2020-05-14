namespace GreenPipes.Contracts
{
    using System;


    /// <summary>
    /// Profiler data emitted for each occurrence
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ProfileData<out T> :
        ProfileData
        where T : PipeContext
    {
        /// <summary>
        /// The context for the profiled send
        /// </summary>
        T Context { get; }
    }


    /// <summary>
    /// Profiler data emitted for each occurrence
    /// </summary>
    public interface ProfileData
    {
        long Id { get; }
        DateTime Timestamp { get; }
        TimeSpan Elapsed { get; }
    }
}
