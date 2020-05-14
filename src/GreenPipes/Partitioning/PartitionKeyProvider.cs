namespace GreenPipes.Partitioning
{
    public delegate byte[] PartitionKeyProvider<in TContext>(TContext context);
}
