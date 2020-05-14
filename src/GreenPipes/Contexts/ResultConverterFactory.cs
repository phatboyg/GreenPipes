namespace GreenPipes.Contexts
{
    using System;
    using System.Linq;
    using Filters;
    using Internals.Extensions;


    public class ResultConverterFactory :
        IPipeContextConverterFactory<ResultContext>
    {
        IPipeContextConverter<ResultContext, TOutput> IPipeContextConverterFactory<ResultContext>.GetConverter<TOutput>()
        {
            if (typeof(TOutput).HasInterface<ResultContext>())
            {
                Type[] innerType = typeof(TOutput).GetClosingArguments(typeof(ResultContext<,>)).ToArray();

                return (IPipeContextConverter<ResultContext, TOutput>)Activator.CreateInstance(typeof(ResultContextConverter<,>).MakeGenericType(innerType));
            }

            throw new ArgumentException($"The output type is not supported: {TypeCache<TOutput>.ShortName}", nameof(TOutput));
        }


        class ResultContextConverter<TRequest, TResult> :
            IPipeContextConverter<ResultContext, ResultContext<TRequest, TResult>>
            where TRequest : class
            where TResult : class
        {
            bool IPipeContextConverter<ResultContext, ResultContext<TRequest, TResult>>.TryConvert(ResultContext input,
                out ResultContext<TRequest, TResult> output)
            {
                output = input as ResultContext<TRequest, TResult>;

                return output != null;
            }
        }
    }
}
