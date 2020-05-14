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
