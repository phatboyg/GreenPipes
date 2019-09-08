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


    public static class EventExtensions
    {
        public static Task PublishEvent<T>(this IPipe<EventContext> pipe, T message)
            where T : class
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var context = new Event<T>(message);

            return pipe.Send(context);
        }


        class Event<T> :
            BasePipeContext,
            EventContext<T>
            where T : class
        {
            readonly T _event;

            public Event(T @event)
            {
                _event = @event;
                Timestamp = DateTime.UtcNow;
            }

            public DateTime Timestamp { get; }

            T EventContext<T>.Event => _event;
        }
    }
}