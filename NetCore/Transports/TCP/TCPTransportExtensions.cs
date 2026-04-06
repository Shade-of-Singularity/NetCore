using System.Diagnostics.CodeAnalysis;

namespace NetCore.Transports.TCP
{
    /// <summary>
    /// Useful extensions when working with <see cref="TCPTransport"/>s.
    /// </summary>
    /// TODO: Cover some methods with auto-gen.
    public static partial class TCPTransportExtensions
    {
        /// <summary>
        /// Registers <see cref="TCPTransport"/> instance in <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="TCPTransport"/> to register.</param>
        public static void RegisterTransport(this NetworkMember member, TCPTransport transport)
        {
            member.RegisterReliableTransport(transport);
            // member.RegisterStreamTransport(transport)
        }

        /// <summary>
        /// Removes <see cref="TCPTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="TCPTransport"/> to remove.</param>
        public static bool RemoveTransport(this NetworkMember member, TCPTransport transport)
        {
            return member.RemoveReliableTransport(transport);
        }

        /// <summary>
        /// Removes <see cref="TCPTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="TCPTransport"/> to remove.</param>
        public static bool RemoveTransport(this NetworkMember member, [NotNullWhen(true)] out TCPTransport? transport)
        {
            return member.RemoveReliableTransport(out transport);
        }
    }
}
