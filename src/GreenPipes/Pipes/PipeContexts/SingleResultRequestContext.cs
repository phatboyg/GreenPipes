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
namespace GreenPipes.Pipes.PipeContexts
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
        readonly TaskCompletionSource<ResultContext<TResult>> _response;
        readonly IPipe<ResultContext<TRequest, TResult>> _resultPipe;

        public SingleResultRequestContext(TRequest request, IPipe<ResultContext<TRequest, TResult>> resultPipe)
        {
            _resultPipe = resultPipe;
            Request = request;

            _response = new TaskCompletionSource<ResultContext<TResult>>();
        }

        public Task<ResultContext<TResult>> Result => _response.Task;

        public TRequest Request { get; }

        Task<bool> RequestContext<TRequest>.TrySetResult<T>(T result)
        {
            var self = this as SingleResultRequestContext<TRequest, T>;
            if (self != null)
                return self.SetResult(result);

            return TaskUtil.False;
        }

        public bool TrySetException(Exception exception)
        {
            return _response.TrySetException(exception);
        }

        public bool TrySetCanceled()
        {
            return _response.TrySetCanceled();
        }

        async Task<bool> SetResult(TResult result)
        {
            var resultContext = new PipeResultContext<TRequest, TResult>(Request, result);

            await _resultPipe.Send(resultContext).ConfigureAwait(false);

            return _response.TrySetResult(resultContext);
        }
    }
}