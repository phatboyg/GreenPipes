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
    using System.Threading.Tasks;
    using Contracts;
    using NUnit.Framework;
    using Pipes;
    using Util;


    [TestFixture]
    public class Observing_a_dispatch_pipe
    {
        [Test]
        public async Task Should_be_called_post_send()
        {
            IControlPipe pipe = new ControlPipe();

            pipe.ConnectPipe(Pipe.Empty<CommandContext<SetConcurrencyLimit>>());

            var observer = new Observer<CommandContext<SetConcurrencyLimit>>();

            pipe.ConnectObserver(observer);

            var observer2 = new Observer();

            pipe.ConnectObserver(observer2);

            await pipe.SetConcurrencyLimit(32);

            await observer.PostSent;

            Assert.That(async () => await observer2.PostSent, Is.InstanceOf<CommandContext<SetConcurrencyLimit>>());
        }

        [Test]
        public async Task Should_be_called_pre_send()
        {
            IControlPipe pipe = new ControlPipe();

            pipe.ConnectPipe(Pipe.Empty<CommandContext<SetConcurrencyLimit>>());

            var observer = new Observer<CommandContext<SetConcurrencyLimit>>();

            pipe.ConnectObserver(observer);

            var observer2 = new Observer();

            pipe.ConnectObserver(observer2);

            await pipe.SetConcurrencyLimit(32);

            await observer.PreSent;

            Assert.That(async () => await observer2.PreSent, Is.InstanceOf<CommandContext<SetConcurrencyLimit>>());
        }

        [Test]
        public void Should_be_called_when_send_faulted()
        {
            IControlPipe pipe = new ControlPipe();

            pipe.ConnectPipe(Pipe.New<CommandContext<SetConcurrencyLimit>>(x =>
            {
                x.UseExecute(context =>
                {
                    throw new IntentionalTestException("Wow!");
                });
            }));

            var observer = new Observer<CommandContext<SetConcurrencyLimit>>();

            pipe.ConnectObserver(observer);

            var observer2 = new Observer();

            pipe.ConnectObserver(observer2);

            Assert.That(async () => await pipe.SetConcurrencyLimit(32), Throws.TypeOf<IntentionalTestException>());

            Assert.That(async () => await observer.SendFaulted, Throws.TypeOf<IntentionalTestException>());

            Assert.That(async () => await observer2.SendFaulted, Throws.TypeOf<IntentionalTestException>());
        }


        class Observer<T> :
            IFilterObserver<T>
            where T : class, PipeContext
        {
            readonly TaskCompletionSource<T> _consumeFaulted;
            readonly TaskCompletionSource<T> _postConsumed;
            readonly TaskCompletionSource<T> _preConsumed;

            public Observer()
            {
                _preConsumed = new TaskCompletionSource<T>();
                _postConsumed = new TaskCompletionSource<T>();
                _consumeFaulted = new TaskCompletionSource<T>();
            }

            public Task<T> PreSent => _preConsumed.Task;
            public Task<T> PostSent => _postConsumed.Task;
            public Task<T> SendFaulted => _consumeFaulted.Task;

            Task IFilterObserver<T>.PreSend(T context)
            {
                _preConsumed.TrySetResult(context);

                return TaskUtil.Completed;
            }

            Task IFilterObserver<T>.PostSend(T context)
            {
                _postConsumed.TrySetResult(context);

                return TaskUtil.Completed;
            }

            Task IFilterObserver<T>.SendFault(T context, Exception exception)
            {
                _consumeFaulted.TrySetException(exception);

                return TaskUtil.Completed;
            }
        }

        class Observer :
            IFilterObserver
        {
            readonly TaskCompletionSource<CommandContext> _consumeFaulted;
            readonly TaskCompletionSource<CommandContext> _postConsumed;
            readonly TaskCompletionSource<CommandContext> _preConsumed;

            public Observer()
            {
                _preConsumed = new TaskCompletionSource<CommandContext>();
                _postConsumed = new TaskCompletionSource<CommandContext>();
                _consumeFaulted = new TaskCompletionSource<CommandContext>();
            }

            public Task<CommandContext> PreSent => _preConsumed.Task;
            public Task<CommandContext> PostSent => _postConsumed.Task;
            public Task<CommandContext> SendFaulted => _consumeFaulted.Task;

            Task IFilterObserver.PreSend<T>(T context)
            {
                _preConsumed.TrySetResult(context as CommandContext);

                return TaskUtil.Completed;
            }

            Task IFilterObserver.PostSend<T>(T context)
            {
                _postConsumed.TrySetResult(context as CommandContext);

                return TaskUtil.Completed;
            }

            Task IFilterObserver.SendFault<T>(T context, Exception exception)
            {
                _consumeFaulted.TrySetException(exception);

                return TaskUtil.Completed;
            }
        }
    }
}