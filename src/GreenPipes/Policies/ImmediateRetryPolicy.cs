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
namespace GreenPipes.Policies
{
    using System;


    public class ImmediateRetryPolicy :
        IRetryPolicy
    {
        readonly IExceptionFilter _filter;
        readonly int _retryLimit;

        public ImmediateRetryPolicy(IExceptionFilter filter, int retryLimit)
        {
            _filter = filter;
            _retryLimit = retryLimit;
        }

        public int RetryLimit => _retryLimit;

        void IProbeSite.Probe(ProbeContext context)
        {
            context.Set(new
            {
                Policy = "Immediate",
                Limit = _retryLimit
            });

            _filter.Probe(context);
        }

        RetryPolicyContext<T> IRetryPolicy.CreatePolicyContext<T>(T context)
        {
            return new ImmediateRetryPolicyContext<T>(this, context);
        }

        public bool IsHandled(Exception exception)
        {
            return _filter.Match(exception);
        }
    }
}