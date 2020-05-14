namespace GreenPipes.Filters
{
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// Retains the last value that was sent through the filter, usable as a source to a join pipe
    /// </summary>
    public class LatestFilter<T> :
        IFilter<T>,
        ILatestFilter<T>
        where T : class, PipeContext
    {
        readonly TaskCompletionSource<bool> _hasValue;
        T _latest;

        public LatestFilter()
        {
            _hasValue = TaskUtil.GetTask();
        }

        Task IFilter<T>.Send(T context, IPipe<T> next)
        {
            _latest = context;
            _hasValue.SetCompleted();

            return next.Send(context);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.CreateFilterScope("latest");
        }

        Task<T> ILatestFilter<T>.Latest => GetLatest();

        async Task<T> GetLatest()
        {
            await _hasValue.Task.ConfigureAwait(false);

            return _latest;
        }
    }
}
