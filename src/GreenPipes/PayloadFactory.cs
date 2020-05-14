namespace GreenPipes
{
    public delegate TPayload PayloadFactory<out TPayload>()
        where TPayload : class;
}
