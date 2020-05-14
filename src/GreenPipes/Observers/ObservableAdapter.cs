namespace GreenPipes.Observers
{
    using System;
    using System.Threading.Tasks;


    public class ObservableAdapter<TContext> :
        IFilterObserver<TContext>
        where TContext : class, PipeContext
    {
        readonly IFilterObserver _observer;

        public ObservableAdapter(IFilterObserver observer)
        {
            _observer = observer;
        }

        public Task PreSend(TContext context)
        {
            return _observer.PreSend(context);
        }

        public Task PostSend(TContext context)
        {
            return _observer.PostSend(context);
        }

        public Task SendFault(TContext context, Exception exception)
        {
            return _observer.SendFault(context, exception);
        }
    }
}
