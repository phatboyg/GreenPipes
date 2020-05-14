namespace GreenPipes.Filters
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// Uses a retry policy to handle exceptions, retrying the operation in according
    /// with the policy
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class RepeatFilter<TContext> :
        IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly CancellationToken _cancellationToken;

        public RepeatFilter(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.CreateFilterScope("repeat");
        }

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public async Task Send(TContext context, IPipe<TContext> next)
        {
            while (!_cancellationToken.IsCancellationRequested)
                await next.Send(context).ConfigureAwait(false);
        }
    }
}
