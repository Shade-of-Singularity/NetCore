using NetCore.Transports;
using NetCore.Transports.Special;
using NetCore.Transports.TCP;
using NetCore.Transports.UDP;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Constructor delegate providing essential <see cref="CustomHeaders"/> to the input <see cref="Header"/>.
    /// </summary>
    /// <param name="header">Header to modify.</param>
    public delegate void HeaderConstructor(ref Header header);
    /// <summary>
    /// Constructor delegate providing essential <see cref="CustomHeaders"/> to the input <see cref="Flags"/>.
    /// </summary>
    /// <param name="flags">Flags to modify.</param>
    public delegate void FlagsConstructor(ref Flags flags);

    /// <summary>
    /// Extensions methods for working with <see cref="NetworkMember"/>s.
    /// </summary>
    public static class NetworkMemberExtensions
    {
        /// <summary>
        /// Registers specific <paramref name="transport"/> as both <see cref="IReliableTransport"/> and <see cref="IUnreliableTransport"/>
        /// in a target <see cref="NetworkMember"/> instance.
        /// </summary>
        /// <typeparam name="T">Specific type of <see cref="ITransport"/>.</typeparam>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <param name="transport">Transport to register in a target <paramref name="member"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTransportAsBoth<T>(this NetworkMember member, T transport) where T : class, IReliableTransport, IUnreliableTransport
        {
            member.RegisterReliableTransport(transport);
            member.RegisterUnreliableTransport(transport);
        }

        /// <summary>
        /// Checks if given <see cref="NetworkMember"/> (<paramref name="member"/>) managed transport of a type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Specific type of <see cref="ITransport"/>.</typeparam>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <returns>
        /// <c>true</c> if transport was found.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTransport<T>(this NetworkMember member) where T : class, IReliableTransport, IUnreliableTransport
        {
            return member.HasReliableTransport<T>() || member.HasUnreliableTransport<T>();
        }

        /// <summary>
        /// Registers both <see cref="TCPTransport"/> and <see cref="UDPTransport"/>, and configures them to work together:
        /// <para><see cref="UDPTransport"/> - sends only unreliable messages.</para>
        /// <para><see cref="TCPTransport"/> - sends only reliable messages.</para>
        /// </summary>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <param name="transport">Transport handing both reliable and unreliable messages.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTCPUDPTransport(this NetworkMember member, TCPUDPTransport transport)
        {
            member.RegisterReliableTransport(transport);
            member.RegisterUnreliableTransport(transport);
        }

        /// <summary>
        /// Instantiates and automatically registers given <typeparamref name="TTransport"/> in a given <see cref="NetworkMember"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="NetworkMember.RegisterReliableTransport{T}(T)"/> directly if you want to assign transport to multiple transports.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to instantiate.</typeparam>
        /// <param name="member"><see cref="NetworkMember"/> </param>
        public static void RegisterReliableTransport<TTransport>(this NetworkMember member) where TTransport : class, IReliableTransport, new()
        {
            member.RegisterReliableTransport(new TTransport());
        }
    }
}
