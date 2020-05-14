namespace GreenPipes
{
    public interface IObserverConnector
    {
        /// <summary>
        /// Connect an observer to the filter and/or pipe
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        ConnectHandle ConnectObserver<T>(IFilterObserver<T> observer)
            where T : class, PipeContext;

        /// <summary>
        /// Connect an observer to the filter and/or pipe
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        ConnectHandle ConnectObserver(IFilterObserver observer);
    }


    public interface IObserverConnector<out TContext>
        where TContext : class, PipeContext
    {
        /// <summary>
        /// Connect an observer to the filter and/or pipe
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        ConnectHandle ConnectObserver(IFilterObserver<TContext> observer);
    }
}
