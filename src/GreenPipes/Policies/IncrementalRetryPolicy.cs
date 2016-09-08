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
namespace GreenPipes.Policies
{
    using System;


    public class IncrementalRetryPolicy :
        IRetryPolicy
    {
        readonly IExceptionFilter _filter;
        readonly int _retryLimit;

        public IncrementalRetryPolicy(IExceptionFilter filter, int retryLimit, TimeSpan initialInterval,
            TimeSpan intervalIncrement)
        {
            if (initialInterval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(initialInterval),
                    "The initialInterval must be non-negative or -1, and it must be less than or equal to TimeSpan.MaxValue.");
            }

            if (intervalIncrement < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalIncrement),
                    "The intervalIncrement must be non-negative or -1, and it must be less than or equal to TimeSpan.MaxValue.");
            }

            _filter = filter;
            _retryLimit = retryLimit;
            InitialInterval = initialInterval;
            IntervalIncrement = intervalIncrement;
        }

        public int RetryLimit => _retryLimit;

        public TimeSpan InitialInterval { get; }

        public TimeSpan IntervalIncrement { get; }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.Set(new
            {
                Policy = "Incremental",
                Limit = _retryLimit,
                Initial = InitialInterval,
                Increment = IntervalIncrement
            });

            _filter.Probe(context);
        }

        RetryPolicyContext<T> IRetryPolicy.CreatePolicyContext<T>(T context)
        {
            return new IncrementalRetryPolicyContext<T>(this, context);
        }

        public bool Matches(Exception exception)
        {
            return _filter.Match(exception);
        }
    }
}