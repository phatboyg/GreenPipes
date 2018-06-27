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
namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System.Threading.Tasks;
    using Mapping;


    public class BenchmarkFilter :
        IFilter<TestContext>
    {
        public Task Send(TestContext context, IPipe<TestContext> next)
        {
            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }


    public class BenchmarkResultPipe :
        IResultPipe<TestContext, TestResult>
    {
        public Task<TestResult> Send(TestContext context)
        {
            return Task.FromResult(default(TestResult));
        }

        public void Probe(ProbeContext context)
        {
        }
    }


    public class BenchmarkAwaitFilter :
        IFilter<TestContext>
    {
        public async Task Send(TestContext context, IPipe<TestContext> next)
        {
            await next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }


    public class BenchmarkAwaitResultFilter :
        IResultFilter<TestContext, TestResult>
    {
        public async Task<TestResult> Send(TestContext context, IResultPipe<TestContext, TestResult> next)
        {
            var result = await next.Send(context).ConfigureAwait(false);

            return result;
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}