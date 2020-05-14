namespace GreenPipes.Configurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;


    public class PipeConfigurator<TContext> :
        IBuildPipeConfigurator<TContext>
        where TContext : class, PipeContext
    {
        readonly List<IPipeSpecification<TContext>> _specifications;

        public PipeConfigurator()
        {
            _specifications = new List<IPipeSpecification<TContext>>(4);
        }

        IEnumerable<ValidationResult> ISpecification.Validate()
        {
            return _specifications.SelectMany(x => x.Validate());
        }

        void IPipeConfigurator<TContext>.AddPipeSpecification(IPipeSpecification<TContext> specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            _specifications.Add(specification);
        }

        public IPipe<TContext> Build()
        {
            var builder = new PipeBuilder<TContext>(_specifications.Count);

            var count = _specifications.Count;
            for (var index = 0; index < count; index++)
                _specifications[index].Apply(builder);

            return builder.Build();
        }
    }
}
