// Copyright 2012-2018 Chris Patterson
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
    using Util;


    public class MultipleResultRequestContext<TRequest> :
        BasePipeContext,
        RequestContext<TRequest>
        where TRequest : class
    {
        readonly IPipe<ResultContext> _resultPipe;
        readonly TaskCompletionSource<Task<ResultContext>> _resultTask;

        public MultipleResultRequestContext(TRequest request, IPipe<ResultContext> resultPipe)
        {
            _resultPipe = resultPipe;
            Request = request;

            _resultTask = TaskUtil.GetTask<Task<ResultContext>>();
        }

        public Task<ResultContext> Result => GetResult();

        public TRequest Request { get; }

        public bool TrySetResult<T>(T result) where T : class
        {
            var resultContext = new PipeResultContext<TRequest, T>(Request, result);

            var sendTask = _resultPipe.Send(resultContext);

            return _resultTask.TrySetResult(sendTask.ContinueWith(task =>
            {
                if (task.IsCanceled)
                    throw new OperationCanceledException();

                if (task.IsFaulted)
                    throw task.Exception?.InnerException ?? new ArgumentException("The result faulted and could not be set");

                return (ResultContext)resultContext;
            }, TaskContinuationOptions.ExecuteSynchronously));
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

        async Task<ResultContext> GetResult()
        {
            Task<ResultContext> resultTask = await _resultTask.Task.ConfigureAwait(false);

            return await resultTask.ConfigureAwait(false);
        }
    }
}
