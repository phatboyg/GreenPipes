namespace GreenPipes.Filters
{
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;


    public class DataFlowFilter<T> :
        IFilter<T>
        where T : class, PipeContext
    {
        readonly ITargetBlock<T> _targetBlock;

        public DataFlowFilter(ITargetBlock<T> targetBlock)
        {
            _targetBlock = targetBlock;
        }

        public async Task Send(T context, IPipe<T> next)
        {
            await _targetBlock.SendAsync(context).ConfigureAwait(false);

            await next.Send(context).ConfigureAwait(false);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("dataflow");
        }
    }
}