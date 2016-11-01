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
namespace GreenPipes.Pipes
{
    using System;
    using System.Threading.Tasks;


    public class PipeRequestContext<TRequest> :
        BasePipeContext,
        RequestContext<TRequest>
    {
        public PipeRequestContext(TRequest request)
        {
            Request = request;
        }

        public TRequest Request { get; }

        public Task<bool> TrySetResult<TResponse>(TResponse result)
        {
            throw new NotImplementedException();
        }

        public bool TrySetException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public bool TrySetCanceled()
        {
            throw new NotImplementedException();
        }

        public Task<bool> TrySetResponse<TResponse>(Task<TResponse> response)
        {
            throw new NotImplementedException();
        }
    }
}