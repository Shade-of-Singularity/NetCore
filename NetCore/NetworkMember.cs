using NetCore.Common;
using NetCore.Transports;
using System;
using System.Diagnostics.CodeAnalysis;

namespace NetCore
{
    /// <summary>
    /// Consumer used in ForEach methods for <see cref="ITransport"/>s.
    /// </summary>
    public delegate void TransportConsumer<in TTransport>(TTransport transport)
        where TTransport : ITransport;

    /// <summary>
    /// Consumer used in ForEach methods for <see cref="ITransport"/>s with a <see cref="NetworkMember"/> explicitly specified.
    /// </summary>
    public delegate void MemberTransportConsumer<in TMember, in TTransport>(TMember member, TTransport transport)
        where TMember : NetworkMember<TMember>
        where TTransport : ITransport;

    /// <inheritdoc cref="NetworkMember"/>
    public abstract class NetworkMember<T>(int transports) : NetworkMember(transports) where T : NetworkMember<T>
    {
        /// <summary>
        /// Iterates over all reliable transports using a given <paramref name="consumer"/> delegate.
        /// </summary>
        /// <param name="consumer">Action to use on all registered <see cref="IReliableTransport"/>s.</param>
        public void ForEachReliableTransport(MemberTransportConsumer<T, IReliableTransport> consumer)
        {
            lock (_lock)
            {
                T self = (T)this;
                foreach (var transport in ReliableTransports)
                {
                    consumer(self, transport);
                }
            }
        }

        /// <summary>
        /// Iterates over all unreliable transports using a given <paramref name="consumer"/> delegate.
        /// </summary>
        /// <param name="consumer">Action to use on all registered <see cref="IUnreliableTransport"/>s.</param>
        public void ForEachUnreliableTransport(MemberTransportConsumer<T, IUnreliableTransport> consumer)
        {
            lock (_lock)
            {
                T self = (T)this;
                foreach (var transport in UnreliableTransports)
                {
                    consumer(self, transport);
                }
            }
        }
    }


    /// <summary>
    /// Base class for <see cref="Server"/> and <see cref="Client"/>.
    /// </summary>
    /// <param name="transports">Initial capacity for transports to pre-initialize.</param>
    /// <remarks>
    /// If you need something other than <see cref="IReliableTransport"/> or <see cref="IUnreliableTransport"/>
    /// - fork the project and modify this base class, or define the same logic in a custom <see cref="Client"/> and <see cref="Server"/> class.
    /// </remarks>
    /// TODO: Add a way to map connections, arrived from different transports
    /// to either one connection (TCP+UDP to one)
    /// or to multiple(UDP + SteamUDP to separate).
    public abstract class NetworkMember(int transports)
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default of 2 for transports commonly used by us:
        /// <see cref="Transports.Unix.UnixTransport"/> and <see cref="Transports.Special.TCPUDPTransport"/>s (or others).
        /// </summary>
        public const int DefaultInitialTransportCapacity = 2;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Whether this <see cref="NetworkMember"/> was started using <see cref="Start()"/> method.
        /// </summary>
        public bool IsStarted { get; protected set; }
        /// <summary>
        /// Whether this <see cref="NetworkMember"/> is attempting to connect or is connected to a remote host.
        /// </summary>
        public bool IsActive { get; protected set; }
        /// <summary>
        /// Arguments that were used to start this <see cref="NetworkMember"/>, in a read-only form.
        /// </summary>
        public IReadOnlyStartupArgs StartupArgs => m_StartupArgs;
        /// <summary>
        /// Arguments that were used to connect this <see cref="NetworkMember"/> to remote host, in a read-only form.
        /// </summary>
        public IReadOnlyConnectionArgs ConnectionArgs => m_ConnectionArgs;
        /// <summary>
        /// Provider for all <see cref="ConnectionID"/>s managed by this <see cref="NetworkMember"/> and its transports.
        /// </summary>
        public ConnectionIDProvider ConnectionIDProvider => m_ConnectionIDProvider;




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
        protected HashList<IReliableTransport> ReliableTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="ITransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected HashList<IUnreliableTransport> UnreliableTransports = new(transports);
        /// <summary>
        /// Arguments used to start this <see cref="NetworkMember"/>.
        /// </summary>
        protected readonly StartupArgs m_StartupArgs = [];
        /// <summary>
        /// Arguments used to connect this <see cref="NetworkMember"/> to remote host.
        /// </summary>
        protected readonly ConnectionArgs m_ConnectionArgs = [];
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
        /// Provider for Connection IDs.
        /// </summary>
        private readonly ConnectionIDProvider m_ConnectionIDProvider = new();
        /// <summary>
        /// Maximum amount of headers reported by any registered <see cref="ITransport"/>.
        /// </summary>
        /// <remarks>
        /// If client and server have different headers - they will be combined during the initial communication step.
        /// Transports will report combined total of headers back to the <see cref="NetworkMember"/>
        /// so we can better optimize <see cref="Header"/> and <see cref="Header"/>.
        /// </remarks>
        /// Note: Removed, because we should not depend on asynchronously arriving data in the internal code.
        ///  Async data or transport-only data should only influence how one transport performs.
        //private int m_MaxReportedHeaderAmount = 1024;



        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Constructs <see cref="NetworkMember"/> with a default initial transport capacity - <see cref="DefaultInitialTransportCapacity"/>.
        /// </summary>
        public NetworkMember() : this(DefaultInitialTransportCapacity) { }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Starts this <see cref="NetworkMember"/> using current <see cref="StartupArgs"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        protected virtual bool Start()
        {
            lock (_lock)
            {
                var args = StartupArgs;
                foreach (var transport in ReliableTransports)
                {
                    if (!transport.InvokeStart(args))
                        goto ResetState;
                }

                foreach (var transport in UnreliableTransports)
                {
                    if (!transport.InvokeStart(args))
                        goto ResetState;
                }

                NetworkMembers.IncrementActiveMembers();
                return true;

                ResetState:
                StopInternal();
                return false;
            }
        }

        /// <summary>
        /// Starts this <see cref="NetworkMember"/> using args, modified by <see cref="StartupArgsHandler"/> - <paramref name="handler"/>.
        /// </summary>
        /// <remarks>
        /// Binds all registered <see cref="ITransport"/>s to an <see cref="StartupArgs.LocalIPEndPoint"/> and marks instance as active.
        /// </remarks>
        /// <param name="handler">Handler modifying provided <see cref="StartupArgs"/>.</param>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        public bool Start(StartupArgsHandler handler)
        {
            lock (_lock)
            {
                Stop();
                m_StartupArgs.Clear();
                handler(m_StartupArgs);
                return Start();
            }
        }

        /// <summary>
        /// Restarts this <see cref="NetworkMember"/> using <see cref="StartupArgs"/> from a previous session.
        /// </summary>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: invalid endPoint, custom errors from transports, etc).
        /// </returns>
        public bool Restart()
        {
            lock (_lock)
            {
                Stop();
                return Start();
            }
        }

        /// <summary>
        /// Unbinds and stops all registered <see cref="ITransport"/>s and marks instance as inactive.
        /// </summary>
        public virtual void Stop()
        {
            lock (_lock)
            {
                if (IsStarted)
                {
                    StopInternal();
                    NetworkMembers.DecrementActiveMembers();
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
        /// Connects this <see cref="NetworkMember"/> to a remote host, using current <see cref="ConnectionArgs"/>.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - connects server to a relay and manages a NAT hole.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if connection began.
        /// <c>false</c> if connection couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        protected virtual bool Connect()
        {
            lock (_lock)
            {
                var args = ConnectionArgs;
                foreach (var transport in ReliableTransports)
                {
                    if (!transport.InvokeConnect(args))
                        goto ResetState;
                }

                foreach (var transport in UnreliableTransports)
                {
                    if (!transport.InvokeConnect(args))
                        goto ResetState;
                }

                return true;

                ResetState:
                DisconnectInternal();
                return false;
            }
        }

        /// <summary>
        /// Connects this <see cref="NetworkMember"/> to a remote host
        /// using args, modified by <see cref="ConnectionArgsHandler"/> - <paramref name="handler"/>.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - connects server to a relay and manages a NAT hole.
        /// </remarks>
        /// <param name="handler">Handler modifying provided <see cref="ConnectionArgs"/>.</param>
        /// <returns>
        /// <c>true</c> if connection began.
        /// <c>false</c> if connection couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        public bool Connect(ConnectionArgsHandler handler)
        {
            lock (_lock)
            {
                Disconnect();
                m_ConnectionArgs.Clear();
                handler(m_ConnectionArgs);
                return Connect();
            }
        }

        /// <summary>
        /// Reconnects this <see cref="NetworkMember"/> using <see cref="ConnectionArgs"/> from a previous connection attempt/session.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - connects server to a relay and manages a NAT hole.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if connection began.
        /// <c>false</c> if connection couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        public bool Reconnect()
        {
            lock (_lock)
            {
                Disconnect();
                return Connect();
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
                if (IsActive)
                {
                    DisconnectInternal();
                    IsActive = false;
                }
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
            if (typeof(T) == typeof(IReliableTransport))
            {
                throw new ArgumentException($"You should only register transports with explicitly defined end-type - do not rely on the base.");
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
            if (typeof(T) == typeof(IReliableTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
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
            if (typeof(T) == typeof(IReliableTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
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
            if (typeof(T) == typeof(IReliableTransport))
            {
                throw new ArgumentException($"You should only check transports with explicitly defined end-type - do not rely on the base.");
            }

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
            if (typeof(T) == typeof(IReliableTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

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
            if (typeof(T) == typeof(IReliableTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return ReliableTransports.Get<T>();
            }
        }

        /// <summary>
        /// Iterates over all reliable transports using a given <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action to use on all registered <see cref="IReliableTransport"/>s.</param>
        public void ForEachReliableTransport(TransportConsumer<IReliableTransport> action)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                {
                    action(transport);
                }
            }
        }

        /// <summary>
        /// Removes all <see cref="IReliableTransport"/>s
        /// and calls <see cref="ITransport.Terminate(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Terminate(NetworkMember)"/>.
        /// </returns>
        public bool ClearReliableTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in ReliableTransports)
                {
                    anyFailed = !transport.InvokeTerminate(this);
                }

                return !anyFailed;
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
            if (typeof(T) == typeof(IUnreliableTransport))
            {
                throw new ArgumentException($"You should only register transports with explicitly defined end-type - do not rely on the base.");
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
            if (typeof(T) == typeof(IUnreliableTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
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
            if (typeof(T) == typeof(IUnreliableTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
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
            if (typeof(T) == typeof(IUnreliableTransport))
            {
                throw new ArgumentException($"You should only check transports with explicitly defined end-type - do not rely on the base.");
            }

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
            if (typeof(T) == typeof(IUnreliableTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

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
            if (typeof(T) == typeof(IUnreliableTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return UnreliableTransports.Get<T>();
            }
        }

        /// <summary>
        /// Iterates over all reliable transports using a given <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action to use on all registered <see cref="IUnreliableTransport"/>s.</param>
        public void ForEachUnreliableTransport(TransportConsumer<IUnreliableTransport> action)
        {
            lock (_lock)
            {
                foreach (var transport in UnreliableTransports)
                {
                    action(transport);
                }
            }
        }

        /// <summary>
        /// Removes all <see cref="IUnreliableTransport"/>s
        /// and calls <see cref="ITransport.Terminate(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Terminate(NetworkMember)"/>.
        /// </returns>
        public bool ClearUnreliableTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in ReliableTransports)
                {
                    anyFailed = !transport.InvokeTerminate(this);
                }

                return !anyFailed;
            }
        }
        #endregion
    }
}
