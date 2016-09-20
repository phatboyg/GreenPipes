using System.Threading;
using System.Threading.Tasks;

namespace GreenPipes
{
    public interface IContextSource<TContext> where TContext : class, PipeContext
    {
        Task Fill(IPipe<TContext> pipe, CancellationToken token = default(CancellationToken));
    }
}