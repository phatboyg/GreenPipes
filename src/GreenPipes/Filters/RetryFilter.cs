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
    public class RetryFilter<T> :
        IFilter<T>
        where T : class, PipeContext
    {
        readonly RetryPolicy _retryPolicy;

        public RetryFilter(RetryPolicy retryPolicy)
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
        async Task IFilter<T>.Send(T context, IPipe<T> next)
        {
            try
            {
                await next.Send(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                RetryContext retryContext;
                if (context.TryGetPayload(out retryContext))
                    throw;

                var canRetry = await _retryPolicy.CanRetry(exception, out retryContext).ConfigureAwait(false);
                if (!canRetry)
                {
                    context.GetOrAddPayload(() => retryContext);
                    throw;
                }

                await Attempt(retryContext, context, next).ConfigureAwait(false);
            }
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        async Task Attempt(RetryContext retryContext, T context, IPipe<T> next)
        {
            try
            {
                await next.Send(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                RetryContext nextRetryContext;
                if (context.TryGetPayload(out nextRetryContext))
                    throw;

                var canRetry = await retryContext.CanRetry(exception, out nextRetryContext).ConfigureAwait(false);
                if (!canRetry)
                {
                    context.GetOrAddPayload(() => nextRetryContext);
                    throw;
                }

                if (nextRetryContext.Delay.HasValue)
                    await Task.Delay(nextRetryContext.Delay.Value).ConfigureAwait(false);

                await Attempt(nextRetryContext, context, next).ConfigureAwait(false);
            }
        }
    }
}