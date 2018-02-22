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
    using Contexts;


    /// <summary>
    /// Binds a context to the pipe using a <see cref="IPipeContextSource{TSource}"/>.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    public class PipeContextSourceBindFilter<TContext, TSource> :
        IFilter<TContext>
        where TContext : class, PipeContext
        where TSource : class, PipeContext
    {
        readonly IPipe<BindContext<TContext, TSource>> _output;
        readonly IPipeContextSource<TSource, TContext> _source;

        public PipeContextSourceBindFilter(IPipe<BindContext<TContext, TSource>> output, IPipeContextSource<TSource, TContext> source)
        {
            _output = output;
            _source = source;
        }

        async Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            var bindPipe = new BindPipe(context, _output);

            await _source.Send(context, bindPipe).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("bind");
            _output.Probe(scope);
            _source.Probe(scope);
        }


        struct BindPipe :
            IPipe<TSource>
        {
            readonly TContext _context;
            readonly IPipe<BindContext<TContext, TSource>> _output;

            public BindPipe(TContext context, IPipe<BindContext<TContext, TSource>> output)
            {
                _context = context;
                _output = output;
            }

            Task IPipe<TSource>.Send(TSource context)
            {
                var bindContext = new BindContextProxy<TContext, TSource>(_context, context);

                return _output.Send(bindContext);
            }

            void IProbeSite.Probe(ProbeContext context)
            {
            }
        }
    }
}