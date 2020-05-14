namespace GreenPipes.Pipes
{
    using System;
    using System.Threading.Tasks;
    using Contexts;
    using Internals.Extensions;


    /// <summary>
    /// A pipe for a single request with multiple result types
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public class MultipleResultRequestPipe<TRequest> :
        IRequestPipe<TRequest>
        where TRequest : class
    {
        readonly IPipe<RequestContext> _requestPipe;
        readonly IPipe<ResultContext> _resultPipe;

        public MultipleResultRequestPipe(IPipe<RequestContext> requestPipe, IPipe<ResultContext> resultPipe)
        {
            _resultPipe = resultPipe;
            _requestPipe = requestPipe;
        }

        public Task<ResultContext> Send(TRequest request)
        {
            var context = new MultipleResultRequestContext<TRequest>(request, _resultPipe);
            try
            {
                SendRequest(context);

                return context.Result;
            }
            catch (TaskCanceledException)
            {
                context.TrySetCanceled();

                throw;
            }
            catch (Exception ex)
            {
                context.TrySetException(ex);

                throw;
            }
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("request");
            scope.Add("requestType", TypeCache<TRequest>.ShortName);

            _requestPipe.Probe(scope.CreateScope("requestPipe"));
            _resultPipe.Probe(scope.CreateScope("resultPipe"));
        }

        void SendRequest(RequestContext context)
        {
            var send = _requestPipe.Send(context);
            send.ContinueWith(task => context.TrySetCanceled(), TaskContinuationOptions.OnlyOnCanceled);
            send.ContinueWith(task => context.TrySetException(task.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
