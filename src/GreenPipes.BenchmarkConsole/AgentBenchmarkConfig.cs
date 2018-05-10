namespace GreenPipes.BenchmarkConsole
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;
    using BenchmarkDotNet.Engines;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Jobs;


    public class AgentBenchmarkConfig :
        ManualConfig
    {
        public AgentBenchmarkConfig()
        {
            Add(MemoryDiagnoser.Default);
            Add(new Job
            {
                Env = {Runtime = Runtime.Core},
                Run =
                {
                    TargetCount = 2,
                    RunStrategy = RunStrategy.Throughput,
                    WarmupCount = 1,
                    LaunchCount = 1,
                    UnrollFactor = 1,
                    InvocationCount = 10_000
                }
            });
        }
    }
}