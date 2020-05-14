namespace GreenPipes.Filters
{
    public interface IDynamicFilter<TInput> :
        IFilter<TInput>,
        IPipeConnector,
        IObserverConnector
        where TInput : class, PipeContext
    {
    }


    public interface IDynamicFilter<TInput, in TKey> :
        IFilter<TInput>,
        IPipeConnector,
        IKeyPipeConnector<TKey>,
        IObserverConnector
        where TInput : class, PipeContext
    {
    }
}
