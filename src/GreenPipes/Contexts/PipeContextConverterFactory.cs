﻿// Copyright 2012-2016 Chris Patterson
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
    using Contracts;
    using Filters;
    using Internals.Extensions;


    public class PipeContextConverterFactory :
        IPipeContextConverterFactory<PipeContext>
    {
        IPipeContextConverter<PipeContext, TOutput> IPipeContextConverterFactory<PipeContext>.GetConverter<TOutput>()
        {
            if (typeof(TOutput).HasInterface<CommandContext>())
            {
                var innerType = typeof(TOutput).GetClosingArguments(typeof(CommandContext<>)).Single();

                return (IPipeContextConverter<PipeContext, TOutput>)Activator.CreateInstance(typeof(CommandContextConverter<>).MakeGenericType(innerType));
            }

            if (typeof(TOutput).HasInterface<EventContext>())
            {
                var innerType = typeof(TOutput).GetClosingArguments(typeof(EventContext<>)).Single();

                return (IPipeContextConverter<PipeContext, TOutput>)Activator.CreateInstance(typeof(EventContextConverter<>).MakeGenericType(innerType));
            }

            throw new ArgumentException($"The output type is not supported: {TypeCache<TOutput>.ShortName}", nameof(TOutput));
        }


        class CommandContextConverter<T> :
            IPipeContextConverter<PipeContext, CommandContext<T>>
            where T : class
        {
            bool IPipeContextConverter<PipeContext, CommandContext<T>>.TryConvert(PipeContext input, out CommandContext<T> output)
            {
                output = input as CommandContext<T>;

                return output != null;
            }
        }


        class EventContextConverter<T> :
            IPipeContextConverter<PipeContext, EventContext<T>>
            where T : class
        {
            bool IPipeContextConverter<PipeContext, EventContext<T>>.TryConvert(PipeContext input, out EventContext<T> output)
            {
                output = input as EventContext<T>;

                return output != null;
            }
        }
    }
}