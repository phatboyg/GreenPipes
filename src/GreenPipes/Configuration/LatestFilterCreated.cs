namespace GreenPipes
{
    using Filters;


    public delegate void LatestFilterCreated<T>(ILatestFilter<T> filter)
        where T : class, PipeContext;
}
