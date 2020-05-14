namespace GreenPipes.Contexts
{
    using System;
    using System.Linq;
    using Filters;
    using Internals.Extensions;


    public class RequestConverterFactory :
        IPipeContextConverterFactory<PipeContext>
    {
        IPipeContextConverter<PipeContext, TOutput> IPipeContextConverterFactory<PipeContext>.GetConverter<TOutput>()
        {
            if (typeof(TOutput).HasInterface<RequestContext>())
            {
                var innerType = typeof(TOutput).GetClosingArguments(typeof(RequestContext<>)).Single();

                return (IPipeContextConverter<PipeContext, TOutput>)Activator.CreateInstance(typeof(RequestContextConverter<>).MakeGenericType(innerType));
            }

            throw new ArgumentException($"The output type is not supported: {TypeCache<TOutput>.ShortName}", nameof(TOutput));
        }


        class RequestContextConverter<TRequest> :
            IPipeContextConverter<PipeContext, RequestContext<TRequest>>
            where TRequest : class
        {
            bool IPipeContextConverter<PipeContext, RequestContext<TRequest>>.TryConvert(PipeContext input,
                out RequestContext<TRequest> output)
            {
                output = input as RequestContext<TRequest>;

                return output != null;
            }
        }
    }
}
