namespace GreenPipes
{
    using System.Threading.Tasks;
    using Contracts;


    public static class ConcurrencyLimitExtensions
    {
        /// <summary>
        /// Set the concurrency limit of the filter
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="concurrencyLimit"></param>
        /// <returns></returns>
        public static Task SetConcurrencyLimit(this IPipe<CommandContext> pipe, int concurrencyLimit)
        {
            return pipe.SendCommand<SetConcurrencyLimit>(new Limit(concurrencyLimit));
        }


        class Limit :
            SetConcurrencyLimit
        {
            public Limit(int concurrencyLimit)
            {
                ConcurrencyLimit = concurrencyLimit;
            }

            public int ConcurrencyLimit { get; }
        }
    }
}
