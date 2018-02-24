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
    using System.Threading.Tasks;
    using Util;


    public class ExponentialRetryPolicyContext<TContext> :
        RetryPolicyContext<TContext>
        where TContext : class, PipeContext
    {
        readonly TContext _context;
        readonly ExponentialRetryPolicy _policy;

        public ExponentialRetryPolicyContext(ExponentialRetryPolicy policy, TContext context)
        {
            _policy = policy;
            _context = context;
        }

        public TContext Context => _context;

        public bool CanRetry(Exception exception, out RetryContext<TContext> retryContext)
        {
            retryContext = new ExponentialRetryContext<TContext>(_policy, _context, exception, 1);

            return _policy.IsHandled(exception);
        }

        public Task RetryFaulted(Exception exception)
        {
            return TaskUtil.Completed;
        }
    }
}