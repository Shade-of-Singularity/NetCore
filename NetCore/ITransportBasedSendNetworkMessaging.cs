using NetCore.Transports;
using System;

namespace NetCore
{
    /// <summary>
    /// Interface for working with session-based message sending methods.
    /// </summary>
    /// Note: Internal AutoGen covered.
    public partial interface ITransportBasedSendNetworkMessaging : ITransportBasedNetworkMessaging
    {
        /// <summary>
        /// <para>Unreliably sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendUnreliable<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IUnreliableTransport;
        /// <summary>
        /// <para>Reliably sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendReliable<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IReliableTransport;
        /// <summary>
        /// <para>Sequentially sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendSequential<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, ISequentialTransport;
        /// <summary>
        /// <para>Resiliently sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendResilient<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IResilientTransport;
    }
}
