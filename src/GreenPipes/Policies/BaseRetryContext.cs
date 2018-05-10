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
    using System.Threading.Tasks;
    using Util;


    public class BaseRetryContext<TContext> :
        RetryContext
        where TContext : class, PipeContext
    {
        protected BaseRetryContext(TContext context, Exception exception, int retryCount, CancellationToken cancellationToken)
        {
            Context = context;
            Exception = exception;
            RetryCount = retryCount;
            CancellationToken = cancellationToken;

            RetryAttempt = retryCount + 1;
        }

        public TContext Context { get; }

        public CancellationToken CancellationToken { get; }

        public Exception Exception { get; }

        public int RetryAttempt { get; }

        public int RetryCount { get; }

        public virtual TimeSpan? Delay => default;

        Type RetryContext.ContextType => typeof(TContext);

        public virtual Task PreRetry()
        {
            return TaskUtil.Completed;
        }

        public virtual Task RetryFaulted(Exception exception)
        {
            return TaskUtil.Completed;
        }
    }
}