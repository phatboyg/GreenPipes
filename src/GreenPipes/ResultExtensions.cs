namespace GreenPipes
{
    using System.Threading.Tasks;


    public static class ResultExtensions
    {
        public static async Task<TResult> Result<TResult>(this Task<ResultContext> result)
            where TResult : class
        {
            var resultContext = await result.ConfigureAwait(false);

            return resultContext.GetResult<TResult>();
        }

        public static async Task<TResult> Result<TResult>(this Task<ResultContext<TResult>> result)
            where TResult : class
        {
            ResultContext<TResult> resultContext = await result.ConfigureAwait(false);

            return resultContext.Result;
        }
    }
}
