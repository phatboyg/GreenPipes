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
namespace GreenPipes.Configurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;


    public class PipeConfigurator<TContext> :
        IBuildPipeConfigurator<TContext>
        where TContext : class, PipeContext
    {
        readonly List<IPipeSpecification<TContext>> _specifications;

        public PipeConfigurator()
        {
            _specifications = new List<IPipeSpecification<TContext>>(4);
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
            var builder = new PipeBuilder<TContext>(_specifications.Count);

            var count = _specifications.Count;
            for (var index = 0; index < count; index++)
                _specifications[index].Apply(builder);

            return builder.Build();
        }
    }
}
