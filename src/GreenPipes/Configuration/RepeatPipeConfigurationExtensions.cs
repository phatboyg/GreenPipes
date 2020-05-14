namespace GreenPipes
{
    using System;
    using System.Threading;
    using Specifications;


    public static class RepeatPipeConfigurationExtensions
    {
        /// <summary>
        /// Repeat the subsequent filter pipe until the cancellationToken is cancelled.
        /// </summary>
        /// <typeparam name="T">The pipe type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="cancellationToken">The cancellationToken to cancel the repetition</param>
        public static void UseRepeat<T>(this IPipeConfigurator<T> configurator, CancellationToken cancellationToken)
            where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var pipeBuilderConfigurator = new RepeatPipeSpecification<T>(cancellationToken);

            configurator.AddPipeSpecification(pipeBuilderConfigurator);
        }
    }
}
