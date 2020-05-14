namespace GreenPipes.Introspection
{
    public static class ProbeHostInfoCache
    {
        public static ProbeHostInfo Host => Cached.HostInfo;


        static class Cached
        {
            internal static readonly ProbeHostInfo HostInfo = new GreenPipesHostInfo(true);
        }
    }
}
