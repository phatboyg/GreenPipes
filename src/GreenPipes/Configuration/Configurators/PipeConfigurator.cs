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
namespace GreenPipes.Configurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;
    using Specifications;
    using Validation;


    public class PipeConfigurator<TContext> :
        IBuildPipeConfigurator<TContext>
        where TContext : class, PipeContext
    {
        readonly List<IPipeSpecification<TContext>> _specifications;

        public PipeConfigurator()
        {
            _specifications = new List<IPipeSpecification<TContext>>();
        }

        IEnumerable<ValidationResult> ISpecification.Validate()
        {
            return _specifications.SelectMany(x => x.Validate());
        }

        void IPipeConfigurator<TContext>.AddPipeSpecification(IPipeSpecification<TContext> specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            _specifications.Add(specification);
        }

        public IPipe<TContext> Build()
        {
            ValidatePipeConfiguration();

            var builder = new PipeBuilder<TContext>();

            foreach (var configurator in _specifications)
                configurator.Apply(builder);

            return builder.Build();
        }

        void ValidatePipeConfiguration()
        {
            IPipeConfigurationResult result = new PipeConfigurationResult(_specifications.SelectMany(x => x.Validate()));
            if (result.ContainsFailure)
                throw new PipeConfigurationException(result.GetMessage("The pipe configuration was invalid"));
        }
    }


    public class PipeConfigurator<TContext, TResult> :
        IBuildPipeConfigurator<TContext, TResult>
        where TContext : class, PipeContext
        where TResult : class
    {
        readonly List<IPipeSpecification<TContext, TResult>> _specifications;
        IPipe<TContext, TResult> _handlerPipe;

        public PipeConfigurator()
        {
            _specifications = new List<IPipeSpecification<TContext, TResult>>();
        }

        IEnumerable<ValidationResult> ISpecification.Validate()
        {
            return _specifications.SelectMany(x => x.Validate());
        }

        void IPipeConfigurator<TContext, TResult>.AddPipeSpecification(IPipeSpecification<TContext, TResult> specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            _specifications.Add(specification);
        }

        public void AddPipeSpecification(IPipeSpecification<TContext> specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            _specifications.Add(new ResultFilterPipeSpecification<TContext, TResult>(specification));
        }

        public void SetHandlerPipe(IPipe<TContext, TResult> handlerPipe)
        {
            _handlerPipe = handlerPipe;
        }

        public IPipe<TContext, TResult> Build()
        {
            ValidatePipeConfiguration();

            var builder = new PipeBuilder<TContext, TResult>(_handlerPipe);

            foreach (var configurator in _specifications)
                configurator.Apply(builder);

            return builder.Build();
        }

        void ValidatePipeConfiguration()
        {
            IPipeConfigurationResult result = new PipeConfigurationResult(Validate());
            if (result.ContainsFailure)
                throw new PipeConfigurationException(result.GetMessage("The pipe configuration was invalid"));
        }

        IEnumerable<ValidationResult> Validate()
        {
            if (_handlerPipe == null)
                yield return this.Failure("HandlerPipe", "must not be null");

            foreach (var result in _specifications.SelectMany(x => x.Validate()))
            {
                yield return result;
            }
            ;
        }
    }
}