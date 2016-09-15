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


    public class FactoryTargetFilter<TTarget> :
        ITargetFilter<TTarget>
        where TTarget : class
    {
        readonly ITargetFactory<TTarget> _targetFactory;

        public FactoryTargetFilter(ITargetFactory<TTarget> targetFactory)
        {
            _targetFactory = targetFactory;
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("source");

            _targetFactory.Probe(scope);

        }

        async Task ITargetFilter<TTarget>.Send<T>(T context, IPipe<BindContext<T, TTarget>> next)
        {
            var source = await _targetFactory.CreateSource(context).ConfigureAwait(false);

            var sourceContext = new Context<T>(context, source);

            await next.Send(sourceContext).ConfigureAwait(false);
        }


        class Context<T> :
            BasePipeContext,
            BindContext<T, TTarget>
            where T : class, PipeContext
        {
            readonly T _context;

            public Context(T context, TTarget source)
                : base(context)
            {
                _context = context;
                Target = source;
            }

            T BindContext<T, TTarget>.Context => _context;

            public TTarget Target { get; }
        }
    }
}