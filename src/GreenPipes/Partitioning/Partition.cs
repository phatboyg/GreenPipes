namespace GreenPipes.Partitioning
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Agents;


    public class Partition :
        Agent,
        IDisposable
    {
        readonly int _index;
        readonly SemaphoreSlim _limit;
        long _attemptCount;
        long _failureCount;
        long _successCount;

        public Partition(int index)
        {
            _index = index;
            _limit = new SemaphoreSlim(1);
        }

        public void Dispose()
        {
            _limit.Dispose();
        }

        public void Probe(ProbeContext context)
        {
            var partitionScope = context.CreateScope($"partition-{_index}");
            partitionScope.Set(new
            {
                AttemptCount = _attemptCount,
                SuccessCount = _successCount,
                FailureCount = _failureCount
            });
        }

        public async Task Send<T>(T context, IPipe<T> next)
            where T : class, PipeContext
        {
            await _limit.WaitAsync(context.CancellationToken).ConfigureAwait(false);

            try
            {
                Interlocked.Increment(ref _attemptCount);

                await next.Send(context).ConfigureAwait(false);

                Interlocked.Increment(ref _successCount);
            }
            catch
            {
                Interlocked.Increment(ref _failureCount);
                throw;
            }
            finally
            {
                _limit.Release();
            }
        }

        protected override async Task StopAgent(StopContext context)
        {
            await _limit.WaitAsync(context.CancellationToken).ConfigureAwait(false);

            await base.StopAgent(context).ConfigureAwait(false);

            _limit.Release(1);
        }
    }
}
