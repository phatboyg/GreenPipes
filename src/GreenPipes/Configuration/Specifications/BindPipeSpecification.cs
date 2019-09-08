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


    public class BindPipeSpecification<TLeft, TRight> :
        IPipeSpecification<TLeft>,
        IBindConfigurator<TLeft, TRight>
        where TLeft : class, PipeContext
        where TRight : class, PipeContext
    {
        readonly IPipeConfigurator<TLeft> _contextPipeConfigurator;
        readonly IBuildPipeConfigurator<BindContext<TLeft, TRight>> _pipeConfigurator;
        readonly IPipeContextSource<TRight, TLeft> _source;

        public BindPipeSpecification(IPipeContextSource<TRight, TLeft> source)
        {
            _source = source;
            _pipeConfigurator = new PipeConfigurator<BindContext<TLeft, TRight>>();
            _contextPipeConfigurator = new ContextPipeConfigurator(_pipeConfigurator);
        }

        IPipeConfigurator<TLeft> IBindConfigurator<TLeft, TRight>.ContextPipe => _contextPipeConfigurator;

        void IPipeConfigurator<BindContext<TLeft, TRight>>.AddPipeSpecification(IPipeSpecification<BindContext<TLeft, TRight>> specification)
        {
            _pipeConfigurator.AddPipeSpecification(specification);
        }

        void IPipeSpecification<TLeft>.Apply(IPipeBuilder<TLeft> builder)
        {
            IPipe<BindContext<TLeft, TRight>> pipe = _pipeConfigurator.Build();

            var bindFilter = new PipeContextSourceBindFilter<TLeft, TRight>(pipe, _source);

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
            IPipeConfigurator<TLeft>
        {
            readonly IPipeConfigurator<BindContext<TLeft, TRight>> _configurator;

            public ContextPipeConfigurator(IPipeConfigurator<BindContext<TLeft, TRight>> configurator)
            {
                _configurator = configurator;
            }

            public void AddPipeSpecification(IPipeSpecification<TLeft> specification)
            {
                BindContext<TLeft, TRight> ContextProvider(BindContext<TLeft, TRight> input, TLeft context)
                {
                    return context as BindContext<TLeft, TRight> ?? new BindContextProxy<TLeft, TRight>(context, input.Right);
                }

                _configurator.AddPipeSpecification(new SplitFilterPipeSpecification<BindContext<TLeft, TRight>, TLeft>(specification,
                    ContextProvider,
                    context => context.Left));
            }
        }
    }
}