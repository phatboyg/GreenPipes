using System.Threading;
using System.Threading.Tasks;
using GreenPipes.Payloads;
using NUnit.Framework;

namespace GreenPipes.Tests
{
    using System;


    [TestFixture]
    public class Parent_Child_Pipes
    {
        [Test]
        public async Task ShouldDeliverToBoth()
        {
            var count1 = 0;
            var count2 = 0;

            var pipe2 = Pipe.New<InitialContext>(cfg =>
            {
                cfg.UseExecuteAsync(async payload =>
                {
                    var pipe1 = Pipe.New<SubContext>(subCfg =>
                    {
                        subCfg.UseExecute(p =>
                        {
                            Interlocked.Increment(ref count1);
                        });
                    });

                    await pipe1.Send(new SubContext()).ConfigureAwait(false);

                    Interlocked.Increment(ref count2);
                });
            });
            

            await pipe2.Send(new InitialContext()).ConfigureAwait(false);

            Assert.That(count1, Is.EqualTo(1));
            Assert.That(count2, Is.EqualTo(1));


            Console.WriteLine(pipe2.GetProbeResult().ToJsonString());
        }

        [Test]
        public async Task Should_compose_pipes()
        {
            var count1 = 0;
            var count2 = 0;

            var pipe1 = Pipe.New<InitialContext>(cfg =>
            {
                cfg.UseExecute(p =>
                {
                    Interlocked.Increment(ref count1);
                });
            });

            var pipe2 = Pipe.New<InitialContext>(cfg =>
            {
                cfg.UseFork(pipe1);

                cfg.UseExecuteAsync(async payload =>
                {
                    Interlocked.Increment(ref count2);
                });
            });            

            await pipe2.Send(new InitialContext()).ConfigureAwait(false);

            Assert.That(count1, Is.EqualTo(1));
            Assert.That(count2, Is.EqualTo(1));


            Console.WriteLine(pipe2.GetProbeResult().ToJsonString());
        }

        class InitialContext :
            BasePipeContext,
            PipeContext
        {
            public InitialContext()
                : base(new PayloadCache())
            {
            }
        }

        class SubContext :
            BasePipeContext,
            PipeContext
        {
            public SubContext()
                : base(new PayloadCache())
            {
            }
        }
    }
}