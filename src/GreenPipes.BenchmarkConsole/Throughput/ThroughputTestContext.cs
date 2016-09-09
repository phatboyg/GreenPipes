namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System;
    using System.Threading;
    using Payloads;


    public class ThroughputTestContext :
        BasePipeContext,
        TestContext
    {
        public ThroughputTestContext()
            : base(new PayloadCache(), CancellationToken.None)
        {
        }

        public ThroughputTestContext(Guid correlationId, string payload)
            : base(new PayloadCache(), CancellationToken.None)

        {
            CorrelationId = correlationId;
            Payload = payload;
        }

        public string Payload { get; set; }

        public Guid CorrelationId { get; set; }

        public int Attempts { get; set; }
    }
}