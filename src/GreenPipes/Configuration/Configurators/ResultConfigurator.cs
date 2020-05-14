namespace GreenPipes.Configurators
{
    using System.Collections.Generic;
    using Builders;
    using Pipes;


    /// <summary>
    /// This will become a specification, since the goal is to have everything rally around
    /// the dispatch pipe
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class ResultConfigurator<TRequest, TResult> :
        IResultConfigurator<TRequest, TResult>,
        IBuildResultPipeConfigurator<TRequest, TResult>,
        IBuildPipeConfigurator<ResultContext<TRequest, TResult>>
        where TRequest : class
        where TResult : class
    {
        readonly IPipe<RequestContext> _pipe;
        readonly IBuildPipeConfigurator<ResultContext<TRequest, TResult>> _pipeConfigurator;

        public ResultConfigurator(IPipe<RequestContext> pipe)
        {
            _pipe = pipe;
            _pipeConfigurator = new PipeConfigurator<ResultContext<TRequest, TResult>>();
        }

        IPipe<ResultContext<TRequest, TResult>> IBuildPipeConfigurator<ResultContext<TRequest, TResult>>.Build()
        {
            return _pipeConfigurator.Build();
        }

        public IRequestPipe<TRequest, TResult> Build()
        {
            IPipe<ResultContext<TRequest, TResult>> responsePipe = _pipeConfigurator.Build();

            return new SingleResultRequestPipe<TRequest, TResult>(_pipe, responsePipe);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return _pipeConfigurator.Validate();
        }

        public void AddPipeSpecification(IPipeSpecification<ResultContext<TRequest, TResult>> specification)
        {
            _pipeConfigurator.AddPipeSpecification(specification);
        }
    }
}
