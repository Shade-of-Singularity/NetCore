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
    /// Note: Maybe instead of declaring all <see cref="QuickLists"/> we can simply use <see cref="QuickList{TBase}"/> for <see cref="QuickLists"/>?
    ///  It will require rewriting <see cref="QuickList{TBase}"/> to return "ref TBase" items instead of simply items,
    ///  but besides not being sure where to declare new <see cref="QuickLists"/> to then take a reference from - I don't see much issues.
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
        readonly Client client;



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
        /// Which async mode to use when <see cref="Connect(ConnectionArgsProvider?, bool)"/> is called.
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
        protected QuickList<IResilientTransport> ResilientTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IReliableTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected QuickList<IReliableTransport> ReliableTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="ISequentialTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected QuickList<ISequentialTransport> SequentialTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IUnreliableTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected QuickList<IUnreliableTransport> UnreliableTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected QuickList<IStreamTransport> StreamTransports = new(transports);
        /// <summary>
        /// Dictionary with all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> can use.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected QuickList<IFileTransport> FileTransports = new(transports);
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

















        /*


        enum Operation : byte
        {
            None, Start, Stop, Restart,
        }




        private CancellationTokenSource? lastTokenSource;
        private Operation lastOperation;
        private UniTask lastTask;

        protected UniTask StartOperation() => UniTask.CompletedTask;
        public UniTask Start()
        {
            lock (_lock)
            {
                switch (lastOperation)
                {
                    case Operation.None: break;
                    case Operation.Start: return lastTask; // Simply awaits an existing task.
                    case Operation.Stop:
                        
                        break;

                    case Operation.Restart: throw new Exception("Already processing an operation.");
                    default: throw new SwitchExpressionException(lastOperation);
                }

                
            }
        }

        protected UniTask StopOperation() => UniTask.CompletedTask;
        public bool TryStart(out UniTask task)
        {
            lock (_lock)
            {
                if (lastOperation == Operation.None)
                {
                    task = Start();
                    return true;
                }

                task = default;
                return false;
            }
        }

        public UniTask Stop()
        {

        }

        public bool TryStop(out UniTask task)
        {

        }

        public UniTask Restart()
        {

        }









        */























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
        public UniTask<bool> Start(StartupArgsProvider? provider, bool clear = true) =>
            Settings.UseConcurrentProtections
            ? StartProtected(provider, clear)
            : StartDirect(provider, clear);

        /// <summary>
        /// If network member is not started - starts it using <see cref="Start(StartupArgsProvider?, bool)"/> method.
        /// </summary>
        /// <param name="task">Task returned by <see cref="Start(StartupArgsProvider?, bool)"/> or <c>default</c> when returns <c>false</c>.</param>
        /// <param name="provider">Handler modifying provided <see cref="StartupArgs"/>.</param>
        /// <param name="clear">
        /// Whether to clear <see cref="StartupArgs"/> from a previous session
        /// before passing them to <paramref name="provider"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if <see cref="NetworkMember"/> was stopped and will now be started.
        /// <c>false</c> if <see cref="NetworkMember"/> will not be started as it is already started.
        /// </returns>
        public bool TryStart(out UniTask<bool> task, StartupArgsProvider? provider, bool clear = true)
        {
            lock (_lock)
            {
                switch (m_StartState)
                {
                    case StartState.Stopped: task = Start(provider, clear); return true;
                    case StartState.Starting:
                    case StartState.Started:
                    case StartState.Stopping: task = UniTask.FromResult(false); return false;
                    default: throw new SwitchExpressionException(m_StartState);
                }
            }
        }

        /// <summary>
        /// Stops currently running operation (i.e. <see cref="m_LastOperation"/> and <see cref="m_LastTokenSource"/>)
        /// <paramref name="source"/> will be provided a new value.
        /// <paramref name="last"/> will be set to <see cref="m_LastOperation"/> if it active.
        /// </summary>
        /// <remarks>
        /// Requires caller to enclose this method in a <![CDATA[lock (_lock) { ... }]]> block.
        /// </remarks>
        private void OverrideOperationUnlocked(out CancellationTokenSource source, out UniTask<bool> last)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateStateForStart(StartState state)
        {
            switch (state)
            {
                case StartState.Stopped: break;
                case StartState.Starting: throw new NetworkMemberStateException($"{GetType().Name} is already starting.");
                case StartState.Started: throw new NetworkMemberStateException($"{GetType().Name} has already started.");
                case StartState.Stopping: throw new NetworkMemberStateException($"{GetType().Name} cannot start - member is currently stopping.");
                default: throw new SwitchExpressionException(state);
            }
        }

        private async UniTask<bool> StartProtected(StartupArgsProvider? provider, bool clear)
        {
            UniTask<bool> last, start;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
            }

            await UniTask.Yield();
            lock (_lock)
            {
                // Operation was cancelled by another thread.
                // Part of the high-concurrency protection layer - do not remove.
                // Must run after a Yield or a small delay function.
                if (source.IsCancellationRequested)
                {
                    return false;
                }

                ValidateStateForStart(m_StartState);
                if (clear) m_StartupArgs.Clear();
                provider?.Invoke(m_StartupArgs);

                m_StartState = StartState.Starting;
                m_LastOperation = start = Create(this,
                    static async (member, token) => await member.StartOperation(member.m_StartupArgs, token),
                    source.Token).Preserve();
                NetworkMembers.ListActiveMember(this);
            }

            return await InvokeStartInternal(last, start, source);
        }

        private UniTask<bool> StartDirect(StartupArgsProvider? provider, bool clear)
        {
            UniTask<bool> last, start;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
                ValidateStateForStart(m_StartState);
                if (clear) m_StartupArgs.Clear();
                provider?.Invoke(m_StartupArgs);

                m_StartState = StartState.Starting;
                m_LastOperation = start = Create(this,
                    static async (member, token) => await member.StartOperation(member.m_StartupArgs, token),
                    source.Token).Preserve();
                NetworkMembers.ListActiveMember(this);
            }

            return InvokeStartInternal(last, start, source);
        }

        private async UniTask<bool> InvokeStartInternal(UniTask<bool> last, UniTask<bool> start, CancellationTokenSource source)
        {
            try { await last; }
            catch { } // Exception will be logged by the original caller.

            bool result = false;
            try
            {
                result = await start && !source.IsCancellationRequested;
                return result;
            }
            catch (Exception exception)
            {
                LogException(exception);
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
        /// TODO: Make restart awaitable with one united awaitable return value.
        public UniTask<bool> Restart() =>
            Settings.UseConcurrentProtections
            ? RestartProtected()
            : RestartDirect();

        private async UniTask<bool> RestartProtected()
        {
            UniTask<bool> last;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
            }

            await UniTask.Yield();
            bool doStop;
            lock (_lock)
            {
                // Operation was cancelled by another thread.
                // Part of the high-concurrency protection layer - do not remove.
                // Must run after a Yield or a small delay function.
                if (source.IsCancellationRequested)
                {
                    return false;
                }

                doStop = m_StartState switch
                {
                    StartState.Starting or StartState.Started => true,
                    _ => false,
                };
            }

            return await InvokeRestartInternal(last, doStop, source);
        }

        private UniTask<bool> RestartDirect()
        {
            bool doStop;
            UniTask<bool> last;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
                doStop = m_StartState switch
                {
                    StartState.Starting or StartState.Started => true,
                    _ => false,
                };
            }

            return InvokeRestartInternal(last, doStop, source);
        }

        private async UniTask<bool> InvokeRestartInternal(UniTask<bool> last, bool doStop, CancellationTokenSource source)
        {
            try { await last; }
            catch { } // Exception will be logged by the original caller.

            last = UniTask.FromResult(false);
            if (doStop)
            {
                await InvokeStopInternal(last, stop: Create(this,
                    static async (member, token) => await member.StopOperation(token),
                    source.Token), source);
            }

            if (source.IsCancellationRequested)
            {
                return false;
            }

            return await InvokeStartInternal(last, Create(this,
                static async (member, token) => await member.StartOperation(member.m_StartupArgs, token),
                source.Token), source);
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
            // Note: Consider renting all transports instead.
            lock (_lock)
            {
                // Success result is ignored, as we are simply resetting the state.
                foreach (var transport in ResilientTransports)
                    transport.InvokeStop();

                foreach (var transport in ReliableTransports)
                    transport.InvokeStop();

                foreach (var transport in SequentialTransports)
                    transport.InvokeStop();

                foreach (var transport in UnreliableTransports)
                    transport.InvokeStop();

                foreach (var transport in StreamTransports)
                    transport.InvokeStop();

                foreach (var transport in FileTransports)
                    transport.InvokeStop();
            }

            return UniTask.FromResult(true);
        }

        /// <summary>
        /// Unbinds and stops all registered <see cref="ITransport"/>s and marks instance as inactive.
        /// </summary>
        /// <returns>
        /// <c>true</c> if started stopped.
        /// <c>false</c> if couldn't stop, likely due to exceptions from transports, or if stop operation was cancelled.
        /// </returns>
        /// TODO: disconnect currently connected transports.
        public UniTask<bool> Stop() =>
            Settings.UseConcurrentProtections
            ? StopProtected()
            : StopDirect();

        /// <summary>
        /// If stopping is possible - runs <see cref="Stop"/> method.
        /// </summary>
        /// <param name="task">Task returned by <see cref="Stop"/>, or a completed task when returns <c>false</c>.</param>
        /// <returns>
        /// <c>true</c> if stopping is possible, will be made, and <paramref name="task"/> was provided.
        /// <c>false</c> if stopping is not allowed and will not be made (E.g. already stopped or stopping.)
        /// </returns>
        public bool TryStop(out UniTask<bool> task)
        {
            lock (_lock)
            {
                switch (m_StartState)
                {
                    case StartState.Starting:
                    case StartState.Started: task = Stop(); return true;
                    case StartState.Stopped:
                    case StartState.Stopping:
                    default: task = UniTask.FromResult(false); return false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateStateForStop(StartState state)
        {
            switch (state)
            {
                case StartState.Starting:
                case StartState.Started: break;
                case StartState.Stopped: throw new NetworkMemberStateException($"{GetType().Name} was already stopped.");
                case StartState.Stopping: throw new NetworkMemberStateException($"{GetType().Name} is already stopping.");
                default: throw new SwitchExpressionException(state);
            }
        }

        private async UniTask<bool> StopProtected()
        {
            UniTask<bool> last, stop;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
            }

            await UniTask.Yield();
            lock (_lock)
            {
                // Operation was cancelled by another thread.
                // Part of the high-concurrency protection layer - do not remove.
                // Must run after a Yield or a small delay function.
                if (source.IsCancellationRequested)
                {
                    return false;
                }

                ValidateStateForStop(m_StartState);

                // Begins stopping the member.
                m_StartState = StartState.Stopping;
                m_LastOperation = stop = Create(this,
                    static async (member, token) => await member.StopOperation(token),
                    source.Token).Preserve();
            }

            return await InvokeStopInternal(last, stop, source);
        }

        private UniTask<bool> StopDirect()
        {
            UniTask<bool> last, stop;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
                ValidateStateForStop(m_StartState);

                // Begins stopping the member.
                m_StartState = StartState.Stopping;
                m_LastOperation = stop = Create(this,
                    static async (member, token) => await member.StopOperation(token),
                    source.Token).Preserve();
            }

            return InvokeStopInternal(last, stop, source);
        }

        private async UniTask<bool> InvokeStopInternal(UniTask<bool> last, UniTask<bool> stop, CancellationTokenSource source)
        {
            try { await last; }
            catch { } // Exception will be logged by the original caller.

            bool result = false;
            try
            {
                result = await stop && !source.IsCancellationRequested;
                return result;
            }
            catch (Exception exception)
            {
                LogException(exception);
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




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Connection
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private void ValidateConnection()
        {
            lock (_lock)
            {
                if (m_StartState != StartState.Started)
                    throw new Exception($"Cannot use connection/disconnection methods before {GetType().Name} was started.");
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
        protected virtual async UniTask<bool> ConnectOperation(ConnectionArgs args, CancellationToken token)
        {
            ITransport[] transports = RentTransports(out int total);

            try
            {
                int index = 0;
                for (; index < total; index++)
                {
                    // Transports manage their own cancellation token.
                    if (token.IsCancellationRequested || !await transports[index].InvokeConnect(args))
                        goto ResetState;
                }

                return true;

                ResetState: // Stops active transports to release resources.
                index--; // Failed transport skipped - it will automatically stop itself.
                for (; index >= 0; index--)
                {
                    transports[index].InvokeDisconnect();
                }

                return false;
            }
            finally
            {
                ReturnTransports(transports);
            }
        }

        /// <summary>
        /// Connects this <see cref="NetworkMember"/> to a remote host
        /// using args, modified by <see cref="ConnectionArgsProvider"/> - <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - connects server to a relay and manages a NAT hole.
        /// </remarks>
        /// <param name="provider">Handler modifying provided <see cref="ConnectionArgs"/>.</param>
        /// <param name="clear">Whether to clear <see cref="ConnectionArgs"/> provided to the <paramref name="provider"/></param>
        /// <returns>
        /// <c>true</c> if connection began.
        /// <c>false</c> if connection couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        /// TODO: Add "TryConnect" method.
        /// TODO: Add a way to automatically disconnect and connect to another host with one awaitable call.
        public UniTask<bool> Connect(ConnectionArgsProvider? provider, bool clear = true)
        {
            ValidateConnection();
            return Settings.UseConcurrentProtections ? ConnectProtected(provider, clear) : ConnectDirect(provider, clear);
        }

        /// <summary>
        /// If not connected already:
        /// Connects this <see cref="NetworkMember"/> to a remote host
        /// using args, modified by <see cref="ConnectionArgsProvider"/> - <paramref name="provider"/>.
        /// </summary>
        /// <param name="task">
        /// Task, returned by <see cref="Connect(ConnectionArgsProvider?, bool)"/>,
        /// or <c>null</c> when <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// <c>true</c> if member was not connected and will try to connect. <paramref name="task"/> will be provided as well.
        /// <c>false</c> if member is already connected and will to attempt a connection.
        /// </returns>
        /// <inheritdoc cref="Connect(ConnectionArgsProvider?, bool)"/>
        /// <param name="provider"></param>
        /// <param name="clear"></param>
        public bool TryConnect(out UniTask<bool> task, ConnectionArgsProvider? provider, bool clear = true)
        {
            lock (_lock)
            {
                switch (m_ConnectionState)
                {
                    case ConnectionState.Idle: task = Connect(provider, clear); return true;
                    case ConnectionState.Connecting:
                    case ConnectionState.Connected:
                    case ConnectionState.Disconnecting: task = UniTask.FromResult(false); return false;
                    default: throw new SwitchExpressionException(m_ConnectionState);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateStateForConnect(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Idle: break;
                case ConnectionState.Connecting: throw new NetworkMemberStateException($"{GetType().Name} is already connecting.");
                case ConnectionState.Connected: throw new NetworkMemberStateException($"{GetType().Name} has already connected.");
                case ConnectionState.Disconnecting: throw new NetworkMemberStateException($"{GetType().Name} cannot start - member is currently disconnecting.");
                default: throw new SwitchExpressionException(state);
            }
        }

        private async UniTask<bool> ConnectProtected(ConnectionArgsProvider? handler, bool clear)
        {
            UniTask<bool> last, connect;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
            }

            await UniTask.Yield();
            lock (_lock)
            {
                // Operation was cancelled by another thread.
                // Part of the high-concurrency protection layer - do not remove.
                // Must run after a Yield or a small delay function.
                if (source.IsCancellationRequested)
                {
                    return false;
                }

                ValidateStateForConnect(m_ConnectionState);

                if (clear) m_ConnectionArgs.Clear();
                handler?.Invoke(m_ConnectionArgs);

                m_ConnectionState = ConnectionState.Connecting;
                m_LastOperation = connect = Create(this,
                    static async (member, token) => await member.ConnectOperation(member.m_ConnectionArgs, token),
                    source.Token).Preserve();
            }

            return await InvokeConnectInternal(last, connect, source);
        }

        private UniTask<bool> ConnectDirect(ConnectionArgsProvider? handler, bool clear)
        {
            UniTask<bool> last, connect;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
                ValidateStateForConnect(m_ConnectionState);

                if (clear) m_ConnectionArgs.Clear();
                handler?.Invoke(m_ConnectionArgs);

                m_ConnectionState = ConnectionState.Connecting;
                m_LastOperation = connect = Create(this,
                    static async (member, token) => await member.ConnectOperation(member.m_ConnectionArgs, token),
                    source.Token).Preserve();
            }

            return InvokeConnectInternal(last, connect, source);
        }

        private async UniTask<bool> InvokeConnectInternal(UniTask<bool> last, UniTask<bool> connect, CancellationTokenSource source)
        {
            try { await last; }
            catch { } // Exception will be logged by the original caller.

            bool result = false;
            try
            {
                result = await connect && !source.IsCancellationRequested;
                return result;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
            finally
            {
                lock (_lock)
                {
                    m_ConnectionState = result ? ConnectionState.Connected : ConnectionState.Idle;

                    if (ReferenceEquals(m_LastTokenSource, source))
                    {
                        m_LastTokenSource.Dispose();
                        m_LastTokenSource = null;
                    }
                }
            }
        }

        /// <summary>
        /// Reconnects this <see cref="NetworkMember"/> using <see cref="ConnectionArgs"/> from a previous connection attempt/session.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - connects server to a relay and manages a NAT hole.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if reconnection began.
        /// <c>false</c> if reconnection couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        public UniTask<bool> Reconnect()
        {
            ValidateConnection();
            return Settings.UseConcurrentProtections ? ReconnectProtected(null, false) : ReconnectDirect(null, false);
        }

        /// <summary>
        /// Does not work like a regular <see cref="Reconnect()"/>.
        /// Instead - stops current connection and starts a new one in one method call.
        /// Essentially a substitute of two sequential <see cref="Stop"/> and <see cref="Start"/> calls.
        /// </summary>
        /// <param name="provider">Handler modifying provided <see cref="ConnectionArgs"/>.</param>
        /// <param name="clear">Whether to clear <see cref="ConnectionArgs"/> provided to the <paramref name="provider"/></param>
        /// <returns>
        /// <c>true</c> if reconnection began.
        /// <c>false</c> if reconnection couldn't start (reasons: invalid end-points, custom errors from transports, etc).
        /// </returns>
        public UniTask<bool> Reconnect(ConnectionArgsProvider? provider, bool clear = true)
        {
            ValidateConnection();
            return Settings.UseConcurrentProtections ? ReconnectProtected(provider, clear) : ReconnectDirect(provider, clear);
        }

        private async UniTask<bool> ReconnectProtected(ConnectionArgsProvider? provider, bool clear)
        {
            UniTask<bool> last, connect;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
            }

            await UniTask.Yield();

            //
            // TODO: Disconnect here.
            //

            lock (_lock)
            {
                // Operation was cancelled by another thread.
                // Part of the high-concurrency protection layer - do not remove.
                // Must run after a Yield or a small delay function.
                if (source.IsCancellationRequested)
                {
                    return false;
                }

                ValidateStateForConnect(m_ConnectionState);

                if (clear) m_ConnectionArgs.Clear();
                provider?.Invoke(m_ConnectionArgs);

                m_ConnectionState = ConnectionState.Connecting;
                m_LastOperation = connect = Create(this,
                    static async (member, token) => await member.ConnectOperation(member.m_ConnectionArgs, token),
                    source.Token).Preserve();
            }

            return await InvokeReconnectInternal(last, connect, source);
        }

        private UniTask<bool> ReconnectDirect(ConnectionArgsProvider? provider, bool clear)
        {
            UniTask<bool> last, connect;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);

                //
                // TODO: Disconnect here.
                //

                ValidateStateForConnect(m_ConnectionState);

                if (clear) m_ConnectionArgs.Clear();
                provider?.Invoke(m_ConnectionArgs);

                m_ConnectionState = ConnectionState.Connecting;
                m_LastOperation = connect = Create(this,
                    static async (member, token) => await member.ConnectOperation(member.m_ConnectionArgs, token),
                    source.Token).Preserve();
            }

            return InvokeConnectInternal(last, connect, source);
        }

        private async UniTask<bool> InvokeReconnectInternal(UniTask<bool> last, UniTask<bool> reconnect, CancellationTokenSource source)
        {
            try { await last; }
            catch { } // Exception will be logged by the original caller.

            bool result = false;
            try
            {
                result = await reconnect && !source.IsCancellationRequested;
                return result;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
            finally
            {
                lock (_lock)
                {
                    m_ConnectionState = result ? ConnectionState.Connected : ConnectionState.Idle;

                    if (ReferenceEquals(m_LastTokenSource, source))
                    {
                        m_LastTokenSource.Dispose();
                        m_LastTokenSource = null;
                    }
                }
            }
        }

        /// <summary>
        /// Disconnects this <see cref="NetworkMember"/> from a remote host.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - disconnects from a remote relay.
        /// </remarks>
        protected virtual UniTask<bool> DisconnectOperation()
        {
            // Note: Consider renting all transports instead.
            lock (_lock)
            {
                // Success result is ignored, as we are simply resetting the state.
                foreach (var transport in ResilientTransports)
                    transport.InvokeStop();

                foreach (var transport in ReliableTransports)
                    transport.InvokeStop();

                foreach (var transport in SequentialTransports)
                    transport.InvokeStop();

                foreach (var transport in UnreliableTransports)
                    transport.InvokeStop();

                foreach (var transport in StreamTransports)
                    transport.InvokeStop();

                foreach (var transport in FileTransports)
                    transport.InvokeStop();
            }

            return UniTask.FromResult(true);
        }

        /// <summary>
        /// Disconnects this <see cref="NetworkMember"/> from a remote host.
        /// </summary>
        /// <remarks>
        /// When called on a <see cref="Server"/> - disconnects from a remote relay.
        /// </remarks>
        public UniTask<bool> Disconnect()
        {
            ValidateConnection();
            return Settings.UseConcurrentProtections ? DisconnectProtected() : DisconnectDirect();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateStateForDisconnect(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Connecting:
                case ConnectionState.Connected: break;
                case ConnectionState.Idle: throw new NetworkMemberStateException($"Cannot disconnect an idle {GetType().Name}.");
                case ConnectionState.Disconnecting: throw new NetworkMemberStateException($"{GetType().Name} is already disconnecting.");
                default: throw new SwitchExpressionException(state);
            }
        }

        private async UniTask<bool> DisconnectProtected()
        {
            UniTask<bool> last, disconnect;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
            }

            await UniTask.Yield();
            lock (_lock)
            {
                // Operation was cancelled by another thread.
                // Part of the high-concurrency protection layer - do not remove.
                // Must run after a Yield or a small delay function.
                if (source.IsCancellationRequested)
                {
                    return false;
                }

                ValidateStateForDisconnect(m_ConnectionState);

                m_ConnectionState = ConnectionState.Connecting;
                m_LastOperation = disconnect = Create(this,
                    static async (member, token) => await member.DisconnectOperation(),
                    source.Token).Preserve();
            }

            return await InvokeDisconnectInternal(last, disconnect, source);
        }

        private UniTask<bool> DisconnectDirect()
        {
            UniTask<bool> last, disconnect;
            CancellationTokenSource source;
            lock (_lock)
            {
                OverrideOperationUnlocked(out source, out last);
                ValidateStateForDisconnect(m_ConnectionState);

                m_ConnectionState = ConnectionState.Connecting;
                m_LastOperation = disconnect = Create(this,
                    static async (member, token) => await member.DisconnectOperation(),
                    source.Token).Preserve();
            }

            return InvokeDisconnectInternal(last, disconnect, source);
        }

        private async UniTask<bool> InvokeDisconnectInternal(UniTask<bool> last, UniTask<bool> disconnect, CancellationTokenSource source)
        {
            try { await last; }
            catch { } // Exception will be logged by the original caller.

            bool result = false;
            try
            {
                result = await disconnect && !source.IsCancellationRequested;
                return result;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
            finally
            {
                lock (_lock)
                {
                    if (result)
                    {
                        m_ConnectionState = ConnectionState.Idle;
                    }

                    if (ReferenceEquals(m_LastTokenSource, source))
                    {
                        m_LastTokenSource.Dispose();
                        m_LastTokenSource = null;
                    }
                }
            }
        }

        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                            Transport Management
        /// .                  TODO: Only terminate transport if it was removed from ALL other storages.
        /// .       Note: this might be easier to resolve if we provide custom storage solution, allowing for filtering.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region General transport management
        /// <summary>
        /// Registers transport under all <see cref="SendingMode"/>s.
        /// </summary>
        public void RegisterGeneralTransport<TTransport>(TTransport transport) where TTransport : class,
            IResilientTransport, ISequentialTransport, IUnreliableTransport, IReliableTransport
        {
            if (typeof(TTransport) == typeof(IUnreliableTransport))
            {
                throw new ArgumentException($"You should only register transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                // TODO: Terminate only if transport is removed from all types.
                RegisterResilientTransport(transport);
                RegisterSequentialTransport(transport);
                RegisterUnreliableTransport(transport);
                RegisterReliableTransport(transport);
            }
        }

        /// <summary>
        /// Tries to remove <typeparamref name="TTransport"/> association with all <see cref="SendingMode"/>s.
        /// </summary>
        /// <typeparam name="TTransport">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport which was removed just a moment ago.</param>
        /// <returns>
        /// <c>true</c> if transport was present, was removed and the instance is provided as <paramref name="transport"/>.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveGeneralTransport<TTransport>([NotNullWhen(true)] out TTransport? transport) where TTransport : class,
            IResilientTransport, ISequentialTransport, IUnreliableTransport, IReliableTransport
        {
            throw new NotImplementedException();
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
        public bool RemoveGeneralTransport<T>(T transport) where T : class, IUnreliableTransport
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether this <see cref="NetworkMember"/> manages a specific reliable transport.
        /// </summary>
        public bool HasGeneralTransport<T>() where T : class, IUnreliableTransport
        {
            throw new NotImplementedException();
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
        public bool TryGetGeneralTransport<T>([NotNullWhen(true)] out T? transport) where T : class, IUnreliableTransport
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves <see cref="IUnreliableTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <returns>Transport instance or <c>null</c> when not found.</returns>
        public T GetGeneralTransport<T>() where T : class, IUnreliableTransport
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Iterates over all <see cref="IUnreliableTransport"/>s using a given <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action to use on all registered <see cref="IUnreliableTransport"/>s.</param>
        public void ForEachGeneralTransport(TransportConsumer<IUnreliableTransport> action)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all <see cref="IUnreliableTransport"/>s
        /// and calls <see cref="ITransport.Terminate(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Terminate(NetworkMember)"/>.
        /// </returns>
        public bool ClearGeneralTransports()
        {
            throw new NotImplementedException();
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
        public T GetUnreliableTransport<T>() where T : class, IUnreliableTransport
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
        /// Iterates over all <see cref="IUnreliableTransport"/>s using a given <paramref name="action"/>.
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
        public T GetReliableTransport<T>() where T : class, IReliableTransport
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
        /// Iterates over all <see cref="IReliableTransport"/>s using a given <paramref name="action"/>.
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

        #region Sequential transport registration/removal.
        /// <summary>
        /// Registers <see cref="ISequentialTransport"/>.
        /// </summary>
        public void RegisterSequentialTransport<T>(T transport) where T : class, ISequentialTransport
        {
            if (typeof(T) == typeof(ISequentialTransport))
            {
                throw new ArgumentException($"You should only register transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                if (SequentialTransports.Remove(out T? removed))
                {
                    removed.InvokeTerminate(this);
                }

                SequentialTransports.Add(transport);
                transport.InvokeInitialize(this);
            }
        }

        /// <summary>
        /// Tries to remove <see cref="ISequentialTransport"/> from the map of active transports.
        /// </summary>
        /// <typeparam name="T">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport which was removed just a moment ago.</param>
        /// <returns>
        /// <c>true</c> if transport was present, was removed and the instance is provided as <paramref name="transport"/>.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveNotifyTransport<T>([NotNullWhen(true)] out T? transport) where T : class, ISequentialTransport
        {
            if (typeof(T) == typeof(ISequentialTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                if (SequentialTransports.Remove(out transport))
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
        public bool RemoveSequentialTransport<T>(T transport) where T : class, ISequentialTransport
        {
            if (typeof(T) == typeof(ISequentialTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                if (SequentialTransports.Remove(transport))
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
        public bool HasNotifyTransport<T>() where T : class, ISequentialTransport
        {
            if (typeof(T) == typeof(ISequentialTransport))
            {
                throw new ArgumentException($"You should only check transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return SequentialTransports.Has<T>();
            }
        }

        /// <summary>
        /// Tries to retrieve <see cref="ISequentialTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <param name="transport">Transport instance or <c>null</c> when not found.</param>
        /// <returns>
        /// <c>true</c> if found and <paramref name="transport"/> was provided.
        /// <c>false</c> if not found and <paramref name="transport"/> is null.
        /// </returns>
        public bool TryGetNotifyTransport<T>([NotNullWhen(true)] out T? transport) where T : class, ISequentialTransport
        {
            if (typeof(T) == typeof(ISequentialTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return SequentialTransports.TryGet(out transport);
            }
        }

        /// <summary>
        /// Retrieves <see cref="ISequentialTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <returns>Transport instance or <c>null</c> when not found.</returns>
        public T GetSequentialTransport<T>() where T : class, ISequentialTransport
        {
            if (typeof(T) == typeof(ISequentialTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return SequentialTransports.Get<T>();
            }
        }

        /// <summary>
        /// Iterates over all <see cref="ISequentialTransport"/>s using a given <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action to use on all registered <see cref="ISequentialTransport"/>s.</param>
        public void ForEachNotifyTransport(TransportConsumer<ISequentialTransport> action)
        {
            lock (_lock)
            {
                foreach (var transport in SequentialTransports)
                {
                    action(transport);
                }
            }
        }

        /// <summary>
        /// Removes all <see cref="ISequentialTransport"/>s
        /// and calls <see cref="ITransport.Terminate(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Terminate(NetworkMember)"/>.
        /// </returns>
        public bool ClearNotifyTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in SequentialTransports)
                {
                    anyFailed = !transport.InvokeTerminate(this);
                }

                return !anyFailed;
            }
        }
        #endregion

        #region Resilient transport registration/removal.
        /// <summary>
        /// Registers <see cref="IResilientTransport"/>.
        /// </summary>
        public void RegisterResilientTransport<T>(T transport) where T : class, IResilientTransport
        {
            if (typeof(T) == typeof(IResilientTransport))
            {
                throw new ArgumentException($"You should only register transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                if (ResilientTransports.Remove(out T? removed))
                {
                    removed.InvokeTerminate(this);
                }

                ResilientTransports.Add(transport);
                transport.InvokeInitialize(this);
            }
        }

        /// <summary>
        /// Tries to remove <see cref="IResilientTransport"/> from the map of active transports.
        /// </summary>
        /// <typeparam name="T">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport which was removed just a moment ago.</param>
        /// <returns>
        /// <c>true</c> if transport was present, was removed and the instance is provided as <paramref name="transport"/>.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveResilientTransport<T>([NotNullWhen(true)] out T? transport) where T : class, IResilientTransport
        {
            if (typeof(T) == typeof(IResilientTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                if (ResilientTransports.Remove(out transport))
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
        public bool RemoveResilientTransport<T>(T transport) where T : class, IResilientTransport
        {
            if (typeof(T) == typeof(IResilientTransport))
            {
                throw new ArgumentException($"You should only remove transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                if (ResilientTransports.Remove(transport))
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
        public bool HasResilientTransport<T>() where T : class, IResilientTransport
        {
            if (typeof(T) == typeof(IResilientTransport))
            {
                throw new ArgumentException($"You should only check transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return ResilientTransports.Has<T>();
            }
        }

        /// <summary>
        /// Tries to retrieve <see cref="IResilientTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <param name="transport">Transport instance or <c>null</c> when not found.</param>
        /// <returns>
        /// <c>true</c> if found and <paramref name="transport"/> was provided.
        /// <c>false</c> if not found and <paramref name="transport"/> is null.
        /// </returns>
        public bool TryGetResilientTransport<T>([NotNullWhen(true)] out T? transport) where T : class, IResilientTransport
        {
            if (typeof(T) == typeof(IResilientTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return ResilientTransports.TryGet(out transport);
            }
        }

        /// <summary>
        /// Retrieves <see cref="IResilientTransport"/> under a given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of transport to look for.</typeparam>
        /// <returns>Transport instance or <c>null</c> when not found.</returns>
        public T GetResilientTransport<T>() where T : class, IResilientTransport
        {
            if (typeof(T) == typeof(IResilientTransport))
            {
                throw new ArgumentException($"You should only retrieve transports with explicitly defined end-type - do not rely on the base.");
            }

            lock (_lock)
            {
                return ResilientTransports.Get<T>();
            }
        }

        /// <summary>
        /// Iterates over all <see cref="IResilientTransport"/>s using a given <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action to use on all registered <see cref="IResilientTransport"/>s.</param>
        public void ForEachResilientTransport(TransportConsumer<IResilientTransport> action)
        {
            lock (_lock)
            {
                foreach (var transport in ResilientTransports)
                {
                    action(transport);
                }
            }
        }

        /// <summary>
        /// Removes all <see cref="IResilientTransport"/>s
        /// and calls <see cref="ITransport.Terminate(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Terminate(NetworkMember)"/>.
        /// </returns>
        public bool ClearResilientTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in ResilientTransports)
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
            lock (_lock)
                return ResilientTransports.Count
                    + ReliableTransports.Count
                    + SequentialTransports.Count
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

                foreach (var t in SequentialTransports)
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
                return ((target + Increment - 1) >> IncrementTotalBits) << IncrementTotalBits;
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

        /// TODO: Implement logger from ServiceCore instead.
        private static void LogException(Exception exception)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(exception);
            Console.ForegroundColor = color;
        }
    }
}
