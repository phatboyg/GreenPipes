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
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;


    /// <summary>
    /// Limits the number of calls through the filter to a specified count per time interval
    /// specified.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RateLimitFilter<T> :
        IFilter<T>,
        IPipe<CommandContext<SetRateLimit>>,
        IDisposable
        where T : class, PipeContext
    {
        readonly TimeSpan _interval;
        readonly SemaphoreSlim _limit;
        int _rateLimit;
        readonly Timer _timer;
        int _count;

        public RateLimitFilter(int rateLimit, TimeSpan interval)
        {
            _rateLimit = rateLimit;
            _interval = interval;
            _limit = new SemaphoreSlim(rateLimit);
            _timer = new Timer(Reset, null, interval, interval);
        }

        public void Dispose()
        {
            _limit?.Dispose();
            _timer?.Dispose();
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("rateLimit");
            scope.Add("limit", _rateLimit);
            scope.Add("available", _limit.CurrentCount);
            scope.Add("interval", _interval);
        }

        [DebuggerNonUserCode]
        public async Task Send(T context, IPipe<T> next)
        {
            await _limit.WaitAsync(context.CancellationToken).ConfigureAwait(false);

            Interlocked.Increment(ref _count);

            await next.Send(context).ConfigureAwait(false);
        }

        public async Task Send(CommandContext<SetRateLimit> context)
        {
            var rateLimit = context.Command.RateLimit;
            if (rateLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(rateLimit), "The rate limit must be >= 1");

            var previousLimit = _rateLimit;
            if (rateLimit > previousLimit)
                _limit.Release(rateLimit - previousLimit);
            else
            {
                for (; previousLimit > rateLimit; previousLimit--)
                    await _limit.WaitAsync().ConfigureAwait(false);
            }

            _rateLimit = rateLimit;
        }

        void Reset(object state)
        {
            var processed = Interlocked.Exchange(ref _count, 0);
            if (processed > 0)
                _limit.Release(processed);
        }
    }
}