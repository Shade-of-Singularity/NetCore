using NetCore.Messaging;

namespace NetCore.Transports
{
    /// <summary>
    /// (Unreliable, Unordered) Transport for sending messages.
    /// </summary>
    /// <seealso cref="SendingMode.Unreliable"/>
    public interface IUnreliableTransport : ITransport, ISendUnreliable, IHandleUnreliable { }
}
