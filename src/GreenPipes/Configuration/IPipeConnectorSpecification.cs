namespace GreenPipes
{
    public interface IPipeConnectorSpecification :
        ISpecification
    {
        void Connect(IPipeConnector connector);
    }
}
