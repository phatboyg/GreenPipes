namespace GreenPipes.Configurators
{
    using System.ComponentModel;


    public interface IRetryConfigurator :
        IExceptionConfigurator,
        IRetryObserverConnector
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        void SetRetryPolicy(RetryPolicyFactory factory);
    }
}
