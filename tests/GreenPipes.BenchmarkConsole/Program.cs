namespace GreenPipes.BenchmarkConsole
{
    using System;
    using BenchmarkDotNet.Running;


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Green Pipes Benchmark");
            Console.WriteLine();

            BenchmarkRunner.Run<SendBenchmark>();

            //          BenchmarkRunner.Run<SupervisorBenchmark>();

            //          BenchmarkRunner.Run<PayloadBenchmark>();
        }
    }
}
