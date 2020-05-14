namespace GreenPipes.Configurators
{
    /// <summary>
    /// Configure a response pipe, which handles a response from a request pipe
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IResultConfigurator<TRequest, TResult> :
        IPipeConfigurator<ResultContext<TRequest, TResult>>
        where TRequest : class
        where TResult : class
    {
    }
}
