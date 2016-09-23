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
namespace GreenPipes.Policies
{
    using System;
    using System.Threading.Tasks;
    using Util;


    public class ImmediateRetryContext<TContext> :
        BaseRetryContext,
        RetryContext<TContext>
        where TContext : class, PipeContext
    {
        readonly ImmediateRetryPolicy _policy;

        public ImmediateRetryContext(ImmediateRetryPolicy policy, TContext context, Exception exception, int retryCount)
            : base(context, retryCount)
        {
            _policy = policy;
            Context = context;
            RetryCount = retryCount;
            Exception = exception;
        }

        public TContext Context { get; }

        public Exception Exception { get; }

        public int RetryCount { get; }

        public TimeSpan? Delay => default(TimeSpan?);

        public Task PreRetry()
        {
            return TaskUtil.Completed;
        }

        public Task RetryFaulted(Exception exception)
        {
            return TaskUtil.Completed;
        }

        public bool CanRetry(Exception exception, out RetryContext<TContext> retryContext)
        {
            retryContext = new ImmediateRetryContext<TContext>(_policy, Context, Exception, RetryCount + 1);

            return (RetryCount < _policy.RetryLimit) && _policy.Matches(exception);
        }
    }
}