namespace GreenPipes.BenchmarkConsole
{
    using System;


    class ProgramOptionSet :
        OptionSet
    {
        [Flags]
        public enum BenchmarkOptions
        {
            Latency = 1,
            RPC = 2,
        }

        public ProgramOptionSet()
        {
            Add<string>("v|verbose", "Verbose output", x => Verbose = x != null);
            Add<string>("h|help", "Display this help and exit", x => Help = x != null);
            Add<BenchmarkOptions>("run:", "Run benchmark (All, Latency, RPC)", value => Benchmark = value);
            Add("rpc", "Run the RPC benchmark", x => Benchmark = BenchmarkOptions.RPC);
            Add("latency", "Run the Latency benchmark", x => Benchmark = BenchmarkOptions.Latency);

            Benchmark = BenchmarkOptions.Latency | BenchmarkOptions.RPC;
        }

        public BenchmarkOptions Benchmark { get; private set; }

        public bool Verbose { get; set; }
        public bool Help { get; set; }

        public void ShowOptions()
        {
        }
    }
}