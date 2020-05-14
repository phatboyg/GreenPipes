namespace GreenPipes
{
    using System;
    using System.Linq;
    using Configurators;
    using Policies;
    using Specifications;


    public static class RetryConfigurationExtensions
    {
        public static void UseRetry<T>(this IPipeConfigurator<T> configurator, Action<IRetryConfigurator> configure)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new RetryPipeSpecification<T>();

            configure?.Invoke(specification);

            configurator.AddPipeSpecification(specification);
        }

        /// <summary>
        /// Create an immediate retry policy with the specified number of retries, with no
        /// delay between attempts.
        /// </summary>
        /// <param name="configurator"></param>
        /// <returns></returns>
        public static IRetryConfigurator None(this IRetryConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new NoRetryPolicy(filter));

            return configurator;
        }

        /// <summary>
        /// Create an immediate retry policy with the specified number of retries, with no
        /// delay between attempts.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="retryLimit">The number of retries to attempt</param>
        /// <returns></returns>
        public static IRetryConfigurator Immediate(this IRetryConfigurator configurator, int retryLimit)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new ImmediateRetryPolicy(filter, retryLimit));

            return configurator;
        }

        /// <summary>
        /// Create an interval retry policy with the specified intervals. The retry count equals
        /// the number of intervals provided
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="intervals">The intervals before each subsequent retry attempt</param>
        /// <returns></returns>
        public static IRetryConfigurator Intervals(this IRetryConfigurator configurator, params TimeSpan[] intervals)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new IntervalRetryPolicy(filter, intervals));

            return configurator;
        }

        /// <summary>
        /// Create an interval retry policy with the specified intervals. The retry count equals
        /// the number of intervals provided
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="intervals">The intervals before each subsequent retry attempt</param>
        /// <returns></returns>
        public static IRetryConfigurator Intervals(this IRetryConfigurator configurator, params int[] intervals)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new IntervalRetryPolicy(filter, intervals));

            return configurator;
        }

        /// <summary>
        /// Create an interval retry policy with the specified number of retries at a fixed interval
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="interval">The interval between each retry attempt</param>
        /// <returns></returns>
        public static IRetryConfigurator Interval(this IRetryConfigurator configurator, int retryCount, TimeSpan interval)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new IntervalRetryPolicy(filter, Enumerable.Repeat(interval, retryCount).ToArray()));

            return configurator;
        }

        /// <summary>
        /// Create an interval retry policy with the specified number of retries at a fixed interval
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="interval">The interval between each retry attempt</param>
        /// <returns></returns>
        public static IRetryConfigurator Interval(this IRetryConfigurator configurator, int retryCount, int interval)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new IntervalRetryPolicy(filter, Enumerable.Repeat(interval, retryCount).ToArray()));

            return configurator;
        }

        /// <summary>
        /// Create an exponential retry policy with the specified number of retries at exponential
        /// intervals
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="retryLimit"></param>
        /// <param name="minInterval"></param>
        /// <param name="maxInterval"></param>
        /// <param name="intervalDelta"></param>
        /// <returns></returns>
        public static IRetryConfigurator Exponential(this IRetryConfigurator configurator, int retryLimit, TimeSpan minInterval, TimeSpan maxInterval,
            TimeSpan intervalDelta)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new ExponentialRetryPolicy(filter, retryLimit, minInterval, maxInterval, intervalDelta));

            return configurator;
        }

        /// <summary>
        /// Create an incremental retry policy with the specified number of retry attempts with an incrementing
        /// interval between retries
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="retryLimit">The number of retry attempts</param>
        /// <param name="initialInterval">The initial retry interval</param>
        /// <param name="intervalIncrement">The interval to add to the retry interval with each subsequent retry</param>
        /// <returns></returns>
        public static IRetryConfigurator Incremental(this IRetryConfigurator configurator, int retryLimit, TimeSpan initialInterval, TimeSpan intervalIncrement)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRetryPolicy(filter => new IncrementalRetryPolicy(filter, retryLimit, initialInterval, intervalIncrement));

            return configurator;
        }
    }
}
