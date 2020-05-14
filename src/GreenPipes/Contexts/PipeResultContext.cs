namespace GreenPipes.Contexts
{
    using System;
    using Internals.Extensions;


    public class PipeResultContext<TRequest, TResult> :
        BasePipeContext,
        ResultContext<TRequest, TResult>
        where TRequest : class
        where TResult : class
    {
        public PipeResultContext(TRequest request, TResult result)
        {
            Request = request;
            Result = result;
        }

        public TResult Result { get; }

        public TRequest Request { get; }

        T ResultContext.GetResult<T>()
        {
            if (Result is T result)
                return result;

            throw new ArgumentException("The result is not assignable to the specified type: " + TypeCache<T>.ShortName);
        }

        bool ResultContext.TryGetResult<T>(out T result)
        {
            result = Result as T;

            return result != default(T);
        }
    }
}
