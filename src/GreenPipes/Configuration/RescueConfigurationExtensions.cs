namespace GreenPipes
{
    using System;
    using Configurators;
    using Filters;
    using Specifications;


    public static class RescueConfigurationExtensions
    {
        /// <summary>
        /// Rescue exceptions via the alternate pipe
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TRescue"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="rescuePipe"></param>
        /// <param name="rescueContextFactory">Factory method to convert the pipe context to the rescue pipe context</param>
        /// <param name="configure"></param>
        public static void UseRescue<TContext, TRescue>(this IPipeConfigurator<TContext> configurator, IPipe<TRescue> rescuePipe,
            RescueContextFactory<TContext, TRescue> rescueContextFactory, Action<IRescueConfigurator<TContext, TRescue>> configure = null)
            where TContext : class, PipeContext
            where TRescue : class, TContext
        {
            UseRescue(configurator, rescueContextFactory, x =>
            {
                configure?.Invoke(x);

                x.UseFork(rescuePipe);
            });
        }

        /// <summary>
        /// Adds a filter to the pipe which is of a different type than the native pipe context type
        /// </summary>
        /// <typeparam name="TContext">The context type</typeparam>
        /// <typeparam name="TRescue">The filter context type</typeparam>
        /// <param name="configurator">The pipe configurator</param>
        /// <param name="rescueContextFactory"></param>
        /// <param name="configure"></param>
        public static void UseRescue<TContext, TRescue>(this IPipeConfigurator<TContext> configurator,
            RescueContextFactory<TContext, TRescue> rescueContextFactory, Action<IRescueConfigurator<TContext, TRescue>> configure = null)
            where TContext : class, PipeContext
            where TRescue : class, TContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new RescuePipeSpecification<TContext, TRescue>(rescueContextFactory);

            configure?.Invoke(specification);

            configurator.AddPipeSpecification(specification);
        }
    }
}
