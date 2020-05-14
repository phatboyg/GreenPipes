namespace GreenPipes
{
    public delegate TKey KeyAccessor<in TContext, out TKey>(TContext context);
}
