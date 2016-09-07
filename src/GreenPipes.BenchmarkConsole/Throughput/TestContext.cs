namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System;


    public interface TestContext :
        PipeContext
    {
        Guid CorrelationId { get; }
    }
}