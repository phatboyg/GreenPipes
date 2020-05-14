namespace GreenPipes
{
    using System;
    using Specifications;


    public static class ConcurrencyLimitConfigurationExtensions
    {
        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="concurrencyLimit">The concurrency limit for the subsequent filters in the pipeline</param>
        /// <param name="router">A control pipe to support runtime adjustment</param>
        public static void UseConcurrencyLimit<T>(this IPipeConfigurator<T> configurator, int concurrencyLimit, IPipeRouter router = null)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new ConcurrencyLimitPipeSpecification<T>(concurrencyLimit, router);

            configurator.AddPipeSpecification(specification);
        }
    }
}
