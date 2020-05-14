namespace GreenPipes.Filters
{
    /// <summary>
    /// Supports adding filters to a pipe at the reserved location
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInsertFilter<T>
        where T : class, PipeContext
    {
        /// <summary>
        /// Add the filter to the pipe
        /// </summary>
        /// <param name="filter"></param>
        void Add(IFilter<T> filter);

        /// <summary>
        /// Insert a filter before the other filters already added/inserted
        /// </summary>
        /// <param name="filter"></param>
        void Insert(IFilter<T> filter);
    }
}
