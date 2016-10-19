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
namespace GreenPipes.Configurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Internals.Extensions;


    public class PipeValidationContext :
        ValidationContext
    {
        readonly IList<PayloadProvider> _payloadProviders;

        public PipeValidationContext()
        {
            _payloadProviders = new List<PayloadProvider>();
        }

        public ISpecification Current { get; set; }

        public ValidationScope<T> CreateFilterScope<T>(IPipeSpecification<T> specification, Type filterType) where T : class, PipeContext
        {
            return new PipeValidationScope<T>(this, specification, filterType);
        }

        public void ProvidesPayload<T>(PayloadProviderInfo payloadProviderInfo)
        {
            _payloadProviders.Add(new PipePayloadProvider<T>(payloadProviderInfo));
        }

        public IEnumerable<ValidationResult> RequiresPayload<T>()
        {
            PayloadProviderInfo providerInfo;
            if (false == _payloadProviders.Any(x => x.IsProvided<T>(out providerInfo)))
                yield return Current.Failure($"A payload provider for {TypeCache<T>.ShortName} was not found.");
        }
    }
}