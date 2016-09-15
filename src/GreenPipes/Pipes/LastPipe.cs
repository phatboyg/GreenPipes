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
    using Util;


    /// <summary>
    /// The last pipe in a pipeline is always an end pipe that does nothing and returns synchronously
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class LastPipe<TContext> :
        IPipe<TContext>
        where TContext : class, PipeContext
    {
        readonly IFilter<TContext> _filter;

        public LastPipe(IFilter<TContext> filter)
        {
            _filter = filter;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);
        }

        [DebuggerStepThrough]
        public Task Send(TContext context)
        {
            return _filter.Send(context, Cache.LastPipe);
        }


        static class Cache
        {
            internal static readonly IPipe<TContext> LastPipe = new Last();
        }


        class Last :
            IPipe<TContext>
        {
            void IProbeSite.Probe(ProbeContext context)
            {
            }

            Task IPipe<TContext>.Send(TContext context)
            {
                return TaskUtil.Completed;
            }
        }
    }


    /// <summary>
    /// The last pipe in a pipeline is always an end pipe that does nothing and returns synchronously
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class LastPipe<TContext, TResult> :
        IPipe<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly IFilter<TContext, TResult> _filter;
        readonly IPipe<TContext, TResult> _resultPipe;

        public LastPipe(IFilter<TContext, TResult> filter, IPipe<TContext, TResult> resultPipe)
        {
            _filter = filter;
            _resultPipe = resultPipe;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);
        }

        [DebuggerStepThrough]
        public Task<TResult> Send(TContext context)
        {
            return _filter.Send(context, _resultPipe);
        }
    }
}