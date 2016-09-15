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
    using System.Diagnostics;
    using System.Threading.Tasks;


    public class FilterPipe<TContext> :
        IPipe<TContext>
        where TContext : class, PipeContext
    {
        readonly IFilter<TContext> _filter;
        readonly IPipe<TContext> _next;

        public FilterPipe(IFilter<TContext> filter, IPipe<TContext> next)
        {
            _filter = filter;
            _next = next;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);
            _next.Probe(context);
        }

        [DebuggerStepThrough]
        public Task Send(TContext context)
        {
            return _filter.Send(context, _next);
        }
    }


    public class FilterPipe<TContext, TResult> :
        IPipe<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly IFilter<TContext, TResult> _filter;
        readonly IPipe<TContext, TResult> _next;

        public FilterPipe(IFilter<TContext, TResult> filter, IPipe<TContext, TResult> next)
        {
            _filter = filter;
            _next = next;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);
            _next.Probe(context);
        }

        [DebuggerStepThrough]
        public Task<TResult> Send(TContext context)
        {
            return _filter.Send(context, _next);
        }
    }
}