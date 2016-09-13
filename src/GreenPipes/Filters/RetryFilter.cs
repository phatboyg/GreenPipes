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
namespace GreenPipes.Filters
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;


    /// <summary>
    /// Uses a retry policy to handle exceptions, retrying the operation in according
    /// with the policy
    /// </summary>
    public class RetryFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly IRetryPolicy _retryPolicy;

        public RetryFilter(IRetryPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy;
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

            try
            {
                await next.Send(policyContext.Context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                RetryContext<TContext> payloadRetryContext;
                if (context.TryGetPayload(out payloadRetryContext) && !payloadRetryContext.CanRetry(exception, out payloadRetryContext))
                {
                    await policyContext.RetryFaulted(exception).ConfigureAwait(false);
                    throw;
                }

                RetryContext<TContext> retryContext;
                if (!policyContext.CanRetry(exception, out retryContext))
                {
                    await retryContext.RetryFaulted(exception).ConfigureAwait(false);

                    context.GetOrAddPayload(() => retryContext);
                    throw;
                }

                await Attempt(retryContext, next).ConfigureAwait(false);
            }
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        async Task Attempt(RetryContext<TContext> retryContext, IPipe<TContext> next)
        {
            await retryContext.PreRetry().ConfigureAwait(false);

            try
            {
                await next.Send(retryContext.Context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                RetryContext<TContext> payloadRetryContext;
                if (retryContext.Context.TryGetPayload(out payloadRetryContext) && !payloadRetryContext.CanRetry(exception, out payloadRetryContext))
                {
                    await retryContext.RetryFaulted(exception).ConfigureAwait(false);
                    throw;
                }

                RetryContext<TContext> nextRetryContext;
                if (!retryContext.CanRetry(exception, out nextRetryContext))
                {
                    await nextRetryContext.RetryFaulted(exception).ConfigureAwait(false);

                    retryContext.Context.GetOrAddPayload(() => nextRetryContext);
                    throw;
                }

                if (nextRetryContext.Delay.HasValue)
                    await Task.Delay(nextRetryContext.Delay.Value).ConfigureAwait(false);

                await Attempt(nextRetryContext, next).ConfigureAwait(false);
            }
        }
    }
}