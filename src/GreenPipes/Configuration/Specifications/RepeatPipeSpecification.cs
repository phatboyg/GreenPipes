namespace GreenPipes.Specifications
{
    using System.Collections.Generic;
    using System.Threading;
    using Filters;


    public class RepeatPipeSpecification<T> :
        IPipeSpecification<T>
        where T : class, PipeContext
    {
        readonly CancellationToken _cancellationToken;

        public RepeatPipeSpecification(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new RepeatFilter<T>(_cancellationToken));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }
    }
}
