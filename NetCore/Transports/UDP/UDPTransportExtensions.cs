using NetCore;
using System.Diagnostics.CodeAnalysis;

namespace NetCore.Transports.UDP
{
    /// <summary>
    /// Useful extensions when working with <see cref="UDPTransport"/>s.
    /// </summary>
    /// TODO: Cover some methods with auto-gen.
    public static partial class UDPTransportExtensions
    {
        /// <summary>
        /// Registers <see cref="UDPTransport"/> instance in <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="UDPTransport"/> to register.</param>
        public static void RegisterTransport(this NetworkMember member, UDPTransport transport)
        {
            member.RegisterUnreliableTransport(transport);
        }

        /// <summary>
        /// Removes <see cref="UDPTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="UDPTransport"/> to remove.</param>
        public static bool RemoveTransport(this NetworkMember member, UDPTransport transport)
        {
            return member.RemoveUnreliableTransport(transport);
        }

        /// <summary>
        /// Removes <see cref="UDPTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="UDPTransport"/> to remove.</param>
        public static bool RemoveTransport(this NetworkMember member, [NotNullWhen(true)] out UDPTransport? transport)
        {
            return member.RemoveUnreliableTransport(out transport);
        }
    }
}
