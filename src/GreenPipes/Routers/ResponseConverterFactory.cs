// Copyright 2012-2016 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.Routers
{
    using System;
    using System.Linq;
    using Filters;
    using Internals.Extensions;


    public class ResponseConverterFactory :
        IPipeContextConverterFactory<ResultContext>
    {
        IPipeContextConverter<ResultContext, TOutput> IPipeContextConverterFactory<ResultContext>.GetConverter<TOutput>()
        {
            if (typeof(TOutput).HasInterface<ResultContext>())
            {
                var innerType = typeof(TOutput).GetClosingArguments(typeof(ResultContext<>)).Single();

                return (IPipeContextConverter<PipeContext, TOutput>)Activator.CreateInstance(typeof(ResponseContextConverter<>).MakeGenericType(innerType));
            }

            throw new ArgumentException($"The output type is not supported: {TypeCache<TOutput>.ShortName}", nameof(TOutput));
        }


        class ResponseContextConverter<TResponse> :
            IPipeContextConverter<ResultContext, ResultContext<TResponse>>
            where TResponse : class
        {
            bool IPipeContextConverter<ResultContext, ResultContext<TResponse>>.TryConvert(ResultContext input,
                out ResultContext<TResponse> output)
            {
                var outputContext = input as ResultContext<TResponse>;

                if (outputContext != null)
                {
                    output = outputContext;
                    return true;
                }

                output = null;
                return false;
            }
        }
    }
}