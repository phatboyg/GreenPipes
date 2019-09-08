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
using NUnit.Framework;

namespace GreenPipes.Tests
{
    using System.Threading.Tasks;
    using Filters;


    [TestFixture]
    public class Using_the_latest_filter_on_the_pipe
    {
        [Test]
        public async Task Should_keep_track_of_only_the_last_value()
        {
            ILatestFilter<IInputContext<A>> latestFilter = null;

            IPipe<IInputContext<A>> pipe = Pipe.New<IInputContext<A>>(x =>
            {
                x.UseLatest(l => l.Created = filter => latestFilter = filter);
                x.UseExecute(payload =>
                {
                });
            });

            Assert.That(latestFilter, Is.Not.Null);

            var inputContext = new InputContext(new object());

            var limit = 100;
            for (int i = 0; i <= limit; i++)
            {
                var context = new InputContext<A>(inputContext, new A {Index = i});
                await pipe.Send(context);
            }

            IInputContext<A> latest = await latestFilter.Latest;

            Assert.That(latest.Value.Index, Is.EqualTo(limit));
        }


        class A
        {
            public int Index { get; set; }
        }
    }
}