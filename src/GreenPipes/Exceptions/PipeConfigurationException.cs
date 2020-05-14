namespace GreenPipes
{
    using System;
    using System.Runtime.Serialization;


    [Serializable]
    public class PipeConfigurationException :
        Exception
    {
        public PipeConfigurationException()
        {
        }

        public PipeConfigurationException(string message)
            : base(message)
        {
        }

        public PipeConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PipeConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
