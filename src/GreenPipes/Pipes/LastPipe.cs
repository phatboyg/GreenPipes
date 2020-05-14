namespace GreenPipes.Pipes
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// The last pipe in a pipeline is always an end pipe that does nothing and returns synchronously
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class LastPipe<TContext> :
        IPipe<TContext>
        where TContext : class, PipeContext
    {
        readonly IFilter<TContext> _filter;

        public LastPipe(IFilter<TContext> filter)
        {
            _filter = filter;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);
        }

        [DebuggerStepThrough]
        public Task Send(TContext context)
        {
            return _filter.Send(context, Cache.LastPipe);
        }


        static class Cache
        {
            internal static readonly IPipe<TContext> LastPipe = new Last();
        }


        class Last :
            IPipe<TContext>
        {
            void IProbeSite.Probe(ProbeContext context)
            {
            }

            Task IPipe<TContext>.Send(TContext context)
            {
                return TaskUtil.Completed;
            }
        }
    }
}
