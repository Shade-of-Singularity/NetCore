using Cysharp.Threading.Tasks;
using NetCore.Common;
using NetCore.Transports;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

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
    ///  to either one connection (TCP+UDP to one)
    ///  or to multiple(UDP + SteamUDP to separate).
    /// Note: Maybe instead of declaring all <see cref="HashLists"/> we can simply use <see cref="HashList{TBase}"/> for <see cref="HashLists"/>?
    ///  It will require rewriting <see cref="HashList{TBase}"/> to return "ref TBase" items instead of simply items,
    ///  but besides not being sure where to declare new <see cref="HashLists"/> to then take a reference from - I don't see much issues.
    ///  It will make code twice as slow, but will reduce memory usage.
    ///  I'm alright with how things are right now though - allocating 6 arrays with 2 items + 24 bytes per list it's not much:
    ///  = 240 bytes.
    ///  It's nothing compared to other things.
    public abstract partial class NetworkMember(int transports) : INetworkMemberStatistics
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
        /// Current state of the <see cref="NetworkMember"/>.
        /// </summary>
        public StartState StartState
        {
            get
            {
                lock (_lock) return m_StartState;
            }
        }
        /// <summary>
        /// Current state of connection.
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                lock (_lock) return m_ConnectionState;
            }
        }
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
        /// <summary>
        /// Which async mode to use when <see cref="Start(StartupArgsProvider?, bool)"/> is called.
        /// <see cref="AsyncMode.AsyncSingleThreaded"/> and <see cref="AsyncMode.AsyncMultiThreaded"/>
        /// allow starting multiple transports at once.
        /// </summary>
        /// <remarks>
        /// Behavior can be overridden with <see cref="ITransport.SupportedStartAsyncModes"/>.
        /// </remarks>
        public AsyncMode UseAsyncStart
        {
            get => AsyncMode.Synced;
            set
            {
                lock (_lock) throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Which async mode to use when <see cref="Connect(ConnectionArgsHandler?, bool)"/> is called.
        /// <see cref="AsyncMode.AsyncSingleThreaded"/> and <see cref="AsyncMode.AsyncMultiThreaded"/>
        /// allow connecting multiple transports at once.
        /// </summary>
        /// <remarks>
        /// Behavior can be overridden with <see cref="ITransport.SupportedConnectionAsyncModes"/>.
        /// </remarks>
        public AsyncMode UseAsyncConnect
        {
            get => AsyncMode.Synced;
            set
            {
                lock (_lock) throw new NotImplementedException();
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Dictionary with all <see cref="IResilientTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected HashList<IResilientTransport> ResilientTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IReliableTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected HashList<IReliableTransport> ReliableTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="INotifyTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected HashList<INotifyTransport> NotifyTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IUnreliableTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected HashList<IUnreliableTransport> UnreliableTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected HashList<IStreamTransport> StreamTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected HashList<IFileTransport> FileTransports = new(transports);
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
        /// <inheritdoc cref="StartState"/>
        private StartState m_StartState;
        /// <inheritdoc cref="ConnectionState"/>
        private ConnectionState m_ConnectionState;
        /// <summary>
        /// Token for current operation.
        /// </summary>
        private CancellationTokenSource? m_LastTokenSource;
        /// <summary>
        /// Handle for current operation.
        /// </summary>
        private UniTask<bool> m_LastOperation;




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
        /// <param name="args">Args to provide to the <see cref="ITransport"/>s.</param>
        /// <param name="token">Token for cancelling this starting operation.</param>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        protected virtual async UniTask<bool> StartOperation(StartupArgs args, CancellationToken token)
        {
            ITransport[] transports = RentTransports(out int total);

            try
            {
                int index = 0;
                for (; index < total; index++)
                {
                    // Transports manage their own cancellation token.
                    if (token.IsCancellationRequested || !await transports[index].InvokeStart(args))
                        goto ResetState;
                }

                return true;

                ResetState: // Stops active transports to release resources.
                index--; // Failed transport skipped - it will automatically stop itself.
                for (; index >= 0; index--)
                {
                    transports[index].InvokeStop();
                }

                return false;
            }
            finally
            {
                ReturnTransports(transports);
            }
        }

        /// <summary>
        /// Starts this <see cref="NetworkMember"/> using args, modified by <see cref="StartupArgsProvider"/> - <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// Binds all registered <see cref="ITransport"/>s to an <see cref="StartupArgs.LocalIPEndPoint"/> and marks instance as active.
        /// </remarks>
        /// <param name="provider">Handler modifying provided <see cref="StartupArgs"/>.</param>
        /// <param name="clear">
        /// Whether to clear <see cref="StartupArgs"/> from a previous session
        /// before passing them to <paramref name="provider"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        public async UniTask<bool> Start(StartupArgsProvider? provider, bool clear = true)
        {
            UniTask<bool> last;
            CancellationTokenSource source;
            lock (_lock)
            {
                if (m_LastTokenSource is not null)
                {
                    m_LastTokenSource.Cancel();
                    m_LastTokenSource.Dispose();
                    last = m_LastOperation;
                }
                else
                {
                    last = UniTask.FromResult(false);
                }

                m_LastTokenSource = source = new();
            }

            if (Settings.UseConcurrentProtections)
            {
                await UniTask.Yield();
            }

            UniTask<bool> start;
            lock (_lock)
            {
                switch (m_StartState)
                {
                    case StartState.Stopped: break;
                    case StartState.Starting: throw new NetworkMemberStateException($"{GetType().Name} is already starting.");
                    case StartState.Started: throw new NetworkMemberStateException($"{GetType().Name} has already started.");
                    case StartState.Stopping: throw new NetworkMemberStateException($"{GetType().Name} cannot start - member is currently stopping.");
                    default: throw new SwitchExpressionException(m_StartState);
                }

                if (clear) m_StartupArgs.Clear();
                provider?.Invoke(m_StartupArgs);

                m_StartState = StartState.Starting;
                m_LastOperation = start = Create(this, 
                    static (member, token) => member.StartOperation(member.m_StartupArgs, token),
                    source.Token).Preserve();
                NetworkMembers.ListActiveMember(this);
            }

            try { await last; }
            catch { } // Exception will be logged by the original caller.

            bool result = false;
            try
            {
                result = await start && !source.IsCancellationRequested;
                return result;
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(ex); // TODO: Implement logger from ServiceCore.
                Console.ForegroundColor = color;
                return false;
            }
            finally
            {
                lock (_lock)
                {
                    m_StartState = result ? StartState.Started : StartState.Stopped;

                    if (ReferenceEquals(m_LastTokenSource, source))
                    {
                        m_LastTokenSource.Dispose();
                        m_LastTokenSource = null;
                    }
                }
            }
        }

        /// <summary>
        /// Restarts this <see cref="NetworkMember"/> using <see cref="StartupArgs"/> from a previous session.
        /// </summary>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: invalid endPoint, custom errors from transports, etc).
        /// </returns>
        /// TODO: Make restart more reliable under concurrency.
        public async UniTask<bool> Restart()
        {
            UniTask<bool> last;
            CancellationTokenSource source;
            lock (_lock)
            {
                if (m_LastTokenSource is not null)
                {
                    m_LastTokenSource.Cancel();
                    m_LastTokenSource.Dispose();
                    last = m_LastOperation;
                }
                else
                {
                    last = UniTask.FromResult(false);
                }

                m_LastTokenSource = source = new();
                m_LastOperation = default;
            }

            // TODO: Move protection to the outside of a lock, and unite two lock blocks.
            if (Settings.UseConcurrentProtections)
            {
                await UniTask.Yield();
            }

            bool doStop;
            lock (_lock)
            {
                doStop = m_StartState switch
                {
                    StartState.Starting or StartState.Started => true,
                    _ => false,
                };
            }

            try { await last; }
            catch { } // Exception will be logged by the original caller.

            try
            {
                if (doStop) await Stop();
                return await Start(null, clear: true);
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(ex); // TODO: Implement logger from ServiceCore.
                Console.ForegroundColor = color;
                return false;
            }

            /*
            UniTask<bool> start;
            UniTask<bool> stop = default;
            UniTask<bool> last = default;
            lock (_lock)
            {
                switch (m_StartState)
                {
                    case StartState.Stopped: break;
                    case StartState.Started: stop = Stop(); break;

                    case StartState.Starting: throw new NetworkMemberStateException($"Cannot restart {GetType().Name} while it is starting.");
                    case StartState.Stopping: throw new NetworkMemberStateException($"Cannot restart {GetType().Name} while it is stopping.");
                    default: throw new SwitchExpressionException(m_StartState);
                }

                // Registers last operation to wait until it is stopped on cancellation.
                if (m_OperationToken is not null)
                {
                    m_OperationToken.Cancel();
                    m_OperationToken.Dispose();
                    m_OperationToken = null;
                }
                last = m_OperationHandle;
                m_OperationHandle = default;

                start = Start(null, clear: false);
            }

            try
            {
                await last;
                return await start;
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(ex); // TODO: Implement logger from ServiceCore.
                Console.ForegroundColor = color;
                return false;
            }*/
        }

        /// <summary>
        /// If not already started:
        /// Starts this <see cref="NetworkMember"/> using args, modified by <see cref="StartupArgsProvider"/> - <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">Handler modifying provided <see cref="StartupArgs"/>.</param>
        /// <param name="task">
        /// <c>true</c> if started successfully.
        /// <c>false</c> if couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </param>
        /// <returns>
        /// <c>true</c> if <see cref="Start(StartupArgsProvider?, bool)"/> was run and <paramref name="task"/> was provided.
        /// <c>false</c> if <see cref="NetworkMember"/> was already started.
        /// </returns>
        /// <inheritdoc cref="Start(StartupArgsProvider?, bool)"/>
        public bool TryStart(out UniTask<bool> task, StartupArgsProvider handler)
        {
            lock (_lock)
            {
                switch (m_StartState)
                {
                    case StartState.Stopped: task = Start(handler, clear: false); return true;

                    case StartState.Starting:
                    case StartState.Started:
                    case StartState.Stopping: task = default; return false;
                    default: throw new SwitchExpressionException(m_StartState);
                }
            }
        }

        /// <summary>
        /// Actual stop operation. Stops all registered transports.
        /// </summary>
        /// <returns>
        /// Always <c>true</c>, since we don't bother about success with a stop method.
        /// </returns>
        protected virtual UniTask<bool> StopOperation(CancellationToken token)
        {
            // Success result is ignored, as we are simply resetting the state.
            foreach (var transport in ResilientTransports)
                transport.InvokeStop();

            foreach (var transport in ReliableTransports)
                transport.InvokeStop();

            foreach (var transport in NotifyTransports)
                transport.InvokeStop();

            foreach (var transport in UnreliableTransports)
                transport.InvokeStop();

            foreach (var transport in StreamTransports)
                transport.InvokeStop();

            foreach (var transport in FileTransports)
                transport.InvokeStop();

            return UniTask.FromResult(true);
        }

        /// <summary>
        /// Unbinds and stops all registered <see cref="ITransport"/>s and marks instance as inactive.
        /// </summary>
        /// <returns>
        /// <c>true</c> if started stopped.
        /// <c>false</c> if couldn't stop, likely due to exceptions from transports, or if stop operation was cancelled.
        /// </returns>
        public virtual async UniTask<bool> Stop()
        {
            UniTask<bool> last;
            CancellationTokenSource source;
            lock (_lock)
            {
                if (m_LastTokenSource is not null)
                {
                    m_LastTokenSource.Cancel();
                    m_LastTokenSource.Dispose();
                    last = m_LastOperation;
                }
                else
                {
                    last = UniTask.FromResult(false);
                }

                m_LastTokenSource = source = new();
                m_LastOperation = default;
            }

            // TODO: Move protection to the outside of a lock, and unite two lock blocks.
            if (Settings.UseConcurrentProtections)
            {
                await UniTask.Yield();
            }

            UniTask<bool> stop;
            lock (_lock)
            {
                switch (m_StartState)
                {
                    case StartState.Starting:
                    case StartState.Started: break;

                    case StartState.Stopped: throw new NetworkMemberStateException($"{GetType().Name} was already stopped.");
                    case StartState.Stopping: throw new NetworkMemberStateException($"{GetType().Name} is already stopping.");
                    default: throw new SwitchExpressionException(m_StartState);
                }

                // Begins stopping the member.
                m_StartState = StartState.Stopping;
                m_LastOperation = stop = Create(this,
                    static (member, token) => member.StopOperation(token),
                    source.Token).Preserve();
            }

            try { await last; }
            catch { } // Exception will be logged by the original caller.

            bool result = false;
            try
            {
                result = await stop && !source.IsCancellationRequested;
                return result;
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(ex); // TODO: Implement logger from ServiceCore.
                Console.ForegroundColor = color;
                return false;
            }
            finally
            {
                lock (_lock)
                {
                    if (result)
                    {
                        m_StartState = StartState.Stopped;
                    }

                    NetworkMembers.DelistActiveMember(this);
                    if (ReferenceEquals(m_LastTokenSource, source))
                    {
                        m_LastTokenSource.Cancel();
                        m_LastTokenSource.Dispose();
                        m_LastTokenSource = null;
                    }
                }
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
        protected virtual bool Connect(CancellationToken token)
        {
            lock (_lock)
            {
                var args = ConnectionArgs;
                var temp = m_TemporaryTransports;
                temp.Clear();

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
        public UniTask<bool> Connect(ConnectionArgsHandler handler)
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




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Counts all transports (including duplicates).
        /// </summary>
        protected virtual int CountTransports()
        {
            lock (_lock) return ResilientTransports.Count
                    + ReliableTransports.Count
                    + NotifyTransports.Count
                    + UnreliableTransports.Count
                    + StreamTransports.Count
                    + FileTransports.Count;
        }

        /// <summary>
        /// Lists all transports in a newly created or rented array.
        /// </summary>
        /// <returns>All current transports.</returns>
        protected virtual ITransport[] RentTransports(out int total)
        {
            lock (_lock)
            {
                // We cache transports inside the lock to avoid race conditions.
                total = CountTransports();
                ITransport[] transports = ArrayPool<ITransport>.Shared.Rent(NextSize(total));
                int position = 0;

                foreach (var t in ResilientTransports)
                    transports[position++] = t;

                foreach (var t in ReliableTransports)
                    transports[position++] = t;

                foreach (var t in NotifyTransports)
                    transports[position++] = t;

                foreach (var t in UnreliableTransports)
                    transports[position++] = t;

                foreach (var t in StreamTransports)
                    transports[position++] = t;

                foreach (var t in FileTransports)
                    transports[position++] = t;

                return transports;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int NextSize(int target)
            {
                const int Increment = 8;
                const int IncrementTotalBits = 3;
                return (target + Increment - 1) >> IncrementTotalBits;
            }
        }

        /// <summary>
        /// Releases an array, previously returned by <see cref="RentTransports"/> method.
        /// </summary>
        /// <param name="transports">Transports array to return.</param>
        protected virtual void ReturnTransports(ITransport[] transports)
        {
            if (transports is null)
                return;

            ArrayPool<ITransport>.Shared.Return(transports, clearArray: true);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static UniTask<TResult> Create<TState, TResult>(
            TState state, Func<TState, CancellationToken, UniTask<TResult>> factory, CancellationToken token)
        {
            return factory(state, token);
        }
    }
}
