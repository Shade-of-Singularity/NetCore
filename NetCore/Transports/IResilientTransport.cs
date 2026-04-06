using NetCore.Messaging;

namespace NetCore.Transports
{
    /// <summary>
    /// (Reliable, Ordered) Transport for sending messages.
    /// </summary>
    /// <seealso cref="SendingMode.Resilient"/>
    public interface IResilientTransport : ITransport, ISendResilient, IHandleResilient{ }
}
