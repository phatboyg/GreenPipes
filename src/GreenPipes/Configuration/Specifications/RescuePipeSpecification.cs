// Copyright 2012-2016 Chris Patterson
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
    using Filters;


    public class RescuePipeSpecification<TContext, TRescue> :
        ExceptionSpecification,
        IPipeSpecification<TContext>,
        IRescueConfigurator<TContext, TRescue>
        where TContext : class, PipeContext
        where TRescue : class, TContext
    {
        readonly IPipeConfigurator<TContext> _contextPipeConfigurator;
        readonly IBuildPipeConfigurator<TRescue> _pipeConfigurator;
        readonly RescueContextFactory<TContext, TRescue> _rescueContextFactory;

        public RescuePipeSpecification(RescueContextFactory<TContext, TRescue> rescueContextFactory)
        {
            _rescueContextFactory = rescueContextFactory;

            _pipeConfigurator = new PipeConfigurator<TRescue>();
            _contextPipeConfigurator = new ContextPipeConfigurator(_pipeConfigurator);
        }

        public void Apply(IPipeBuilder<TContext> builder)
        {
            IPipe<TRescue> rescuePipe = _pipeConfigurator.Build();

            builder.AddFilter(new RescueFilter<TContext, TRescue>(rescuePipe, Filter, _rescueContextFactory));
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (_rescueContextFactory == null)
                yield return this.Failure("RescueContextFactory", "must not be null");

            foreach (var result in _pipeConfigurator.Validate(context))
                yield return result;
        }

        IPipeConfigurator<TContext> IRescueConfigurator<TContext, TRescue>.ContextPipe => _contextPipeConfigurator;

        void IPipeConfigurator<TRescue>.AddPipeSpecification(IPipeSpecification<TRescue> specification)
        {
            _pipeConfigurator.AddPipeSpecification(specification);
        }


        class ContextPipeConfigurator :
            IPipeConfigurator<TContext>
        {
            readonly IPipeConfigurator<TRescue> _configurator;

            public ContextPipeConfigurator(IPipeConfigurator<TRescue> configurator)
            {
                _configurator = configurator;
            }

            public void AddPipeSpecification(IPipeSpecification<TContext> specification)
            {
                _configurator.AddPipeSpecification(new SplitFilterPipeSpecification<TRescue, TContext>(specification,
                    InputContext, Context));
            }

            static TRescue Context(TRescue context)
            {
                return context;
            }

            static TRescue InputContext(TRescue input, TContext context)
            {
                return input;
            }
        }
    }
}