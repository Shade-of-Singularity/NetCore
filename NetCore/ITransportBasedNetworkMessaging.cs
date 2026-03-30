using NetCore.Transports;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Base interface for messaging interfaces, defining methods required for them.
    /// </summary>
    /// Note: Methods defined here are used by Internal AutoGen for providing better API.
    public interface ITransportBasedNetworkMessaging
    {
        /// <summary>
        /// Check whether transport of a specified type is registered.
        /// </summary>
        bool HasTransport<TTransport>() where TTransport : ITransport;
        /// <summary>
        /// Checks whether transport of a specified type is registered and belongs to a specific sending mode group.
        /// </summary>
        bool HasTransport<TTransport>(SendingMode mode) where TTransport : ITransport;
        /// <summary>
        /// Checks if there is any transport, at all, than will be able to handle a given <see cref="SendingMode"/>.
        /// Used in broadcasting TrySend methods.
        /// </summary>
        bool HasAnyTransport(SendingMode mode);
        /// <summary>
        /// Checks whether this network member manages any unreliable transports.
        /// </summary>
        bool HasAnyUnreliableTransport();
        /// <summary>
        /// Checks whether this network member manages a specific unreliable transport.
        /// </summary>
        bool HasUnreliableTransport<TTransport>() where TTransport : class, IUnreliableTransport;
        /// <summary>
        /// Checks whether this network member manages any reliable transports.
        /// </summary>
        bool HasAnyReliableTransport();
        /// <summary>
        /// Checks whether this network member manages a specific reliable transport.
        /// </summary>
        bool HasReliableTransport<TTransport>() where TTransport : class, IReliableTransport;
        /// <summary>
        /// Checks whether this network member manages any sequential transports.
        /// </summary>
        bool HasAnySequentialTransport();
        /// <summary>
        /// Checks whether this network member manages a specific sequential transport.
        /// </summary>
        bool HasSequentialTransport<TTransport>() where TTransport : class, ISequentialTransport;
        /// <summary>
        /// Checks whether this network member manages any resilient transports.
        /// </summary>
        bool HasAnyResilientTransport();
        /// <summary>
        /// Checks whether this network member manages a specific resilient transport.
        /// </summary>
        bool HasResilientTransport<TTransport>() where TTransport : class, IResilientTransport;
    }
}
