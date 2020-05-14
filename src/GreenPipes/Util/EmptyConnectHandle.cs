namespace GreenPipes.Util
{
    using System;


    /// <summary>
    /// A do-nothing connect handle, simply to satisfy
    /// </summary>
    public class EmptyConnectHandle :
        ConnectHandle
    {
        void IDisposable.Dispose()
        {
        }

        void ConnectHandle.Disconnect()
        {
        }
    }
}
