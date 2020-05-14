namespace GreenPipes
{
    /// <summary>
    /// The response context
    /// </summary>
    public interface ResultContext :
        PipeContext
    {
        /// <summary>
        /// Returns the result type specified, if it is available
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetResult<T>()
            where T : class;

        /// <summary>
        /// Returns the result type specified if matched, otherwise returns false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryGetResult<T>(out T result)
            where T : class;
    }


    public interface ResultContext<out TResult> :
        ResultContext
        where TResult : class
    {
        TResult Result { get; }
    }


    /// <summary>
    /// A response context combined a request with the applied response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ResultContext<out TRequest, out TResult> :
        ResultContext<TResult>
        where TRequest : class
        where TResult : class
    {
        TRequest Request { get; }
    }
}
