namespace NetCore
{
    /// <summary>
    /// Extensions methods for working with <see cref="NetworkMember"/>s.
    /// </summary>
    public static class NetworkMemberExtensions
    {
        /// <summary>
        /// Registers specific <paramref name="transport"/> as both <see cref="IReliableTransport"/> and <see cref="IUnreliableTransport"/>
        /// in a target <see cref="NetworkMember"/> instance.
        /// </summary>
        /// <typeparam name="T">Specific type of <see cref="ITransport"/></typeparam>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <param name="transport">Transport to register in a target <paramref name="member"/>.</param>
        public static void RegisterTransportAsBoth<T>(this NetworkMember member, T transport) where T : class, IReliableTransport, IUnreliableTransport
        {
            member.RegisterReliableTransport(transport);
            member.RegisterUnreliableTransport(transport);
        }

        /// <summary>
        /// Registers both <see cref="TCP.TCPTransport"/> and <see cref="UDP.UDPTransport"/>, and configures them to work together:
        /// <para><paramref name="udp"/> - sends only unreliable messages.</para>
        /// <para><paramref name="tcp"/> - sends only reliable messages.</para>
        /// </summary>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <param name="tcp">TCP Transport.</param>
        /// <param name="udp">UDP Transport.</param>
        public static void RegisterTCPUDPPair(this NetworkMember member, TCP.TCPTransport tcp, UDP.UDPTransport udp)
        {
            udp.TCPTransport = tcp;
            tcp.UDPTransport = udp;
            member.RegisterReliableTransport(tcp);
            member.RegisterUnreliableTransport(udp);
        }
    }
}
