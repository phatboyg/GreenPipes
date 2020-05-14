namespace GreenPipes.Contexts
{
    using System;
    using System.Threading.Tasks;
    using Util;


    public class SingleResultRequestContext<TRequest, TResult> :
        BasePipeContext,
        RequestContext<TRequest>
        where TRequest : class
        where TResult : class
    {
        readonly IPipe<ResultContext<TRequest, TResult>> _resultPipe;
        readonly TaskCompletionSource<Task<ResultContext<TResult>>> _resultTask;

        public SingleResultRequestContext(TRequest request, IPipe<ResultContext<TRequest, TResult>> resultPipe)
        {
            _resultPipe = resultPipe;
            Request = request;

            _resultTask = TaskUtil.GetTask<Task<ResultContext<TResult>>>();
        }

        public Task<ResultContext<TResult>> Result => GetResultContext();

        public TRequest Request { get; }

        bool RequestContext.TrySetResult<T>(T result)
        {
            if (typeof(TResult).IsAssignableFrom(typeof(T)))
            {
                if (result == null)
                    return SetResult(null);

                return SetResult(result as TResult);
            }

            return false;
        }

        public bool TrySetException(Exception exception)
        {
            return _resultTask.TrySetException(exception);
        }

        public bool TrySetCanceled()
        {
            return _resultTask.TrySetCanceled();
        }

        public bool IsCompleted => _resultTask.Task.IsCompleted;

        async Task<ResultContext<TResult>> GetResultContext()
        {
            Task<ResultContext<TResult>> resultTask = await _resultTask.Task.ConfigureAwait(false);

            return await resultTask.ConfigureAwait(false);
        }

        bool SetResult(TResult result)
        {
            var resultContext = new PipeResultContext<TRequest, TResult>(Request, result);

            var sendTask = _resultPipe.Send(resultContext);

            return _resultTask.TrySetResult(sendTask.ContinueWith(task =>
            {
                if (task.IsCanceled)
                    throw new OperationCanceledException();

                if (task.IsFaulted)
                    throw task.Exception?.InnerException ?? new ArgumentException("The result faulted and could not be set");

                return (ResultContext<TResult>)resultContext;
            }, TaskContinuationOptions.ExecuteSynchronously));
        }
    }
}
