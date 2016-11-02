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
namespace GreenPipes.Pipes.PipeContexts
{
    using System;
    using Internals.Extensions;


    public class PipeResultContext<TRequest, TResult> :
        BasePipeContext,
        ResultContext<TRequest, TResult>
        where TRequest : class
        where TResult : class
    {
        public PipeResultContext(TRequest request, TResult result)
        {
            Request = request;
            Result = result;
        }

        public TResult Result { get; }

        public TRequest Request { get; }

        T ResultContext.GetResult<T>()
        {
            var result = Result as T;
            if (result == null)
                throw new ArgumentException("The result is not assignable to the specified type: " + TypeCache<T>.ShortName);

            return result;
        }

        bool ResultContext.TryGetResult<T>(out T result)
        {
            result = Result as T;

            return result != default(T);
        }
    }
}