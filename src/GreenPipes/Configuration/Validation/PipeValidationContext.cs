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
namespace GreenPipes.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configurators;
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

        public ValidationFilterScope CreateFilterScope<T>(IPipeSpecification<T> specification, Type filterType) where T : class, PipeContext
        {
            return new PipeValidationFilterScope<T>(this, specification, filterType);
        }

        public IEnumerable<ValidationResult> ProvidesPayload<T>(PayloadProviderInfo payloadProviderInfo)
        {
            PayloadProviderInfo[] existingProviders = _payloadProviders.SelectMany(x => x.IsProvided<T>()).ToArray();
            if (existingProviders.Any())
                yield return
                    Current.Warning($"The payload type {TypeCache<T>.ShortName} is already provided by {string.Join(", ", existingProviders.Select(x => TypeCache.GetShortName(x.FilterType)))}.");

            _payloadProviders.Add(new PipePayloadProvider<T>(payloadProviderInfo));
        }

        public IEnumerable<ValidationResult> RequiresPayload<T>()
        {
            if (false == _payloadProviders.SelectMany(x => x.IsProvided<T>()).Any())
                yield return Current.Failure($"A payload provider for {TypeCache<T>.ShortName} was not found.");
        }
    }
}