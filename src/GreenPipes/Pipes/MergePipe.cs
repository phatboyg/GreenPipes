namespace GreenPipes.Pipes
{
    using System.Threading.Tasks;
    using Internals.Extensions;


    public class MergePipe<TInput, TSplit> :
        IPipe<TSplit>
        where TSplit : class, PipeContext
        where TInput : class, PipeContext
    {
        readonly MergeFilterContextProvider<TInput, TSplit> _contextProvider;
        readonly TInput _input;
        readonly IPipe<TInput> _next;

        public MergePipe(IPipe<TInput> next, TInput input, MergeFilterContextProvider<TInput, TSplit> contextProvider)
        {
            _next = next;
            _input = input;
            _contextProvider = contextProvider;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("merge");
            scope.Set(new
            {
                InputType = TypeCache<TInput>.ShortName
            });

            _next.Probe(scope);
        }

        Task IPipe<TSplit>.Send(TSplit context)
        {
            var inputContext = _contextProvider(_input, context);

            return _next.Send(inputContext);
        }
    }
}
