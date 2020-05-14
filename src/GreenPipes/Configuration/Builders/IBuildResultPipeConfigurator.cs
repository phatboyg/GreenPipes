namespace GreenPipes.Builders
{
    public interface IBuildResultPipeConfigurator<TRequest, TResult> :
        IPipeConfigurator<ResultContext<TRequest, TResult>>,
        ISpecification
        where TRequest : class
        where TResult : class
    {
        /// <summary>
        /// Builds the pipe, applying any initial specifications to the front of the pipe
        /// </summary>
        /// <returns></returns>
        IRequestPipe<TRequest, TResult> Build();
    }
}
