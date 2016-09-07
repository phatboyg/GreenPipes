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
    using System.Threading.Tasks;

    /// <summary>
    /// Forks to a separate pipe before invoking the next filter in the pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ForkFilter<T> :
        IFilter<T>
        where T : class, PipeContext
    {
        readonly IPipe<T> _pipe;

        public ForkFilter(IPipe<T> pipe)
        {
            _pipe = pipe;
        }

        async Task IFilter<T>.Send(T context, IPipe<T> next)
        {
            await _pipe.Send(context).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("fork");
            _pipe.Probe(scope);
        }
    }
}