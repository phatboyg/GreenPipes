namespace GreenPipes
{
    using System.Threading.Tasks;


    public interface IRequestPipe<in TRequest> :
        IProbeSite
        where TRequest : class
    {
        /// <summary>
        /// Send a request to the pipe
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<ResultContext> Send(TRequest context);
    }


    /// <summary>
    /// A request pipe which allows awaiting a specific response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IRequestPipe<in TRequest, TResult> :
        IProbeSite
        where TRequest : class
        where TResult : class
    {
        /// <summary>
        /// Send a request to the pipe
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResultContext<TResult>> Send(TRequest request);
    }
}
