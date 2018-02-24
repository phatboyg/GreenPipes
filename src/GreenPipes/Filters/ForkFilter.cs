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
    /// Forks a single pipe into two pipes, which are executed concurrently
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ForkFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly IPipe<TContext> _pipe;

        public ForkFilter(IPipe<TContext> pipe)
        {
            _pipe = pipe;
        }

        Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            return Task.WhenAll(_pipe.Send(context), next.Send(context));
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("fork");
            _pipe.Probe(scope);
        }
    }
}