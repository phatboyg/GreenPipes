namespace GreenPipes
{
    using Builders;
    using Validation;


    public static class PipeConfiguratorExtensions
    {
        /// <summary>
        /// Validate the pipe configuration, throwing an exception if any failures are encountered.
        /// </summary>
        /// <param name="configurator"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="PipeConfigurationException"></exception>
        public static void ValidatePipeConfiguration<T>(this IBuildPipeConfigurator<T> configurator)
            where T : class, PipeContext
        {
            IPipeConfigurationResult result = new PipeConfigurationResult(configurator.Validate());
            if (result.ContainsFailure)
                throw new PipeConfigurationException(result.GetMessage("The pipe configuration was invalid"));
        }
    }
}
