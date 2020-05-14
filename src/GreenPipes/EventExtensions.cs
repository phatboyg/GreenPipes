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
