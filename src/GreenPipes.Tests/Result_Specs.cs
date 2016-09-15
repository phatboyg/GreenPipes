// Copyright 2013-2016 Chris Patterson
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
namespace GreenPipes.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Payloads;


    [TestFixture]
    public class Using_a_pipe_with_a_result
    {
        [Test]
        public async Task Should_return_the_result()
        {
            var pipe = Pipe.New<QueryContext, QueryResult>(cfg =>
            {
                cfg.UseLog(Console.Out, (context, log) => Task.FromResult($"Value: {context.Value} = {log.Duration}"));
                cfg.Handler(context => Task.FromResult(new QueryResult {Value = context.Value}));
            });

            var result = await pipe.Send(new QueryContext() {Value = "Hello"});

            Assert.That(result.Value, Is.EqualTo("Hello"));
        }
    }


    class QueryResult
    {
        public string Value { get; set; }
    }


    class QueryContext :
        BasePipeContext,
        PipeContext
    {
        public QueryContext()
            : base(new PayloadCache(), CancellationToken.None)
        {
        }

        public string Value { get; set; }
    }
}