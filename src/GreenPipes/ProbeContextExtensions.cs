namespace GreenPipes
{
    public static class ProbeContextExtensions
    {
        public static ProbeContext CreateFilterScope(this ProbeContext context, string filterType)
        {
            var scope = context.CreateScope("filters");

            scope.Add("filterType", filterType);

            return scope;
        }
    }
}
