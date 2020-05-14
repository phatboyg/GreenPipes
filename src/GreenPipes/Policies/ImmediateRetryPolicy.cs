namespace GreenPipes.Policies
{
    using System;


    public class ImmediateRetryPolicy :
        IRetryPolicy
    {
        readonly IExceptionFilter _filter;
        readonly int _retryLimit;

        public ImmediateRetryPolicy(IExceptionFilter filter, int retryLimit)
        {
            _filter = filter;
            _retryLimit = retryLimit;
        }

        public int RetryLimit => _retryLimit;

        void IProbeSite.Probe(ProbeContext context)
        {
            context.Set(new
            {
                Policy = "Immediate",
                Limit = _retryLimit
            });

            _filter.Probe(context);
        }

        RetryPolicyContext<T> IRetryPolicy.CreatePolicyContext<T>(T context)
        {
            return new ImmediateRetryPolicyContext<T>(this, context);
        }

        public bool IsHandled(Exception exception)
        {
            return _filter.Match(exception);
        }
    }
}
