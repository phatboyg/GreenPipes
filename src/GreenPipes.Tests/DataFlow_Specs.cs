namespace GreenPipes.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using NUnit.Framework;


    [TestFixture]
    public class DataFlow_Specs
    {
        [Test]
        public async Task TEST_NAME()
        {
            var pipe = Pipe.New<Input>(x => x.UseExecuteAsync(context => Console.Out.WriteLineAsync($"{context.Value}")));

            ExecutionDataflowBlockOptions options = new ExecutionDataflowBlockOptions()
            {
                BoundedCapacity = 1000,
                MaxDegreeOfParallelism = 16,
                SingleProducerConstrained = true,
            };

            var pipeBlock = new ActionBlock<Input>(pipe.Send, options);
            var transformBlock = new TransformBlock<string, Input>(x => new Input(x));
            transformBlock.LinkTo(pipeBlock);

            await Task.WhenAll(Enumerable.Range(0, 1000).Select(n => transformBlock.SendAsync($"Item: {n}")));

            transformBlock.Complete();

            await transformBlock.Completion;

            pipeBlock.Complete();

            await pipeBlock.Completion;

        }
    }
}