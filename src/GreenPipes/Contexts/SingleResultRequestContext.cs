// Copyright 2012-2016 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.Contexts
{
    using System;
    using System.Threading.Tasks;


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

            _resultTask = new TaskCompletionSource<Task<ResultContext<TResult>>>();
        }

        public Task<ResultContext<TResult>> Result => _resultTask.Task.Unwrap();

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

        public bool HasResult => _resultTask.Task.IsCompleted;

        bool RequestContext.TryGetResult<T>(out T result)
        {
            if (HasResult && _resultTask.Task.Result.Result.TryGetResult(out result))
                return true;

            result = default(T);
            return false;
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