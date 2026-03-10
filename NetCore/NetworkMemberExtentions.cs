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
    }
}
