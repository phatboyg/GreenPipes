// Copyright 2013-2016 Chris Patterson
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
    using Internals.Extensions;


    public class ResultPipe<TContext, TResult> :
        IPipe<TContext>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly IPipe<TContext, TResult> _next;
        readonly TaskCompletionSource<TResult> _source;

        public ResultPipe(IPipe<TContext, TResult> next)
        {
            _next = next;
            _source = new TaskCompletionSource<TResult>();
        }

        public Task<TResult> Result => _source.Task;

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("result");
            scope.Set(new
            {
                InputType = TypeNameCache<TContext>.ShortName
            });

            _next.Probe(scope);
        }

        public async Task Send(TContext context)
        {
            try
            {
                var result = await _next.Send(context).ConfigureAwait(false);

                _source.TrySetResult(result);
            }
            catch (Exception ex)
            {
                _source.TrySetException(ex);

                throw;
            }
        }
    }
}