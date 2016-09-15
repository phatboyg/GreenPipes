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
namespace GreenPipes.Filters
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Internals.Extensions;
    using Pipes;


    public class ResultFilter<TContext, TResult> :
        IFilter<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly IFilter<TContext> _filter;

        public ResultFilter(IFilter<TContext> filter)
        {
            _filter = filter;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("result");
            scope.Set(new
            {
                SplitType = TypeNameCache<TResult>.ShortName
            });

            _filter.Probe(scope);
        }

        [DebuggerNonUserCode]
        public async Task<TResult> Send(TContext context, IPipe<TContext, TResult> next)
        {
            var mergePipe = new ResultPipe<TContext, TResult>(next);

            await _filter.Send(context, mergePipe).ConfigureAwait(false);

            return await mergePipe.Result.ConfigureAwait(false);
        }
    }
}