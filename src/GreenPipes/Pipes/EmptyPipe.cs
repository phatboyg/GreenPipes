namespace GreenPipes.Pipes
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;


    public class EmptyPipe<TContext> :
        IPipe<TContext>
        where TContext : class, PipeContext
    {
        [DebuggerNonUserCode]
        Task IPipe<TContext>.Send(TContext context)
        {
            return TaskUtil.Completed;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
        }
    }
}
