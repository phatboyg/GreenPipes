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


    public class MultipleResultRequestContext<TRequest> :
        BasePipeContext,
        RequestContext<TRequest>
        where TRequest : class
    {
        readonly TaskCompletionSource<ResultContext> _response;
        readonly IPipe<ResultContext> _resultPipe;

        public MultipleResultRequestContext(TRequest request, IPipe<ResultContext> resultPipe)
        {
            _resultPipe = resultPipe;
            Request = request;

            _response = new TaskCompletionSource<ResultContext>();
        }

        public Task<ResultContext> Result => _response.Task;

        public TRequest Request { get; }

        async Task<bool> RequestContext<TRequest>.TrySetResult<T>(T result)
        {
            var resultContext = new PipeResultContext<TRequest, T>(Request, result);

            await _resultPipe.Send(resultContext).ConfigureAwait(false);

            return _response.TrySetResult(resultContext);
        }

        public bool TrySetException(Exception exception)
        {
            return _response.TrySetException(exception);
        }

        public bool TrySetCanceled()
        {
            return _response.TrySetCanceled();
        }
    }
}