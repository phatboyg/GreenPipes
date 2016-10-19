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
namespace GreenPipes.Specifications
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using Filters;


    public class BindPipeSpecification<TContext, TTarget> :
        IPipeSpecification<TContext>,
        IBindConfigurator<TContext, TTarget>
        where TContext : class, PipeContext
        where TTarget : class
    {
        readonly IPipeConfigurator<TContext> _contextPipeConfigurator;
        readonly IBuildPipeConfigurator<BindContext<TContext, TTarget>> _pipeConfigurator;
        Func<ITargetFilter<TTarget>> _filterFactory;

        public BindPipeSpecification()
        {
            _pipeConfigurator = new PipeConfigurator<BindContext<TContext, TTarget>>();
            _contextPipeConfigurator = new ContextPipeConfigurator(_pipeConfigurator);
        }

        void IBindConfigurator<TContext, TTarget>.SetTargetFactory(ITargetFactory<TTarget> targetFactory)
        {
            _filterFactory = () => new FactoryTargetFilter<TTarget>(targetFactory);
        }

        IPipeConfigurator<TContext> IBindConfigurator<TContext, TTarget>.ContextPipe => _contextPipeConfigurator;

        void IPipeConfigurator<BindContext<TContext, TTarget>>.AddPipeSpecification(IPipeSpecification<BindContext<TContext, TTarget>> specification)
        {
            _pipeConfigurator.AddPipeSpecification(specification);
        }

        void IPipeSpecification<TContext>.Apply(IPipeBuilder<TContext> builder)
        {
            IPipe<BindContext<TContext, TTarget>> pipe = _pipeConfigurator.Build();

            ITargetFilter<TTarget> targetFilter = _filterFactory();

            var bindFilter = new BindFilter<TContext, TTarget>(pipe, targetFilter);

            builder.AddFilter(bindFilter);
        }

        IEnumerable<ValidationResult> ISpecification.Validate(ValidationContext context)
        {
            if (_filterFactory == null)
                yield return this.Failure("FilterFactory", "must not be null");

            foreach (var result in _pipeConfigurator.Validate(context))
            {
                yield return result;
            }
        }


        class ContextPipeConfigurator :
            IPipeConfigurator<TContext>
        {
            readonly IPipeConfigurator<BindContext<TContext, TTarget>> _configurator;

            public ContextPipeConfigurator(IPipeConfigurator<BindContext<TContext, TTarget>> configurator)
            {
                _configurator = configurator;
            }

            public void AddPipeSpecification(IPipeSpecification<TContext> specification)
            {
                _configurator.AddPipeSpecification(new SplitFilterPipeSpecification<BindContext<TContext, TTarget>, TContext>(specification,
                    (input,context) => context as BindContext<TContext,TTarget>?? new Context(context, input.Target), context => context.Context));
            }


            class Context :
                BasePipeContext,
                BindContext<TContext, TTarget>
            {
                readonly TContext _context;

                public Context(TContext context, TTarget source)
                    : base(context)
                {
                    _context = context;
                    Target = source;
                }

                TContext BindContext<TContext, TTarget>.Context => _context;

                public TTarget Target { get; }
            }
        }
    }
}