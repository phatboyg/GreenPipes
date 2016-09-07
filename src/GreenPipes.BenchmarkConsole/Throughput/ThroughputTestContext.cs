namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System;
    using Payloads;


    public class ThroughputTestContext :
        BasePipeContext,
        TestContext
    {
        public ThroughputTestContext()
            : base(new PayloadCache())
        {
        }

        public ThroughputTestContext(Guid correlationId, string payload)
            : base(new PayloadCache())

        {
            CorrelationId = correlationId;
            Payload = payload;
        }

        public string Payload { get; set; }

        public Guid CorrelationId { get; set; }
    }
}