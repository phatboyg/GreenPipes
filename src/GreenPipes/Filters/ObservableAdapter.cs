// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace GreenPipes.Filters
{
    using System;
    using System.Threading.Tasks;


    public class ObservableAdapter<TContext> :
        IFilterObserver<TContext>
        where TContext : class, PipeContext
    {
        readonly IFilterObserver _observer;

        public ObservableAdapter(IFilterObserver observer)
        {
            _observer = observer;
        }

        public Task PreSend(TContext context)
        {
            return _observer.PreSend(context);
        }

        public Task PostSend(TContext context)
        {
            return _observer.PostSend(context);
        }

        public Task SendFault(TContext context, Exception exception)
        {
            return _observer.SendFault(context, exception);
        }
    }
}