using System.Diagnostics.CodeAnalysis;

namespace NetCore.Transports.Special
{
    /// <summary>
    /// Useful extensions when working with <see cref="TCPUDPTransport"/>s.
    /// </summary>
    /// TODO: Cover some methods with auto-gen.
    public static partial class TCPUDPTransportExtensions
    {
        /// <summary>
        /// Registers <see cref="TCPUDPTransport"/> instance in <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="TCPUDPTransport"/> to register.</param>
        public static void RegisterTCPUDPTransport(this NetworkMember member, TCPUDPTransport transport)
        {
            member.RegisterUnreliableTransport(transport);
            member.RegisterReliableTransport(transport);
        }

        /// <summary>
        /// Removes <see cref="TCPUDPTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="TCPUDPTransport"/> to remove.</param>
        public static bool RemoveTCPUDPTransport(this NetworkMember member, TCPUDPTransport transport)
        {
            bool result = false;
            result |= member.RemoveUnreliableTransport(transport);
            result |= member.RemoveReliableTransport(transport);
            return result;
        }

        /// <summary>
        /// Removes <see cref="TCPUDPTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="TCPUDPTransport"/> to remove.</param>
        public static bool RemoveTCPUDPTransport(this NetworkMember member, [NotNullWhen(true)] out TCPUDPTransport? transport)
        {
            transport = default;
            if (member.RemoveUnreliableTransport(out TCPUDPTransport? t)) { transport = t; }
            if (member.RemoveReliableTransport(out t)) { transport = t; }
            return transport is not null;
        }
    }
}
