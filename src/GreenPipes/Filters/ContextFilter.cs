// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System;
    using System.Threading.Tasks;


    /// <summary>
    /// A content filter applies a delegate to the message context, and uses the result to either accept the message
    /// or discard it.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ContextFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly Func<TContext, Task<bool>> _filter;

        public ContextFilter(Func<TContext, Task<bool>> filter)
        {
            _filter = filter;
        }

        public async Task Send(TContext context, IPipe<TContext> next)
        {
            var accept = await _filter(context).ConfigureAwait(false);
            if (accept)
            {
                await next.Send(context).ConfigureAwait(false);
            }
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("context");
        }
    }
}