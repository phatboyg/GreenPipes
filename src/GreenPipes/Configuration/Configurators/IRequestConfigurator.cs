namespace GreenPipes.Configurators
{
    using System;


    /// <summary>
    /// Configure a request, specifying the responses and their pipes
    /// </summary>
    public interface IRequestConfigurator
    {
        IRequestPipe<TRequest> Request<TRequest>(params Func<IRequestConfigurator<TRequest>, IRequestPipe<TRequest>>[] configure)
            where TRequest : class;

        /// <summary>
        /// Create a pipe that handles a request with a single response
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="configureRequest"></param>
        /// <returns></returns>
        IRequestPipe<TRequest, TResult> Request<TRequest, TResult>(Action<IResultConfigurator<TRequest, TResult>> configureRequest = null)
            where TRequest : class
            where TResult : class;
    }


    /// <summary>
    /// Configure a request, specifying the responses and their pipes
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IRequestConfigurator<TRequest> :
        IPipeConfigurator<ResultContext>
        where TRequest : class
    {
        /// <summary>
        /// Declares a result for the request which can be set by a service
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="configure">Configure the result pipe</param>
        /// <returns></returns>
        IRequestPipe<TRequest> Result<TResult>(Action<IRequestConfigurator<TRequest, TResult>> configure = null)
            where TResult : class;
    }


    /// <summary>
    /// Configure a response pipe, which handles a response from a request pipe
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IRequestConfigurator<TRequest, TResult> :
        IPipeConfigurator<ResultContext<TRequest, TResult>>
        where TRequest : class
        where TResult : class
    {
    }
}
