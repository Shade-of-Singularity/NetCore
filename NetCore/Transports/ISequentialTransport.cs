using NetCore.Messaging;

namespace NetCore.Transports
{
    /// <summary>
    /// (Unreliable, Ordered) Transport for sending messages.
    /// </summary>
    /// <seealso cref="SendingMode.Sequential"/>
    public interface ISequentialTransport : ITransport, ISendSequential, IHandleSequential { }
}
