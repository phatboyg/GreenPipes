namespace GreenPipes.Pipes
{
    using System.Threading.Tasks;


    /// <summary>
    /// A stack-based pipe used to insert filters into the pipeline without breaking up the order of the pipe
    /// delivery
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InsertPipe<T> :
        IPipe<T>
        where T : class, PipeContext
    {
        readonly IFilter<T> _filter;
        readonly IPipe<T> _next;

        /// <summary>
        /// Create an insert pipe, struct, on the stack, so it can be quickly used and cleaned up
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="next"></param>
        public InsertPipe(IFilter<T> filter, IPipe<T> next)
        {
            _filter = filter;
            _next = next;
        }

        Task IPipe<T>.Send(T context)
        {
            return _filter.Send(context, _next);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _filter.Probe(context);

            _next.Probe(context);
        }
    }
}
