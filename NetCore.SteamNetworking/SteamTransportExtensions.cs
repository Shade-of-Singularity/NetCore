using System.Diagnostics.CodeAnalysis;

namespace NetCore.SteamNetworking
{
    /// <summary>
    /// Extensions for working with <see cref="SteamTransport"/> more easily.
    /// </summary>
    /// TODO: Cover all such extensions with Auto-gen.
    public static partial class SteamTransportExtensions
    {
        /// <summary>
        /// Registers <see cref="SteamTransport"/> instance in <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="SteamTransport"/> to register.</param>
        public static void RegisterTransport(this NetworkMember member, SteamTransport transport)
        {
            member.RegisterUnreliableTransport(transport);
            member.RegisterReliableTransport(transport);
        }

        /// <summary>
        /// Removes <see cref="SteamTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="SteamTransport"/> to remove.</param>
        public static bool RemoveTransport(this NetworkMember member, SteamTransport transport)
        {
            bool result = false;
            result |= member.RemoveUnreliableTransport(transport);
            result |= member.RemoveReliableTransport(transport);
            return result;
        }

        /// <summary>
        /// Removes <see cref="SteamTransport"/> instance from this <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> to work with.</param>
        /// <param name="transport"><see cref="SteamTransport"/> to remove.</param>
        public static bool RemoveTransport(this NetworkMember member, [NotNullWhen(true)] out SteamTransport? transport)
        {
            transport = default;
            if (member.RemoveUnreliableTransport(out SteamTransport? t)) { transport = t; }
            if (member.RemoveReliableTransport(out t)) { transport = t; }
            return transport is not null;
        }
    }
}
