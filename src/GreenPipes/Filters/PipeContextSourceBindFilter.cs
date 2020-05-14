namespace GreenPipes.Filters
{
    using System.Threading.Tasks;
    using Contexts;
    using Internals.Extensions;


    /// <summary>
    /// Binds a context to the pipe using a <see cref="IPipeContextSource{TSource}"/>.
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    public class PipeContextSourceBindFilter<TLeft, TRight> :
        IFilter<TLeft>
        where TLeft : class, PipeContext
        where TRight : class, PipeContext
    {
        readonly IPipe<BindContext<TLeft, TRight>> _output;
        readonly IPipeContextSource<TRight, TLeft> _source;

        public PipeContextSourceBindFilter(IPipe<BindContext<TLeft, TRight>> output, IPipeContextSource<TRight, TLeft> source)
        {
            _output = output;
            _source = source;
        }

        Task IFilter<TLeft>.Send(TLeft context, IPipe<TLeft> next)
        {
            var bindPipe = new BindPipe(context, _output);

            var sourceTask = _source.Send(context, bindPipe);
            if (sourceTask.IsCompletedSuccessfully())
                return next.Send(context);

            async Task SendAsync()
            {
                await sourceTask.ConfigureAwait(false);

                await next.Send(context).ConfigureAwait(false);
            }

            return SendAsync();
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("bind");
            _output.Probe(scope);
            _source.Probe(scope);
        }


        class BindPipe :
            IPipe<TRight>
        {
            readonly TLeft _context;
            readonly IPipe<BindContext<TLeft, TRight>> _output;

            public BindPipe(TLeft context, IPipe<BindContext<TLeft, TRight>> output)
            {
                _context = context;
                _output = output;
            }

            Task IPipe<TRight>.Send(TRight context)
            {
                var bindContext = new BindContextProxy<TLeft, TRight>(_context, context);

                return _output.Send(bindContext);
            }

            void IProbeSite.Probe(ProbeContext context)
            {
            }
        }
    }
}
