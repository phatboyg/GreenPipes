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
namespace GreenPipes.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class ValidationDependency_Specs
    {
        [Test]
        public void Should_fault_if_dependency_not_found()
        {
            Assert.That(() =>
            {
                IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
                {
                    cfg.AddPipeSpecification(new SomeSpecification());
                });
            }, Throws.TypeOf<PipeConfigurationException>());
        }

        [Test]
        public void Should_pass_if_dependency_provided()
        {
            IPipe<InputContext> pipe = Pipe.New<InputContext>(cfg =>
            {
                cfg.AddPipeSpecification(new ProviderSpecification());
                cfg.AddPipeSpecification(new SomeSpecification());
            });
        }


        class ProviderSpecification :
            IPipeSpecification<InputContext>
        {
            public void Apply(IPipeBuilder<InputContext> builder)
            {
            }

            public IEnumerable<ValidationResult> Validate(ValidationContext context)
            {
                ValidationFilterScope scope = context.CreateFilterScope(this, typeof(ProviderFilter));

                return scope.ProvidesPayload<ISpecialPayload>();
            }
        }


        class ProviderFilter :
            IFilter<InputContext>
        {
            public Task Send(InputContext context, IPipe<InputContext> next)
            {
                return next.Send(context);
            }

            public void Probe(ProbeContext context)
            {
            }
        }


        class SomeSpecification :
            IPipeSpecification<InputContext>
        {
            public void Apply(IPipeBuilder<InputContext> builder)
            {
            }

            public IEnumerable<ValidationResult> Validate(ValidationContext context)
            {
                ValidationFilterScope scope = context.CreateFilterScope(this, typeof(SomeFilter));
                foreach (ValidationResult result in scope.RequiresPayload<ISpecialPayload>())
                    yield return result;
            }
        }


        class SomeFilter :
            IFilter<InputContext>
        {
            public Task Send(InputContext context, IPipe<InputContext> next)
            {
                return next.Send(context);
            }

            public void Probe(ProbeContext context)
            {
            }
        }


        interface ISpecialPayload
        {
        }
    }
}