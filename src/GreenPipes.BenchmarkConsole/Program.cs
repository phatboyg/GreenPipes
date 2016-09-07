namespace GreenPipes.BenchmarkConsole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Throughput;


    class Program
    {
        static List<string> _remaining;

        static void Main(string[] args)
        {
            Console.WriteLine("Green Pipes Benchmark");
            Console.WriteLine();

            var optionSet = new ProgramOptionSet();

            try
            {
                _remaining = optionSet.Parse(args);

                if (optionSet.Help)
                {
                    ShowHelp(optionSet);
                    return;
                }

                optionSet.ShowOptions();

                if (optionSet.Benchmark.HasFlag(ProgramOptionSet.BenchmarkOptions.Latency))
                {
                    RunLatencyBenchmark(optionSet);
                }

                if (Debugger.IsAttached)
                {
                    Console.Write("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            catch (OptionException ex)
            {
                Console.Write("gpbench: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Use 'gpbench --help' for detailed usage information.");
            }
        }

        static void RunLatencyBenchmark(ProgramOptionSet optionSet)
        {
            var messageLatencyOptionSet = new ThroughputOptionSet();

            messageLatencyOptionSet.Parse(_remaining);

            IThroughputSettings settings = messageLatencyOptionSet;

            var benchmark = new ThroughputBenchmark(settings);

            benchmark.Run();
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: gpbench [OPTIONS]+");
            Console.WriteLine("Executes the benchmark using the specified options.");
            Console.WriteLine("If no benchmark is specified, all benchmarks are executed.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);

            Console.WriteLine();
            Console.WriteLine("Benchmark Options:");
            new ThroughputOptionSet().WriteOptionDescriptions(Console.Out);
        }
    }
}