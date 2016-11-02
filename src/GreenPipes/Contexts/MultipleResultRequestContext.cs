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


    public class MultipleResultRequestContext<TRequest> :
        BasePipeContext,
        RequestContext<TRequest>
        where TRequest : class
    {
        readonly IPipe<ResultContext> _resultPipe;
        readonly TaskCompletionSource<ResultContext> _resultTask;
        ResultContext _resultContext;

        public MultipleResultRequestContext(TRequest request, IPipe<ResultContext> resultPipe)
        {
            _resultPipe = resultPipe;
            Request = request;

            _resultTask = new TaskCompletionSource<ResultContext>();
        }

        public Task<ResultContext> Result => _resultTask.Task;

        public TRequest Request { get; }

        async Task<bool> RequestContext.TrySetResult<T>(T result)
        {
            var resultContext = new PipeResultContext<TRequest, T>(Request, result);

            await _resultPipe.Send(resultContext).ConfigureAwait(false);

            var resultWasSet = _resultTask.TrySetResult(resultContext);
            if (resultWasSet)
                _resultContext = resultContext;

            return resultWasSet;
        }

        public bool TrySetException(Exception exception)
        {
            return _resultTask.TrySetException(exception);
        }

        public bool TrySetCanceled()
        {
            return _resultTask.TrySetCanceled();
        }

        public bool HasResult => _resultContext != null;

        bool RequestContext.TryGetResult<T>(out T result)
        {
            if ((_resultContext != null) && _resultContext.TryGetResult(out result))
                return true;

            result = default(T);
            return false;
        }
    }
}