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
namespace GreenPipes.Pipes
{
    using System;
    using System.Threading.Tasks;
    using Contexts;
    using Internals.Extensions;


    /// <summary>
    /// A pipe for a single request with multiple result types
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public class MultipleResultRequestPipe<TRequest> :
        IRequestPipe<TRequest>
        where TRequest : class
    {
        readonly IPipe<RequestContext> _requestPipe;
        readonly IPipe<ResultContext> _resultPipe;

        public MultipleResultRequestPipe(IPipe<RequestContext> requestPipe, IPipe<ResultContext> resultPipe)
        {
            _resultPipe = resultPipe;
            _requestPipe = requestPipe;
        }

        public Task<ResultContext> Send(TRequest request)
        {
            var context = new MultipleResultRequestContext<TRequest>(request, _resultPipe);
            try
            {
                SendRequest(context);

                return context.Result;
            }
            catch (TaskCanceledException)
            {
                context.TrySetCanceled();

                throw;
            }
            catch (Exception ex)
            {
                context.TrySetException(ex);

                throw;
            }
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("request");
            scope.Add("requestType", TypeCache<TRequest>.ShortName);

            _requestPipe.Probe(scope.CreateScope("requestPipe"));
            _resultPipe.Probe(scope.CreateScope("resultPipe"));
        }

        void SendRequest(RequestContext context)
        {
            var send = _requestPipe.Send(context);
            send.ContinueWith(task => context.TrySetCanceled(), TaskContinuationOptions.OnlyOnCanceled);
            send.ContinueWith(task => context.TrySetException(task.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}