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


    public class ConfiguratorPipeConnectorSpecification<TContext> :
        IPipeConfigurator<TContext>,
        IPipeConnectorSpecification
        where TContext : class, PipeContext
    {
        readonly IBuildPipeConfigurator<TContext> _configurator;

        public ConfiguratorPipeConnectorSpecification()
        {
            _configurator = new PipeConfigurator<TContext>();
        }

        public void AddPipeSpecification(IPipeSpecification<TContext> specification)
        {
            _configurator.AddPipeSpecification(specification);
        }

        public void Connect(IPipeConnector connector)
        {
            IPipe<TContext> pipe = _configurator.Build();

            connector.ConnectPipe(pipe);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            return _configurator.Validate(context);
        }
    }
}