namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System;


    public class ThroughputTestContext :
        BasePipeContext,
        TestContext
    {
        public ThroughputTestContext(Guid correlationId, string payload)
        {
            CorrelationId = correlationId;
            Payload = payload;
        }

        public string Payload { get; set; }

        public Guid CorrelationId { get; set; }

        public int Attempts { get; set; }
    }
}
