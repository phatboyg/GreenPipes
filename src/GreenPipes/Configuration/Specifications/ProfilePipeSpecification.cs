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
    using System.Collections.Generic;
    using Filters;
    using Filters.Profile;


    public class ProfilePipeSpecification<T> :
        IPipeSpecification<T>
        where T : class, PipeContext
    {
        readonly ReportProfileData _reportProfileData;
        readonly long _trivialThreshold;

        public ProfilePipeSpecification(ReportProfileData reportProfileData, long trivialThreshold)
        {
            _reportProfileData = reportProfileData;
            _trivialThreshold = trivialThreshold;
        }

        void IPipeSpecification<T>.Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new ProfileFilter<T>(_reportProfileData, _trivialThreshold));
        }

        IEnumerable<ValidationResult> ISpecification.Validate()
        {
            if (_reportProfileData == null)
                yield return this.Failure("ReportProfileData", "must not be null");
            if (_trivialThreshold < 0)
                yield return this.Failure("TrivialThreshold", "must not >= 0");
        }
    }
}