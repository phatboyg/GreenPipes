using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes.Payloads;
using NUnit.Framework;
using Timer = System.Timers.Timer;

namespace GreenPipes.Tests
{
    public class Source_Specs
    {
        CancellationTokenSource _ts;
        int _i;
        IPipe<FileContext> _pipe;
        TestContextSource _source;

        [SetUp]
        public void SetUp()
        {
            _ts = new CancellationTokenSource();
            _i = 0;

            _pipe = Pipe.New<FileContext>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    _i++;
                    Console.WriteLine($"A:{cxt.FilePath}");
                });
            });

            _source = new TestContextSource();

            _source.Connect(_pipe, _ts.Token);
        }

        [Test]
        public async Task ShouldNotStart()
        {
            await Task.Delay(100);

            Assert.That(_i, Is.EqualTo(0));
        }

        [Test]
        public async Task MustBeStarted()
        {
            _source.Start();

            //do some work
            await Task.Delay(50);

            Assert.That(_i, Is.GreaterThan(0));

            _ts.Cancel();
        }

        [Test]
        public async Task CanBeStopped()
        {
            _source.Start();

            //do some work
            await Task.Delay(100);

           //Stop the source
           await _source.Stop();

            //TODO: not sure how to best test that the cancel was propagated.
            int j = _i;

            await Task.Delay(100);

            Assert.That(_i, Is.GreaterThan(1));

            //should stop in a few cycles
            Assert.That(j, Is.LessThan(_i + 2));

        }

        [Test]
        public async Task CanBeCancelled()
        {
            _source.Start();

            //do some work
            await Task.Delay(100);

            //cancel the source
            _ts.Cancel();

            //TODO: not sure how to best test that the cancel was propagated.
            int j = _i;

            await Task.Delay(100);

            Assert.That(_ts.IsCancellationRequested, Is.True);
            Assert.That(_i, Is.GreaterThan(1));
            
            //should stop in a few cycles
            Assert.That(j, Is.LessThan(_i+2));
        }

        [Test]
        public async Task MultiPipe()
        {
            var j = 0;
            var pipe2 = Pipe.New<FileContext>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    j++;
                    Console.WriteLine($"2:{cxt.FilePath}");
                });
            });
            _source.Connect(pipe2);
            _source.Start();

            //do some work
            await Task.Delay(100);

            _source.Stop();

            //TODO: not sure how to best test that the cancel was propagated.
            int cache = _i;

            await Task.Delay(100);
            
            Assert.That(_i, Is.GreaterThan(1));
            Assert.That(j, Is.GreaterThan(1));

            //should stop in a few cycles
            Assert.That(cache, Is.LessThan(_i + 2));
        }

    }

    public class TestContextSource : IContextSource<FileContext>
    {
        readonly Timer _timer;
        bool _stopped;

        public TestContextSource()
        {
            _timer = new Timer(20);
        }


        public Task Start(CancellationToken token = default(CancellationToken))
        {
            _stopped = false;
            _timer.Start();

            return Task.FromResult(true);
        }

        public Task Stop(CancellationToken token = default(CancellationToken))
        {
            InternalStop();
            
            return Task.FromResult(true);
        }

        void InternalStop()
        {
            _stopped = true;
            _timer.Stop();
        }

        public async Task Connect(IPipe<FileContext> pipe, CancellationToken token = new CancellationToken())
        {
            _timer.Elapsed += (sender, args) =>
            {
                pipe.Send(new FileContext()
                {
                    FilePath = DateTime.Now.ToString()
                });
            };
            
            while (!token.IsCancellationRequested || !_stopped)
            {
                if (token.IsCancellationRequested || _stopped)
                {
                    //Stop the timer when cancelled. Calling stop twice on the timer is ok.
                    InternalStop();
                    break;
                }

                await Task.Delay(1);

            }

            _timer.Stop();
           
        }
    }


    public class FileContext : BasePipeContext, PipeContext
    {
        public FileContext() : base(new PayloadCache())
        {

        }

        public string FilePath { get; set; }
    }


}