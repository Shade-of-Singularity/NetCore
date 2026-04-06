using NetCore.Messaging;

namespace NetCore.Transports
{
    /// <summary>
    /// (Reliable, Unordered) Transport for sending messages.
    /// </summary>
    /// <seealso cref="SendingMode.Reliable"/>
    public interface IReliableTransport : ITransport, ISendReliable, IHandleReliable { }
}
