namespace GreenPipes.Partitioning
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Agents;


    public class Partitioner :
        Supervisor,
        IPartitioner
    {
        readonly IHashGenerator _hashGenerator;
        readonly string _id;
        readonly int _partitionCount;
        readonly Partition[] _partitions;

        public Partitioner(int partitionCount, IHashGenerator hashGenerator)
        {
            _id = Guid.NewGuid().ToString("N");

            _partitionCount = partitionCount;
            _hashGenerator = hashGenerator;
            _partitions = Enumerable.Range(0, partitionCount)
                .Select(index => new Partition(index))
                .ToArray();
        }

        IPartitioner<T> IPartitioner.GetPartitioner<T>(PartitionKeyProvider<T> keyProvider)
        {
            return new ContextPartitioner<T>(this, keyProvider);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("partitioner");
            scope.Add("id", _id);
            scope.Add("partitionCount", _partitionCount);

            for (var i = 0; i < _partitions.Length; i++)
                _partitions[i].Probe(scope);
        }

        Task Send<T>(byte[] key, T context, IPipe<T> next)
            where T : class, PipeContext
        {
            var hash = key.Length > 0 ? _hashGenerator.Hash(key) : 0;

            var partitionId = hash % _partitionCount;

            return _partitions[partitionId].Send(context, next);
        }


        class ContextPartitioner<TContext> :
            IPartitioner<TContext>
            where TContext : class, PipeContext
        {
            readonly PartitionKeyProvider<TContext> _keyProvider;
            readonly Partitioner _partitioner;

            public ContextPartitioner(Partitioner partitioner, PartitionKeyProvider<TContext> keyProvider)
            {
                _partitioner = partitioner;
                _keyProvider = keyProvider;
            }

            public Task Send(TContext context, IPipe<TContext> next)
            {
                byte[] key = _keyProvider(context);
                if (key == null)
                    throw new InvalidOperationException("The key cannot be null");

                return _partitioner.Send(key, context, next);
            }

            public void Probe(ProbeContext context)
            {
                _partitioner.Probe(context);
            }

            Task IAgent.Ready => _partitioner.Ready;
            Task IAgent.Completed => _partitioner.Completed;
            CancellationToken IAgent.Stopping => _partitioner.Stopping;
            CancellationToken IAgent.Stopped => _partitioner.Stopped;

            Task IAgent.Stop(StopContext context)
            {
                return _partitioner.Stop(context);
            }

            int ISupervisor.PeakActiveCount => _partitioner.PeakActiveCount;
            long ISupervisor.TotalCount => _partitioner.TotalCount;

            void ISupervisor.Add(IAgent agent)
            {
                _partitioner.Add(agent);
            }
        }
    }
}
