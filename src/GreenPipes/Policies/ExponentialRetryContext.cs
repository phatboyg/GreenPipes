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


    public class ExponentialRetryContext<TContext> :
        BaseRetryContext,
        RetryContext<TContext>
        where TContext : class, PipeContext
    {
        readonly ExponentialRetryPolicy _policy;
        readonly int _retryCount;

        public ExponentialRetryContext(ExponentialRetryPolicy policy, TContext context, Exception exception, int retryCount)
            : base(context, typeof(TContext), retryCount)
        {
            _policy = policy;
            _retryCount = retryCount;
            Context = context;
            Exception = exception;
        }

        public TContext Context { get; }

        public Exception Exception { get; }

        public int RetryCount => _retryCount;

        public TimeSpan? Delay => _policy.Intervals[_retryCount - 1];

        public Task PreRetry()
        {
            return TaskUtil.Completed;
        }

        public Task RetryFaulted(Exception exception)
        {
            return TaskUtil.Completed;
        }

        bool RetryPolicyContext<TContext>.CanRetry(Exception exception, out RetryContext<TContext> retryContext)
        {
            retryContext = new ExponentialRetryContext<TContext>(_policy, Context, Exception, _retryCount + 1);

            return (_retryCount < _policy.Intervals.Length) && _policy.IsHandled(exception);
        }
    }
}