using NetCore.Common;
using NetCore.Loopback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace NetCore
{
    /// <summary>
    /// Base class for <see cref="Server"/> and <see cref="Client"/>.
    /// </summary>
    /// <remarks>
    /// If you need something other than <see cref="IReliableTransport"/> or <see cref="IUnreliableTransport"/>
    /// - fork the project and modify this base class, or define the same logic in a custom <see cref="Client"/> and <see cref="Server"/> class.
    /// </remarks>
    /// TODO: Add a way to map connections, arrived from different transports
    /// to either one connection (TCP+UDP to one)
    /// or to multiple(UDP + SteamUDP to separate).
    public abstract class NetworkMember
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default of 3 for usually default transports: <see cref="TCP.TCPTransport"/>, <see cref="UDP.UDPTransport"/> and <see cref="Loopback.LoopbackTransport"/>.
        /// </summary>
        public const int DefaultInitialTransportCapacity = 3;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Whether this <see cref="NetworkMember"/> was started and <see cref="LocalEndPoint"/> was set.
        /// </summary>
        public bool IsActive => LocalEndPoint is not null;

        /// <summary>
        /// Local end-point on which network member was started.
        /// </summary>
        public IPEndPoint? LocalEndPoint { get; private set; }

        /// <summary>
        /// Remote end-point to which this <see cref="NetworkMember"/> is connected to.
        /// </summary>
        /// <remarks>
        /// When used in <see cref="Server"/> - represent end-point of an relay server.
        /// </remarks>
        public IPEndPoint? RemoteEndPoint { get; private set; }

        /// <summary>
        /// Instance of <see cref="Loopback.LoopbackTransport"/> added by default
        /// to both <see cref="ReliableTransports"/> and <see cref="UnreliableTransports"/>
        /// </summary>
        public LoopbackTransport LoopbackTransport => m_LoopbackTransport;

        /// <summary>
        /// Provider for all <see cref="ConnectionID"/>s managed by this <see cref="NetworkMember"/> and its transports.
        /// </summary>
        public ConnectionIDProvider CIDProvider => m_CIDProvider;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static readonly Dictionary<RuntimeTypeHandle, int> m_InitialTransportCapacity = new(2); // 2 - for Server and Client, by default.




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Dictionary with all <see cref="ITransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected QuickMap<IReliableTransport> ReliableTransports;
        /// <summary>
        /// Dictionary with all <see cref="ITransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected QuickMap<IUnreliableTransport> UnreliableTransports;
        /// <summary>
        /// Lock used everywhere, including for accessing <see cref="ReliableTransports"/> and <see cref="UnreliableTransports"/> maps.
        /// </summary>
        protected readonly object _lock = new();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Local reference to our own <see cref="Loopback.LoopbackTransport"/>
        /// </summary>
        private readonly LoopbackTransport m_LoopbackTransport;
        /// <summary>
        /// Provider for Connection IDs.
        /// </summary>
        private readonly ConnectionIDProvider m_CIDProvider = new();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public NetworkMember()
        {
            int capacity;
            lock (m_InitialTransportCapacity)
            {
                if (!m_InitialTransportCapacity.TryGetValue(GetType().TypeHandle, out capacity))
                {
                    capacity = DefaultInitialTransportCapacity;
                }
            }

            ReliableTransports = new(capacity);
            UnreliableTransports = new(capacity);
            m_LoopbackTransport = new();
            ReliableTransports.Add(m_LoopbackTransport);
            UnreliableTransports.Add(m_LoopbackTransport);
            ((ITransport)m_LoopbackTransport).InvokeInitialize(this);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Binds all registered <see cref="ITransport"/>s to an <paramref name="localEndPoint"/> and marks instance as active.
        /// </summary>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: already started, invalid endPoint, custom errors from transports, etc).
        /// </returns>
        public virtual bool Start(IPEndPoint localEndPoint)
        {
            lock (_lock)
            {
                if (LocalEndPoint is not null)
                {
                    return false;
                }

                foreach (var transport in ReliableTransports)
                {
                    if (!transport.InvokeStart(localEndPoint))
                        goto ResetState;
                }

                foreach (var transport in UnreliableTransports)
                {
                    if (!transport.InvokeStart(localEndPoint))
                        goto ResetState;
                }

                LocalEndPoint = localEndPoint;
                return true;

                ResetState:
                StopInternal();
                return false;
            }
        }

        /// <summary>
        /// Unbinds and stops all registered <see cref="ITransport"/>s and marks instance as inactive.
        /// </summary>
        public virtual void Stop()
        {
            lock (_lock)
            {
                if (LocalEndPoint is not null)
                {
                    StopInternal();
                    LocalEndPoint = null;
                }
            }
        }

        private void StopInternal()
        {
            foreach (var transport in ReliableTransports)
            {
                transport.InvokeStop();
            }

            foreach (var transport in UnreliableTransports)
            {
                transport.InvokeStop();
            }
        }

        /// <summary>
        /// Connects this <see cref="NetworkMember"/> to a remote host.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - connects server to a relay and manages a NAT hole.
        /// </remarks>
        /// <param name="remoteEndPoint">Remote end-point to connect to.</param>
        public virtual bool Connect(IPEndPoint remoteEndPoint)
        {
            lock (_lock)
            {
                if (RemoteEndPoint is not null)
                {
                    Disconnect();
                }

                foreach (var transport in ReliableTransports)
                {
                    if (!transport.InvokeConnect(remoteEndPoint))
                        goto ResetState;
                }

                foreach (var transport in UnreliableTransports)
                {
                    if (!transport.InvokeConnect(remoteEndPoint))
                        goto ResetState;
                }

                RemoteEndPoint = remoteEndPoint;
                return true;

                ResetState:
                DisconnectInternal();
                return false;
            }
        }

        /// <summary>
        /// Disconnects this <see cref="NetworkMember"/> from a remote host.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - disconnects from a remote relay.
        /// </remarks>
        public virtual void Disconnect()
        {
            lock (_lock)
            {
                if (RemoteEndPoint is null)
                {
                    return;
                }

                DisconnectInternal();
                RemoteEndPoint = null;
            }
        }

        void DisconnectInternal()
        {
            foreach (var transport in ReliableTransports)
            {
                transport.InvokeDisconnect();
            }

            foreach (var transport in UnreliableTransports)
            {
                transport.InvokeDisconnect();
            }
        }

        #region Reliable transport registration/removal.
        /// <summary>
        /// Registers reliable transport.
        /// </summary>
        /// Note: Consider adding explicitly reliable-ordered and reliable-unordered.
        ///  Those can be already provided using TCP and UDP transport, but they will need a specific flag to differentiate which mode to use.
        public void RegisterReliableTransport<T>(T transport) where T : class, IReliableTransport
        {
            if (typeof(T) == typeof(LoopbackTransport))
            {
                throw new NotSupportedException($"Modifying reference to a {nameof(LoopbackTransport)} in {nameof(NetworkMember)} is not allowed.");
            }

            lock (_lock)
            {
                if (ReliableTransports.Remove(out T? removed))
                {
                    removed.InvokeTerminate(this);
                }

                ReliableTransports.Add(transport);
                transport.InvokeInitialize(this);
            }
        }

        /// <summary>
        /// Tries to remove <see cref="IReliableTransport"/> from the map of active transports.
        /// </summary>
        /// <typeparam name="T">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport which was removed just a moment ago.</param>
        /// <returns>
        /// <c>true</c> if transport was present, was removed and the instance is provided as <paramref name="transport"/>.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveReliableTransport<T>([NotNullWhen(true)] out T? transport) where T : class, IReliableTransport
        {
            if (typeof(T) == typeof(LoopbackTransport))
            {
                throw new NotSupportedException($"Modifying reference to a {nameof(LoopbackTransport)} in {nameof(NetworkMember)} is not allowed.");
            }

            lock (_lock)
            {
                if (ReliableTransports.Remove(out transport))
                {
                    transport.InvokeTerminate(this);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Tries to remove specific <paramref name="transport"/> from the map of active transports.
        /// </summary>
        /// <typeparam name="T">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport to remove.</param>
        /// <returns>
        /// <c>true</c> if transport was present and it was removed.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveReliableTransport<T>(T transport) where T : class, IReliableTransport
        {
            if (typeof(T) == typeof(LoopbackTransport))
            {
                throw new NotSupportedException($"Modifying reference to a {nameof(LoopbackTransport)} in {nameof(NetworkMember)} is not allowed.");
            }

            lock (_lock)
            {
                if (ReliableTransports.Remove(transport))
                {
                    transport.InvokeTerminate(this);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Checks whether this <see cref="NetworkMember"/> manages a specific reliable transport.
        /// </summary>
        public bool HasReliableTransport<T>() where T : class, IReliableTransport
        {
            lock (_lock)
            {
                return ReliableTransports.Has<T>();
            }
        }

        /// <summary>
        /// Tries to retrieve <see cref="IReliableTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <param name="transport">Transport instance or <c>null</c> when not found.</param>
        /// <returns>
        /// <c>true</c> if found and <paramref name="transport"/> was provided.
        /// <c>false</c> if not found and <paramref name="transport"/> is null.
        /// </returns>
        public bool TryGetReliableTransport<T>([NotNullWhen(true)] out T? transport) where T : class, IReliableTransport
        {
            lock (_lock)
            {
                return ReliableTransports.TryGet(out transport);
            }
        }

        /// <summary>
        /// Retrieves <see cref="IReliableTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <returns>Transport instance or <c>null</c> when not found.</returns>
        public T? GetReliableTransport<T>() where T : class, IReliableTransport
        {
            lock (_lock)
            {
                return ReliableTransports.Get<T>();
            }
        }
        #endregion

        #region Unreliable transport registration/removal.
        /// <summary>
        /// Registers reliable transport.
        /// </summary>
        /// Note: Consider adding explicitly unreliable-unordered and unreliable-ordered.
        ///  Those can be already provided using TCP and UDP transport, but they will need a specific flag to differentiate which mode to use.
        public void RegisterUnreliableTransport<T>(T transport) where T : class, IUnreliableTransport
        {
            if (typeof(T) == typeof(LoopbackTransport))
            {
                throw new NotSupportedException($"Modifying reference to a {nameof(LoopbackTransport)} in {nameof(NetworkMember)} is not allowed.");
            }

            lock (_lock)
            {
                if (UnreliableTransports.Remove(out T? removed))
                {
                    removed.InvokeTerminate(this);
                }

                UnreliableTransports.Add(transport);
                transport.InvokeInitialize(this);
            }
        }

        /// <summary>
        /// Tries to remove <see cref="IUnreliableTransport"/> from the map of active transports.
        /// </summary>
        /// <typeparam name="T">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport which was removed just a moment ago.</param>
        /// <returns>
        /// <c>true</c> if transport was present, was removed and the instance is provided as <paramref name="transport"/>.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveUnreliableTransport<T>([NotNullWhen(true)] out T? transport) where T : class, IUnreliableTransport
        {
            if (typeof(T) == typeof(LoopbackTransport))
            {
                throw new NotSupportedException($"Modifying reference to a {nameof(LoopbackTransport)} in {nameof(NetworkMember)} is not allowed.");
            }

            lock (_lock)
            {
                if (UnreliableTransports.Remove(out transport))
                {
                    transport.InvokeTerminate(this);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Tries to remove specific <paramref name="transport"/> from the map of active transports.
        /// </summary>
        /// <typeparam name="T">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport to remove.</param>
        /// <returns>
        /// <c>true</c> if transport was present and it was removed.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveUnreliableTransport<T>(T transport) where T : class, IUnreliableTransport
        {
            if (typeof(T) == typeof(LoopbackTransport))
            {
                throw new NotSupportedException($"Modifying reference to a {nameof(LoopbackTransport)} in {nameof(NetworkMember)} is not allowed.");
            }

            lock (_lock)
            {
                if (UnreliableTransports.Remove(transport))
                {
                    transport.InvokeTerminate(this);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Checks whether this <see cref="NetworkMember"/> manages a specific reliable transport.
        /// </summary>
        public bool HasUnreliableTransport<T>() where T : class, IUnreliableTransport
        {
            lock (_lock)
            {
                return UnreliableTransports.Has<T>();
            }
        }

        /// <summary>
        /// Tries to retrieve <see cref="IUnreliableTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <param name="transport">Transport instance or <c>null</c> when not found.</param>
        /// <returns>
        /// <c>true</c> if found and <paramref name="transport"/> was provided.
        /// <c>false</c> if not found and <paramref name="transport"/> is null.
        /// </returns>
        public bool TryGetUnreliableTransport<T>([NotNullWhen(true)] out T? transport) where T : class, IUnreliableTransport
        {
            lock (_lock)
            {
                return UnreliableTransports.TryGet(out transport);
            }
        }

        /// <summary>
        /// Retrieves <see cref="IUnreliableTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <returns>Transport instance or <c>null</c> when not found.</returns>
        public T? GetUnreliableTransport<T>() where T : class, IUnreliableTransport
        {
            lock (_lock)
            {
                return UnreliableTransports.Get<T>();
            }
        }
        #endregion




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Sets default initial capacity of the internal <see cref="QuickMap{T}"/> for <see cref="ITransport"/>s (see also: <see cref="ReliableTransports"/>).
        /// </summary>
        public static void SetDefaultTransportCapacity<T>(int capacity) where T : NetworkMember
        {
            if (capacity is < 0 or > QuickIndex.Limit)
            {
                throw new ArgumentOutOfRangeException($"{nameof(QuickMap<ITransport>)} capacity should be within bounds: [0:{QuickIndex.Limit}]. Provided: {capacity}");
            }

            lock (m_InitialTransportCapacity)
            {
                m_InitialTransportCapacity[typeof(T).TypeHandle] = capacity;
            }
        }
    }
}
