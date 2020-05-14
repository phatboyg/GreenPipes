namespace GreenPipes.Configurators
{
    public delegate IRetryPolicy RetryPolicyFactory(IExceptionFilter filter);
}
