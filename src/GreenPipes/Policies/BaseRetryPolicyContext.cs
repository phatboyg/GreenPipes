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


    public abstract class BaseRetryPolicyContext<TContext> :
        RetryPolicyContext<TContext>
        where TContext : class, PipeContext
    {
        CancellationTokenSource _cancellationTokenSource;
        readonly IRetryPolicy _policy;
        CancellationTokenRegistration _registration;

        protected BaseRetryPolicyContext(IRetryPolicy policy, TContext context)
        {
            _policy = policy;
            Context = context;
        }

        protected CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CreateCancellationToken();

        public TContext Context { get; }

        public virtual bool CanRetry(Exception exception, out RetryContext<TContext> retryContext)
        {
            retryContext = CreateRetryContext(exception, CancellationToken);

            return _policy.IsHandled(exception) && !_cancellationTokenSource.IsCancellationRequested;
        }

        Task RetryPolicyContext<TContext>.RetryFaulted(Exception exception)
        {
            return TaskUtil.Completed;
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        void IDisposable.Dispose()
        {
            _registration.Dispose();
        }

        protected abstract RetryContext<TContext> CreateRetryContext(Exception exception, CancellationToken cancellationToken);

        CancellationToken CreateCancellationToken()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            if (Context.CancellationToken.CanBeCanceled)
                _registration = Context.CancellationToken.Register(_cancellationTokenSource.Cancel);

            return _cancellationTokenSource.Token;
        }
    }
}