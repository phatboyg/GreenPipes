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
namespace GreenPipes.Specifications
{
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using Contexts;
    using Filters;


    public class BindPipeSpecification<TContext, TSource> :
        IPipeSpecification<TContext>,
        IBindConfigurator<TContext, TSource>
        where TContext : class, PipeContext
        where TSource : class, PipeContext
    {
        readonly IPipeConfigurator<TContext> _contextPipeConfigurator;
        readonly IBuildPipeConfigurator<BindContext<TContext, TSource>> _pipeConfigurator;
        readonly IPipeContextSource<TSource, TContext> _source;

        public BindPipeSpecification(IPipeContextSource<TSource, TContext> source)
        {
            _source = source;
            _pipeConfigurator = new PipeConfigurator<BindContext<TContext, TSource>>();
            _contextPipeConfigurator = new ContextPipeConfigurator(_pipeConfigurator);
        }

        IPipeConfigurator<TContext> IBindConfigurator<TContext, TSource>.ContextPipe => _contextPipeConfigurator;

        void IPipeConfigurator<BindContext<TContext, TSource>>.AddPipeSpecification(IPipeSpecification<BindContext<TContext, TSource>> specification)
        {
            _pipeConfigurator.AddPipeSpecification(specification);
        }

        void IPipeSpecification<TContext>.Apply(IPipeBuilder<TContext> builder)
        {
            IPipe<BindContext<TContext, TSource>> pipe = _pipeConfigurator.Build();

            var bindFilter = new PipeContextSourceBindFilter<TContext, TSource>(pipe, _source);

            builder.AddFilter(bindFilter);
        }

        IEnumerable<ValidationResult> ISpecification.Validate()
        {
            if (_source == null)
                yield return this.Failure("PipeContextSource", "must not be null");

            foreach (var result in _pipeConfigurator.Validate())
                yield return result;
        }


        class ContextPipeConfigurator :
            IPipeConfigurator<TContext>
        {
            readonly IPipeConfigurator<BindContext<TContext, TSource>> _configurator;

            public ContextPipeConfigurator(IPipeConfigurator<BindContext<TContext, TSource>> configurator)
            {
                _configurator = configurator;
            }

            public void AddPipeSpecification(IPipeSpecification<TContext> specification)
            {
                BindContext<TContext, TSource> ContextProvider(BindContext<TContext, TSource> input, TContext context)
                {
                    return context as BindContext<TContext, TSource> ?? new BindContextProxy<TContext, TSource>(context, input.SourceContext);
                }

                _configurator.AddPipeSpecification(new SplitFilterPipeSpecification<BindContext<TContext, TSource>, TContext>(specification,
                    ContextProvider,
                    context => context.Context));
            }
        }
    }
}