namespace GreenPipes.Filters
{
    using System.Threading.Tasks;
    using Partitioning;


    public class PartitionFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly IPartitioner<TContext> _partitioner;

        public PartitionFilter(PartitionKeyProvider<TContext> keyProvider, IPartitioner partitioner)
        {
            _partitioner = partitioner.GetPartitioner(keyProvider);
        }

        Task IFilter<TContext>.Send(TContext context, IPipe<TContext> next)
        {
            return _partitioner.Send(context, next);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("partition");
            _partitioner.Probe(scope);
        }
    }
}
