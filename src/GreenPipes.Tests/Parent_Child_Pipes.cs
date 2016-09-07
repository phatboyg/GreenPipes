using System.Threading;
using System.Threading.Tasks;
using GreenPipes.Payloads;
using NUnit.Framework;

namespace GreenPipes.Tests
{
    [TestFixture]
    public class Parent_Child_Pipes
    {
        [Test]
        public async Task ShouldDeliverToBoth()
        {
            var count1 = 0;
            var count2 = 0;

            var pipe2 = Pipe.New<InitialContext>(x =>
            {
                x.UseExecuteAsync(async payload =>
                {
                    var pipe1 = Pipe.New<SubContext>(xx =>
                    {
                        xx.UseExecute(p =>
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