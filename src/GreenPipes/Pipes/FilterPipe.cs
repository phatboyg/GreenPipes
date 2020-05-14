namespace GreenPipes.Pipes
{
    using System.Diagnostics;
    using System.Threading.Tasks;


    public class FilterPipe<TContext> :
        IPipe<TContext>
        where TContext : class, PipeContext
    {
        readonly IFilter<TContext> _filter;
        readonly IPipe<TContext> _next;

        public FilterPipe(IFilter<TContext> filter, IPipe<TContext> next)
        {
            _filter = filter;
            _next = next;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);
            _next.Probe(context);
        }

        [DebuggerStepThrough]
        Task IPipe<TContext>.Send(TContext context)
        {
            return _filter.Send(context, _next);
        }
    }
}
