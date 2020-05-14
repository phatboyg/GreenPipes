namespace GreenPipes.Policies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class ExponentialRetryPolicy :
        IRetryPolicy
    {
        readonly IExceptionFilter _filter;
        readonly int _highInterval;
        readonly int _lowInterval;
        readonly int _maxInterval;
        readonly int _minInterval;
        readonly int _retryLimit;

        public ExponentialRetryPolicy(IExceptionFilter filter, int retryLimit, TimeSpan minInterval, TimeSpan maxInterval, TimeSpan intervalDelta)
        {
            _filter = filter;
            _retryLimit = retryLimit;
            _minInterval = (int)minInterval.TotalMilliseconds;
            _maxInterval = (int)maxInterval.TotalMilliseconds;

            _lowInterval = (int)(intervalDelta.TotalMilliseconds * 0.8);
            _highInterval = (int)(intervalDelta.TotalMilliseconds * 1.2);

            Intervals = GetIntervals().ToArray();
        }

        public TimeSpan[] Intervals { get; }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.Set(new
            {
                Policy = "Exponential",
                Limit = _retryLimit,
                Min = _minInterval,
                Max = _maxInterval,
                Low = _lowInterval,
                High = _highInterval
            });

            _filter.Probe(context);
        }

        RetryPolicyContext<T> IRetryPolicy.CreatePolicyContext<T>(T context)
        {
            return new ExponentialRetryPolicyContext<T>(this, context);
        }

        public bool IsHandled(Exception exception)
        {
            return _filter.Match(exception);
        }

        IEnumerable<TimeSpan> GetIntervals()
        {
            var random = new Random();

            for (var i = 0; _retryLimit == int.MaxValue || i < _retryLimit; i++)
            {
                var delta = (int)Math.Min(_minInterval + Math.Pow(2, i) * random.Next(_lowInterval, _highInterval), _maxInterval);

                yield return TimeSpan.FromMilliseconds(delta);
            }
        }

        public override string ToString()
        {
            return $"Exponential (limit {_retryLimit}, min {_minInterval}ms, max {_maxInterval}ms";
        }
    }
}
