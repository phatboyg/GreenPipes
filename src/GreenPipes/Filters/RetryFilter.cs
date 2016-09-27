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
namespace GreenPipes.Filters
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Observers;


    /// <summary>
    /// Uses a retry policy to handle exceptions, retrying the operation in according
    /// with the policy
    /// </summary>
    public class RetryFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly RetryObservable _observers;
        readonly IRetryPolicy _retryPolicy;

        public RetryFilter(IRetryPolicy retryPolicy, RetryObservable observers)
        {
            _retryPolicy = retryPolicy;
            _observers = observers;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("retry");

            _retryPolicy.Probe(scope);
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        async Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            RetryPolicyContext<TContext> policyContext = _retryPolicy.CreatePolicyContext(context);

            await _observers.PostCreate(policyContext).ConfigureAwait(false);

            try
            {
                await next.Send(policyContext.Context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                RetryContext<TContext> payloadRetryContext;
                if (context.TryGetPayload(out payloadRetryContext))
                {
                    await policyContext.RetryFaulted(exception).ConfigureAwait(false);

                    await _observers.RetryFault(payloadRetryContext).ConfigureAwait(false);

                    throw;
                }

                RetryContext genericRetryContext;
                if (context.TryGetPayload(out genericRetryContext))
                {
                    await policyContext.RetryFaulted(exception).ConfigureAwait(false);

                    await _observers.RetryFault(genericRetryContext).ConfigureAwait(false);

                    throw;
                }

                RetryContext<TContext> retryContext;
                if (!policyContext.CanRetry(exception, out retryContext))
                {
                    await retryContext.RetryFaulted(exception).ConfigureAwait(false);

                    await _observers.RetryFault(retryContext).ConfigureAwait(false);

                    context.GetOrAddPayload(() => retryContext);
                    throw;
                }

                await _observers.PostFault(retryContext).ConfigureAwait(false);

                if (retryContext.Delay.HasValue)
                    await Task.Delay(retryContext.Delay.Value).ConfigureAwait(false);

                await Attempt(retryContext, next).ConfigureAwait(false);
            }
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        async Task Attempt(RetryContext<TContext> retryContext, IPipe<TContext> next)
        {
            await retryContext.PreRetry().ConfigureAwait(false);

            await _observers.PreRetry(retryContext).ConfigureAwait(false);

            try
            {
                await next.Send(retryContext.Context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                RetryContext<TContext> payloadRetryContext;
                if (retryContext.Context.TryGetPayload(out payloadRetryContext))
                {
                    await retryContext.RetryFaulted(exception).ConfigureAwait(false);

                    await _observers.RetryFault(payloadRetryContext).ConfigureAwait(false);

                    throw;
                }

                RetryContext genericRetryContext;
                if (retryContext.Context.TryGetPayload(out genericRetryContext))
                {
                    await retryContext.RetryFaulted(exception).ConfigureAwait(false);

                    await _observers.RetryFault(genericRetryContext).ConfigureAwait(false);

                    throw;
                }

                RetryContext<TContext> nextRetryContext;
                if (!retryContext.CanRetry(exception, out nextRetryContext))
                {
                    await nextRetryContext.RetryFaulted(exception).ConfigureAwait(false);

                    await _observers.RetryFault(nextRetryContext).ConfigureAwait(false);

                    retryContext.Context.GetOrAddPayload(() => nextRetryContext);

                    throw;
                }

                await _observers.PostFault(nextRetryContext).ConfigureAwait(false);

                if (nextRetryContext.Delay.HasValue)
                    await Task.Delay(nextRetryContext.Delay.Value).ConfigureAwait(false);

                await Attempt(nextRetryContext, next).ConfigureAwait(false);
            }
        }
    }
}