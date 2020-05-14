namespace GreenPipes
{
    using System;


    public interface RequestContext :
        PipeContext
    {
        /// <summary>
        /// True if the request has been completed and a result specified
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Attempt to specify a result for the request
        /// </summary>
        /// <typeparam name="T">The result type</typeparam>
        /// <param name="result">The result</param>
        /// <returns>True if the response was accepted, false if a response was already accepted</returns>
        bool TrySetResult<T>(T result)
            where T : class;

        /// <summary>
        /// Specify that the request faulted and will have an exception
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool TrySetException(Exception exception);

        /// <summary>
        /// Specify that the request was cancelled
        /// </summary>
        /// <returns></returns>
        bool TrySetCanceled();
    }


    /// <summary>
    /// The context of a request sent to a pipe
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface RequestContext<out TRequest> :
        RequestContext
        where TRequest : class
    {
        /// <summary>
        /// The request type that was sent to the pipe
        /// </summary>
        TRequest Request { get; }
    }
}
