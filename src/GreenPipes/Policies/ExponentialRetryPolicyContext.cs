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
    using System.Threading;


    public class ExponentialRetryPolicyContext<TContext> :
        BaseRetryPolicyContext<TContext>
        where TContext : class, PipeContext
    {
        readonly ExponentialRetryPolicy _policy;

        public ExponentialRetryPolicyContext(ExponentialRetryPolicy policy, TContext context)
            : base(policy, context)
        {
            _policy = policy;
        }

        public override bool CanRetry(Exception exception, out RetryContext<TContext> retryContext)
        {
            retryContext = new NoRetryContext<TContext>(Context, exception, CancellationToken);

            return false;
        }

        protected override RetryContext<TContext> CreateRetryContext(Exception exception, CancellationToken cancellationToken)
        {
            return new ExponentialRetryContext<TContext>(_policy, Context, exception, 0, cancellationToken);
        }
    }
}