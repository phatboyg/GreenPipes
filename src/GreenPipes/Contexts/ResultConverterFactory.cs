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