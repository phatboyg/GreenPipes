// Copyright 2012-2016 Chris Patterson
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
    using Contexts;
    using Contracts;
    using NUnit.Framework;
    using Policies;
    using Policies.ExceptionFilters;
    using Util;


    [TestFixture]
    public class Using_the_retry_filter
    {
        [Test]
        public async Task Should_be_observable_at_each_retry()
        {
            var observer = new RetryObserver();

            var count = 0;
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
        public async Task Should_be_observable_at_the_inner_level()
        {
            var observer = new RetryObserver();

            var count = 0;
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
        public async Task Should_be_observable_at_the_outer_level()
        {
            var observer = new RetryObserver();

            var count = 0;
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

            var count = 0;
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

        [Test]
        public void Should_retry_the_specified_times_and_fail()
        {
            var count = 0;
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
        public async Task Should_retry_and_then_succeed_without_repeating_forever()
        {
            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r => r.Immediate(1));
                x.UseExecute(payload =>
                {
                    var current = Interlocked.Increment(ref count);

                    if (current == 1)
                        throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            await pipe.Send(context);

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void Should_ignore_the_exception()
        {
            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r =>
                {
                    r.Ignore<IntentionalTestException>();
                    r.Immediate(4);
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
        }

        [Test]
        public void Should_ignore_the_inner_exception()
        {
            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r =>
                {
                    r.Ignore<IntentionalTestException>();
                    r.Immediate(4);
                });
                x.UseExecute(payload =>
                {
                    count++;
                    throw new Exception("Inside is death", new IntentionalTestException("Kaboom!"));
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<Exception>());

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Should_not_stack_overflow_with_a_ridiculous_retry_count()
        {
            var count = 0;
            IPipe<TestContext> pipe = Pipe.New<TestContext>(x =>
            {
                x.UseRetry(r => r.Interval(20000, TimeSpan.Zero));
                x.UseExecute(payload =>
                {
                    count++;
                    throw new IntentionalTestException("Kaboom!");
                });
            });

            var context = new TestContext();

            Assert.That(async () => await pipe.Send(context), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(20001));
        }

        [Test]
        public void Should_support_overloading_downstream()
        {
            var count = 0;
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
            var count = 0;
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
        public void Should_support_overloading_downstream_with_dispatch()
        {
            var count = 0;
            IPipe<CommandContext> pipe = Pipe.New<CommandContext>(x =>
            {
                x.UseRetry(r => r.None());
                x.UseDispatch(new PipeContextConverterFactory(), d =>
                {
                    d.Pipe<CommandContext<SetConcurrencyLimit>>(p =>
                    {
                        p.UseRetry(r => r.Interval(4, TimeSpan.FromMilliseconds(2)));
                        p.UseExecute(payload =>
                        {
                            count++;
                            throw new IntentionalTestException("Kaboom!");
                        });
                    });
                });
            });

            Assert.That(async () => await pipe.SetConcurrencyLimit(32), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(5));
        }

        [Test]
        public void Should_support_overloading_downstream_with_dispatch_either_way()
        {
            var count = 0;
            IPipe<CommandContext> pipe = Pipe.New<CommandContext>(x =>
            {
                x.UseRetry(r => r.Interval(4, TimeSpan.FromMilliseconds(2)));
                x.UseDispatch(new PipeContextConverterFactory(), d =>
                {
                    d.Pipe<CommandContext<SetConcurrencyLimit>>(p =>
                    {
                        p.UseRetry(r => r.None());
                        p.UseExecute(payload =>
                        {
                            count++;
                            throw new IntentionalTestException("Kaboom!");
                        });
                    });
                });
            });

            Assert.That(async () => await pipe.SetConcurrencyLimit(32), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Should_support_overloading_downstream_with_dispatch_either_that()
        {
            var count = 0;
            IPipe<CommandContext> pipe = Pipe.New<CommandContext>(x =>
            {
                x.UseRetry(r => r.Interval(4, TimeSpan.FromMilliseconds(2)));
                x.UseRetry(r => r.None());
                x.UseDispatch(new PipeContextConverterFactory(), d =>
                {
                    d.Pipe<CommandContext<SetConcurrencyLimit>>(p =>
                    {
                        p.UseExecute(payload =>
                        {
                            count++;
                            throw new IntentionalTestException("Kaboom!");
                        });
                    });
                });
            });

            Assert.That(async () => await pipe.SetConcurrencyLimit(32), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(1));
        }
        
        [Test]
        public void Should_support_overloading_downstream_with_dispatch_either_that_thang()
        {
            var count = 0;
            IPipe<CommandContext> pipe = Pipe.New<CommandContext>(x =>
            {
                x.UseRetry(r => r.SetRetryPolicy(filter => new CommandContextRetryPolicy(new IntervalRetryPolicy(filter, 4, 4, 4, 4))));
                x.UseRetry(r => r.SetRetryPolicy(filter => new CommandContextRetryPolicy(new NoRetryPolicy(filter))));
                x.UseDispatch(new PipeContextConverterFactory(), d =>
                {
                    d.Pipe<CommandContext<SetConcurrencyLimit>>(p =>
                    {
                        p.UseExecute(payload =>
                        {
                            count++;
                            throw new IntentionalTestException("Kaboom!");
                        });
                    });
                });
            });

            Assert.That(async () => await pipe.SetConcurrencyLimit(32), Throws.TypeOf<IntentionalTestException>());

            Assert.That(count, Is.EqualTo(1));
        }


        class TestContext :
            BasePipeContext,
            PipeContext
        {
        }


        class RetryObserver :
            IRetryObserver
        {
            readonly TaskCompletionSource<bool> _postFault;
            readonly TaskCompletionSource<bool> _retryFault;
            int _postFaultCount;
            int _retryFaultCount;

            public RetryObserver()
            {
                _retryFault = new TaskCompletionSource<bool>();
                _postFault = new TaskCompletionSource<bool>();
            }

            public int RetryFaultCount => _retryFaultCount;

            public Task RetryFault => _retryFault.Task;

            public int PostFaultCount => _postFaultCount;

            public Task PostFault => _postFault.Task;

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
        }
    }
}