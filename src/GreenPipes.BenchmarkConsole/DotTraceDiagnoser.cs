// Copyright 2012-2018 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.BenchmarkConsole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using BenchmarkDotNet.Analysers;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Diagnosers;
    using BenchmarkDotNet.Engines;
    using BenchmarkDotNet.Exporters;
    using BenchmarkDotNet.Loggers;
    using BenchmarkDotNet.Running;
    using BenchmarkDotNet.Validators;


    public class DotTraceDiagnoser :
        Attribute,
        IDiagnoser
    {
        readonly string _saveLocation;
        Process _process;
        Process profiler;

        public DotTraceDiagnoser()
        {
            _saveLocation = $"C:\\Users\\chris\\AppData\\Local\\Temp\\GreenPipes\\{DateTimeOffset.Now.UtcDateTime:yyyyMMddTHHmmss}.bench.dtp";
        }

        /// <inheritdoc />
        public IColumnProvider GetColumnProvider() => EmptyColumnProvider.Instance;

        /// <inheritdoc />
        public RunMode GetRunMode(Benchmark benchmark) => RunMode.ExtraRun;

        /// <inheritdoc />
        public void Handle(HostSignal signal, DiagnoserActionParameters parameters)
        {
            switch (signal)
            {
                case HostSignal.BeforeMainRun:
                    try
                    {
                        var startInfo = new ProcessStartInfo(
                            @"C:\Users\chris\AppData\Local\JetBrains\Installations\dotTrace11\ConsoleProfiler.exe",
                            $"attach {parameters.Process.Id} --save-to={_saveLocation} --profiling-type=Sampling")
                        {
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            WindowStyle = ProcessWindowStyle.Normal,
                            UseShellExecute = false,
                        };
                        Console.WriteLine(startInfo.FileName);
                        Console.WriteLine(startInfo.Arguments);
                        _process = new Process
                        {
                            StartInfo = startInfo
                        };
                        _process.ErrorDataReceived += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.Data);
                        _process.OutputDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
                        _process.Start();
                        _process.BeginErrorReadLine();
                        _process.BeginOutputReadLine();
                        _process.Exited += (sender, args) =>
                        {
                            _process.Dispose();
                        };
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.StackTrace);
                        throw;
                    }
                    break;
                case HostSignal.AfterMainRun:
                    break;
                case HostSignal.BeforeAnythingElse:
                    break;
                case HostSignal.AfterAll:
                    break;
                case HostSignal.SeparateLogic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signal), signal, null);
            }
        }

        /// <inheritdoc />
        public void ProcessResults(DiagnoserResults results)
        {
        }

        /// <inheritdoc />
        public void DisplayResults(ILogger logger)
        {
        }

        /// <inheritdoc />
        public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters) =>
            Enumerable.Empty<ValidationError>();

        /// <inheritdoc />
        public IEnumerable<string> Ids => new[] {nameof(DotTraceDiagnoser)};

        /// <inheritdoc />
        public IEnumerable<IExporter> Exporters => Enumerable.Empty<IExporter>();

        public IEnumerable<IAnalyser> Analysers => Enumerable.Empty<IAnalyser>();
    }
}