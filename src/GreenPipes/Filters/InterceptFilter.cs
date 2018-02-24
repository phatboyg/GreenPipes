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
namespace GreenPipes.Filters
{
    using System.Threading.Tasks;


    /// <summary>
    /// Intercepts the pipe and executes an adjacent pipe prior to executing the next filter in the main pipe
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class InterceptFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly IPipe<TContext> _pipe;

        public InterceptFilter(IPipe<TContext> pipe)
        {
            _pipe = pipe;
        }

        async Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            await _pipe.Send(context).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("wiretap");

            _pipe.Probe(scope);
        }
    }
}