namespace GreenPipes.Builders
{
    using Configurators;


    public interface IBuildRequestPipeConfigurator<TRequest, TResult> :
        IRequestConfigurator<TRequest, TResult>,
        IPipeConnectorSpecification
        where TRequest : class
        where TResult : class
    {
        /// <summary>
        /// Builds the pipe, applying any initial specifications to the front of the pipe
        /// </summary>
        /// <returns></returns>
        IRequestPipe<TRequest> Build(IPipe<ResultContext> resultPipe);
    }
}
