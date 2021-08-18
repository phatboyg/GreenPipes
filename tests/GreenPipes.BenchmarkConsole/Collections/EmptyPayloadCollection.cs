namespace GreenPipes.BenchmarkConsole.Collections
{
    public class EmptyPayloadCollection :
        BasePayloadCollection
    {
        public EmptyPayloadCollection(IReadOnlyPayloadCollection parent = null)
            : base(parent)
        {
        }

        public override IPayloadCollection Add(IPayloadValue payload)
        {
            return new SinglePayloadCollection(payload, Parent);
        }


        internal static class Shared
        {
            public static readonly EmptyPayloadCollection Empty = new EmptyPayloadCollection();
        }
    }
}
