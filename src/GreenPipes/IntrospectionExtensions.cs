namespace GreenPipes
{
    using System;
    using System.Threading;
    using Introspection;


    public static class IntrospectionExtensions
    {
        public static ProbeResult GetProbeResult(this IProbeSite probeSite, CancellationToken cancellationToken = default)
        {
            var builder = new ProbeResultBuilder(Guid.NewGuid(), cancellationToken);

            probeSite.Probe(builder);

            return ((IProbeResultBuilder)builder).Build();
        }
    }
}
