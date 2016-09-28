using System.Threading;
using System.Threading.Tasks;

namespace GreenPipes
{
    public interface IContextSource<out TContext> where TContext : class, PipeContext
    {
        Task Start(CancellationToken token);
        Task Stop(CancellationToken token);

        Task Connect(IPipe<TContext> pipe, CancellationToken token = default(CancellationToken));
    }
}