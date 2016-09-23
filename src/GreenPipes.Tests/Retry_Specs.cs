// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using Util;


    [TestFixture]
    public class Using_the_retry_filter
    {
        class TestContext :
            BasePipeContext,
            PipeContext
        {
            public TestContext()
                : base(new PayloadCache())
            {                
            }
        }


        [Test]
        public void Should_retry_the_specified_times_and_fail()
        {
            int count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r => r.Interval(4, TimeSpan.FromMilliseconds(2)));
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(5));
        }

        [Test]
        public void Should_support_overloading_downstream()
        {
            int count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r => r.Interval(4, TimeSpan.FromMilliseconds(2)));
                x.UseRetry(r => r.None());
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Should_support_overloading_downstream_either_way()
        {
            int count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r => r.None());
                x.UseRetry(r => r.Interval(4, TimeSpan.FromMilliseconds(2)));
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(5));
        }

        [Test]
        public async Task Should_be_observable_at_the_inner_level()
        {
            var observer = new RetryObserver();

            int count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r => r.None());
                x.UseRetry(r =>
                {
                    r.Interval(4, TimeSpan.FromMilliseconds(2));
                    r.ConnectRetryObserver(observer);
                });
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(5));

            await observer.RetryFault;

            Assert.That(observer.RetryFaultCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_be_observable_at_each_retry()
        {
            var observer = new RetryObserver();

            int count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r => r.None());
                x.UseRetry(r =>
                {
                    r.Interval(4, TimeSpan.FromMilliseconds(2));
                    r.ConnectRetryObserver(observer);
                });
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(5));

            await observer.PostFault;

            Assert.That(observer.PostFaultCount, Is.EqualTo(4));
        }

        [Test]
        public async Task Should_be_observable_at_the_outer_level()
        {
            var observer = new RetryObserver();

            int count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r =>
                {
                    r.None();
                    r.ConnectRetryObserver(observer);
                });
                x.UseRetry(r => r.Interval(4, TimeSpan.FromMilliseconds(2)));
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(5));

            await observer.RetryFault;

            Assert.That(observer.RetryFaultCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_be_observable_with_no_retry_allowed()
        {
            var observer = new RetryObserver();

            int count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r =>
                {
                    r.None();
                    r.ConnectRetryObserver(observer);
                });
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(1));

            await observer.RetryFault;

            Assert.That(observer.RetryFaultCount, Is.EqualTo(1));
        }


        class RetryObserver :
            IRetryObserver
        {
            readonly TaskCompletionSource<bool> _retryFault;
            int _retryFaultCount;
            readonly TaskCompletionSource<bool> _postFault;
            int _postFaultCount;

            public RetryObserver()
            {
                _retryFault = new TaskCompletionSource<bool>();
                _postFault = new TaskCompletionSource<bool>();
            }

            Task IRetryObserver.PostCreate<T>(RetryPolicyContext<T> context)
            {
                return TaskUtil.Completed;
            }

            Task IRetryObserver.PostFault<T>(RetryContext<T> context)
            {
                Interlocked.Increment(ref _postFaultCount);

                _postFault.TrySetResult(true);

                return TaskUtil.Completed;
            }

            Task IRetryObserver.PreRetry<T>(RetryContext<T> context)
            {
                return TaskUtil.Completed;
            }

            Task IRetryObserver.RetryFault<T>(RetryContext<T> context)
            {
                Interlocked.Increment(ref _retryFaultCount);

                _retryFault.TrySetResult(true);

                return TaskUtil.Completed;
            }

            public int RetryFaultCount => _retryFaultCount;

            public Task RetryFault => _retryFault.Task;

            public int PostFaultCount => _postFaultCount;

            public Task PostFault => _postFault.Task;
        }
    }
}