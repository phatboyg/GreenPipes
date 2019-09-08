// Copyright 2012-2018 Chris Patterson
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
namespace GreenPipes
{
    using System;
    using System.Threading.Tasks;
    using Contracts;


    public static class CommandExtensions
    {
        public static Task SendCommand<T>(this IPipe<CommandContext> pipe, T command)
            where T : class
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var context = new Command<T>(command);

            return pipe.Send(context);
        }


        class Command<T> :
            BasePipeContext,
            CommandContext<T>
            where T : class
        {
            readonly T _command;

            public Command(T command)
            {
                _command = command;
                Timestamp = DateTime.UtcNow;
            }

            public DateTime Timestamp { get; }

            T CommandContext<T>.Command => _command;
        }
    }
}