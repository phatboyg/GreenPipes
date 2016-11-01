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
                var outputContext = input as RequestContext<TRequest>;

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