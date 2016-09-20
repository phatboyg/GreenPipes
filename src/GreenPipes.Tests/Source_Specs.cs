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
        [Test]
        public async Task X()
        {
            var ts = new CancellationTokenSource();
            int i = 0;

            var pipe = Pipe.New<FileContext>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    i++;
                    Console.WriteLine(cxt.FilePath);
                });
            });

            var source = new TestContextSource();

            source.Fill(pipe, ts.Token);

            await Task.Delay(100);

            ts.Cancel();
            Console.WriteLine("Cancelled");
            //TODO: not sure how to best test that the cancel was propagated.
            int j = i;

            await Task.Delay(100);

            Assert.That(ts.IsCancellationRequested, Is.True);
            Assert.That(i, Is.GreaterThan(1));
            //should stop in a few cycles
            Assert.That(j, Is.LessThan(i+2));
        }

    }

    public class TestContextSource : IContextSource<FileContext>
    {
        Timer _timer;

        public async Task Fill(IPipe<FileContext> pipe, CancellationToken token = new CancellationToken())
        {
            _timer = new Timer(20);
            _timer.Start();
            _timer.Elapsed += (sender, args) =>
            {
                pipe.Send(new FileContext()
                {
                    FilePath = "A:"+DateTime.Now
                });
            };

            while (!token.IsCancellationRequested)
            {
                if (token.IsCancellationRequested)
                    break;

                await Task.Delay(1);

            }

            _timer.Stop();
           
        }
    }
    public class FileContextSource : IContextSource<FileContext>
    {
        public async Task Fill(IPipe<FileContext> pipe, CancellationToken token = new CancellationToken())
        {
            //I know its not the best, just using it as a demo
            using (var fw = new FileSystemWatcher())
            {
                fw.Created += BuildHandler(pipe);

                while (!token.IsCancellationRequested)
                {
                    //pause here so we don't thrash???
                    await Task.Delay(100, token);
                }
            }
        }

        FileSystemEventHandler BuildHandler(IPipe<FileContext> pipe)
        {
            return (s, e) =>
            {
                var cxt = new FileContext();
                cxt.FilePath = e.FullPath;
                //not awaiting so i don't block
                pipe.Send(cxt);
            };
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