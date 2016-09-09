﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Threading.Tasks;
    using Contracts;
    using Control;
    using Filters;


    public class ControlPipe :
        IControlPipe
    {
        readonly DispatchFilter<CommandContext> _filter;
        readonly IPipe<CommandContext> _pipe;

        public ControlPipe()
        {
            _filter = new DispatchFilter<CommandContext>(new CommandContextConverterFactory());
            _pipe = Pipe.New<CommandContext>(x => x.UseFilter(_filter));
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("managementPipe");

            _pipe.Probe(scope);
        }

        Task IPipe<CommandContext>.Send(CommandContext context)
        {
            return _pipe.Send(context);
        }

        ConnectHandle IPipeConnector.ConnectPipe<T>(IPipe<T> pipe)
        {
            return _filter.ConnectPipe(pipe);
        }

        public ConnectHandle ConnectObserver<T>(IFilterObserver<T> observer) where T : class, PipeContext
        {
            return ((IObserverConnector)_filter).ConnectObserver(observer);
        }

        public ConnectHandle ConnectObserver(IFilterObserver observer)
        {
            return ((IObserverConnector)_filter).ConnectObserver(observer);
        }
    }
}