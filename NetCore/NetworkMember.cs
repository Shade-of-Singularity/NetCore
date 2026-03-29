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
    /// State exception of an network member.
    /// Usually thrown when you try to use a networking method, but target <see cref="NetworkMember"/> is in a state which doesn't allow given function right now.
    /// Some of those exceptions (especially when explicitly stated) are system bugs and needs to be reported to us.
    /// </summary>
    /// <param name="message"><inheritdoc/></param>
    public sealed class MemberStateException(string message) : Exception(message);

    /// <summary>
    /// Thrown when you attempt to use networking methods on a member, which was not started yet or is still starting.
    /// </summary>
    /// <param name="message"><inheritdoc/></param>
    public sealed class MemberIsNotStartedException(string message) : Exception(message);

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
        /// Iterates over all unreliable transports under a lock, using a given <paramref name="consumer"/> delegate.
        /// </summary>
        /// <param name="consumer">Action to use on all registered <see cref="IUnreliableTransport"/>s.</param>
        public void ForEachUnreliableTransport(MemberTransportConsumer<T, IUnreliableTransport> consumer)
        {
            // TODO: Consider using injection and/or give a way to provide arguments for a delegate.
            //  This is so runtime won't have to allocate a class for remembering the variables.
            //  A way for accessing the Lookup itself under a lock can also be considered.
            lock (_lock)
            {
                T self = (T)this;
                foreach (var transport in UnreliableTransports)
                {
                    consumer(self, transport);
                }
            }
        }
        /// <summary>
        /// Iterates over all reliable transports under a lock, using a given <paramref name="consumer"/> delegate.
        /// </summary>
        /// <param name="consumer">Action to use on all registered <see cref="IReliableTransport"/>s.</param>
        public void ForEachReliableTransport(MemberTransportConsumer<T, IReliableTransport> consumer)
        {
            // TODO: Consider using injection and/or give a way to provide arguments for a delegate.
            //  This is so runtime won't have to allocate a class for remembering the variables.
            //  A way for accessing the Lookup itself under a lock can also be considered.
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
        /// Iterates over all sequential transports under a lock, using a given <paramref name="consumer"/> delegate.
        /// </summary>
        /// <param name="consumer">Action to use on all registered <see cref="ISequentialTransport"/>s.</param>
        public void ForEachSequentialTransport(MemberTransportConsumer<T, ISequentialTransport> consumer)
        {
            // TODO: Consider using injection and/or give a way to provide arguments for a delegate.
            //  This is so runtime won't have to allocate a class for remembering the variables.
            //  A way for accessing the Lookup itself under a lock can also be considered.
            lock (_lock)
            {
                T self = (T)this;
                foreach (var transport in SequentialTransports)
                {
                    consumer(self, transport);
                }
            }
        }
        /// <summary>
        /// Iterates over all resilient transports under a lock, using a given <paramref name="consumer"/> delegate.
        /// </summary>
        /// <param name="consumer">Action to use on all registered <see cref="IResilientTransport"/>s.</param>
        public void ForEachResilientTransport(MemberTransportConsumer<T, IResilientTransport> consumer)
        {
            // TODO: Consider using injection and/or give a way to provide arguments for a delegate.
            //  This is so runtime won't have to allocate a class for remembering the variables.
            //  A way for accessing the Lookup itself under a lock can also be considered.
            lock (_lock)
            {
                T self = (T)this;
                foreach (var transport in ResilientTransports)
                {
                    consumer(self, transport);
                }
            }
        }
    }


    /// <summary>
    /// Base class for <see cref="Server"/> and <see cref="Client"/>.
    /// </summary>
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
    /// TODO: Make sure transports can only be added/removed if member is not started already.
    public abstract partial class NetworkMember : INetworkMemberStatistics
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
        public StartupState StartupState
        {
            get
            {
                MemberState state;
                lock (_lock) state = m_State;
                return state switch
                {
                    MemberState.Stopped => StartupState.Stopped,
                    MemberState.Starting => StartupState.Starting,
                    MemberState.Stopping => StartupState.Stopping,

                    MemberState.Started_Idle => StartupState.Started,
                    MemberState.Started_Connecting => StartupState.Started,
                    MemberState.Started_Disconnecting => StartupState.Started,
                    MemberState.Started_Connected => StartupState.Started,

                    _ => throw new SwitchExpressionException(state),
                };
            }
        }
        /// <summary>
        /// Current state of the connection.
        /// </summary>
        /// <remarks>
        /// If there are no transports registered - <see cref="NetworkMember"/> almost immediately
        /// marked as <see cref="ConnectionState.Connected"/> on <see cref="Connect"/> method call.
        /// Otherwise - marked as <see cref="ConnectionState.Connected"/> when at least one transport is connected.
        /// </remarks>
        public ConnectionState ConnectionState
        {
            get
            {
                MemberState state;
                lock (_lock) state = m_State;
                return state switch
                {
                    MemberState.Stopped => ConnectionState.Idle,
                    MemberState.Starting => ConnectionState.Idle,
                    MemberState.Stopping => ConnectionState.Idle,
                    MemberState.Started_Idle => ConnectionState.Idle,

                    MemberState.Started_Connecting => ConnectionState.Connecting,
                    MemberState.Started_Disconnecting => ConnectionState.Disconnecting,
                    MemberState.Started_Connected => (Transports.Count == 0 || m_ConnectedTransportsCount > 0) ? ConnectionState.Connected : ConnectionState.Connecting,

                    _ => throw new SwitchExpressionException(state),
                };
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
        /// CRTP-based list (similar to dictionary), which stores references to all transports this <see cref="NetworkMember"/> manages. 
        /// </summary>
        protected readonly CRTPList<ITransport> Transports;
        /// <summary>
        /// Lookup for all <see cref="IResilientTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected CRTPList<ITransport>.Lookup<IResilientTransport> ResilientTransports;
        /// <summary>
        /// Lookup for all <see cref="IReliableTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected CRTPList<ITransport>.Lookup<IReliableTransport> ReliableTransports;
        /// <summary>
        /// Lookup for all <see cref="ISequentialTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected CRTPList<ITransport>.Lookup<ISequentialTransport> SequentialTransports;
        /// <summary>
        /// Lookup for all <see cref="IUnreliableTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected CRTPList<ITransport>.Lookup<IUnreliableTransport> UnreliableTransports;
        /// <summary>
        /// Lookup for all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected CRTPList<ITransport>.Lookup<IStreamTransport> StreamTransports;
        /// <summary>
        /// Lookup for all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        /// <remarks>
        /// Not readonly to support mutation in registration methods.
        /// </remarks>
        protected CRTPList<ITransport>.Lookup<IFileTransport> FileTransports;
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




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Constructs <see cref="NetworkMember"/> with a default initial transport capacity - <see cref="DefaultInitialTransportCapacity"/>.
        /// </summary>
        public NetworkMember() : this(DefaultInitialTransportCapacity) { }

        /// <summary>
        /// Constructs <see cref="NetworkMember"/> with a specified initial transport capacity - <paramref name="capacity"/>.
        /// </summary>
        /// <param name="capacity">Initial capacity for transports to pre-initialize.</param>
        public NetworkMember(int capacity)
        {
            Transports = new(capacity);
            // TODO: Review after fully implementing .GetLookup<T>() method.
            ResilientTransports = Transports.GetLookup<IResilientTransport>();
            SequentialTransports = Transports.GetLookup<ISequentialTransport>();
            UnreliableTransports = Transports.GetLookup<IUnreliableTransport>();
            ReliableTransports = Transports.GetLookup<IReliableTransport>();
            StreamTransports = Transports.GetLookup<IStreamTransport>();
            FileTransports = Transports.GetLookup<IFileTransport>();
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Starts all transports with a given <see cref="NetCore.StartupArgs"/>.
        /// </summary>
        /// <remarks>
        /// If any of the transports fail - unless this operation is cancelled, run <see cref="ITransport.InvokeStop"/> on all started transports.
        /// This will reset their state back to uninitialized state.
        /// Note: Remove the note unless transactional system is implemented in <see cref="ITransport.InvokeStart"/>.
        /// </remarks>
        protected virtual async UniTask<OperationResult> StartOperation(StartupArgs args, CancellationToken token)
        {
            // TODO: Support multi-threaded startup.
            ITransport[] transports = RentTransports(out int total);

            try
            {
                for (int i = 0; i < total; i++)
                {
                    if (token.IsCancellationRequested)
                        return OperationResult.CancelledOrInvalid;

                    if (await transports[i].InvokeStart(args) == OperationResult.Failed)
                    {
                        // Resets state on failure.
                        while (i >= 0)
                        {
                            await transports[i--].InvokeStop();
                        }

                        return OperationResult.Failed;
                    }
                }

                return OperationResult.Success;
            }
            finally
            {
                ReturnTransports(transports);
            }
        }

        /// <summary>
        /// Stops all transports. This operation cannot be cancelled.
        /// <para>
        /// This operation can complete synchronously.
        /// </para>
        /// <para>
        /// When this method is called - transports are already started.
        /// If <see cref="StartOperation"/> was previously cancelled - some <see cref="ITransport"/>s might remain started, while other will remain stopped.
        /// This method is expected to call <see cref="ITransport.InvokeStop"/> to return all transports to a previous state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>This operation cannot be cancelled.</para>
        /// Instead - all other "activation" operations (e.g. <see cref="StartOperation"/>) will wait for this one to complete.
        /// </remarks>
        protected virtual async UniTask<OperationResult> StopOperation()
        {
            // TODO: transports can change between start and stop calls.
            //  In case this happens - on removal, Disconnect and Stop operations should run on a transport.
            //  And they should be awaited until completed on Initialize, Start or Connect calls when assigned to another member or on manual call.
            ITransport[] transports = RentTransports(out int total);

            try
            {
                for (int i = 0; i < total; i++)
                {
                    if (await transports[i].InvokeStop() == OperationResult.Failed)
                        return OperationResult.Failed;
                }

                return OperationResult.Success;
            }
            finally
            {
                ReturnTransports(transports);
            }
        }

        /// <summary>
        /// Asks all transports to connect to a remote end-point, specified with provided <see cref="NetCore.ConnectionArgs"/>.
        /// If used server-side (e.g. in <see cref="Server"/>) - remote end-point is treated as a relay.
        /// <para>
        /// When this method is called - all transports are in a disconnected state.
        /// This is enforced by the system architecture (unless developer manually interfere).
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>This operation cannot be cancelled.</para>
        /// If <paramref name="token"/> is cancelled - a <see cref="DisconnectOperation"/> will run shortly after this one quits.
        /// Additionally - it will physically wait for this operation to stop.
        /// This was bade for better reliability with async operations.
        /// </remarks>
        protected virtual async UniTask<OperationResult> ConnectOperation(ConnectionArgs args, CancellationToken token)
        {
            ITransport[] transports = RentTransports(out int total);

            try
            {
                for (int i = 0; i < total; i++)
                {
                    if (token.IsCancellationRequested)
                        return OperationResult.CancelledOrInvalid;

                    if (await transports[i].InvokeConnect(args) == OperationResult.Failed)
                    {
                        // Resets state on failure.
                        while (i >= 0)
                        {
                            await transports[i--].InvokeDisconnect();
                        }

                        return OperationResult.Failed;
                    }
                }

                return OperationResult.Success;
            }
            finally
            {
                ReturnTransports(transports);
            }
        }

        /// <summary>
        /// Disconnects all transports. This operation cannot be cancelled.
        /// <para>
        /// When this method is called - all transports are connected.
        /// If <see cref="ConnectOperation"/> was cancelled - some transports might be either fully connected, or was never connected in a first place.
        /// This method is expected to call <see cref="ITransport.InvokeDisconnect"/>, to reset all transports back to disconnected state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>This operation cannot be cancelled.</para>
        /// This operation cannot be cancelled.
        /// Instead - all other "activation" operations (e.g. <see cref="StartOperation"/>) will wait for this one to complete.
        /// </remarks>
        protected virtual async UniTask<OperationResult> DisconnectOperation()
        {
            // TODO: transports can change between start and stop calls.
            //  In case this happens - on removal, Disconnect and Stop operations should run on a transport.
            //  And they should be awaited until completed on Initialize, Start or Connect calls when assigned to another member or on manual call.
            ITransport[] transports = RentTransports(out int total);

            try
            {
                for (int i = 0; i < total; i++)
                {
                    if (await transports[i].InvokeDisconnect() == OperationResult.Failed)
                        return OperationResult.Failed;
                }

                return OperationResult.Success;
            }
            finally
            {
                ReturnTransports(transports);
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               SendTo Messages
        /// .                                A.K.A.: Good luck translating documentation.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region General SendTo args methods
        /// <summary>
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliableTo(ref header, datagram, ref flags, args); return;
                case SendingMode.Reliable: SendReliableTo(ref header, datagram, ref flags, args); return;
                case SendingMode.Sequential: SendSequentialTo(ref header, datagram, ref flags, args); return;
                case SendingMode.Resilient: SendResilientTo(ref header, datagram, ref flags, args); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point, outside of a current connection.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliableTo<TTransport>(ref header, datagram, ref flags, args); return;
                case SendingMode.Reliable: SendReliableTo<TTransport>(ref header, datagram, ref flags, args); return;
                case SendingMode.Sequential: SendSequentialTo<TTransport>(ref header, datagram, ref flags, args); return;
                case SendingMode.Resilient: SendResilientTo<TTransport>(ref header, datagram, ref flags, args); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliableTo(ref header, datagram, ref flags, args),
                SendingMode.Reliable => TrySendReliableTo(ref header, datagram, ref flags, args),
                SendingMode.Sequential => TrySendSequentialTo(ref header, datagram, ref flags, args),
                SendingMode.Resilient => TrySendResilientTo(ref header, datagram, ref flags, args),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point, outside of a current connection.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliableTo<TTransport>(ref header, datagram, ref flags, args),
                SendingMode.Reliable => TrySendReliableTo<TTransport>(ref header, datagram, ref flags, args),
                SendingMode.Sequential => TrySendSequentialTo<TTransport>(ref header, datagram, ref flags, args),
                SendingMode.Resilient => TrySendResilientTo<TTransport>(ref header, datagram, ref flags, args),
                _ => throw new SwitchExpressionException(mode),
            };
        }
        #endregion

        #region Narrow SendTo args methods - Unreliable
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendUnreliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            using (header.Lock()) using (flags.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliableTo(datagram, header, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to unreliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliableTo"/>
        public bool TrySendUnreliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliableTo(datagram, header, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendUnreliableTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendUnreliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetUnreliableTransport<TTransport>().SendUnreliableTo(datagram, header, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to unreliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliableTo{TTransport}"/>
        public bool TrySendUnreliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (UnreliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendUnreliableTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow SendTo args methods - Reliable
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendReliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliableTo(datagram, header, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to reliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliableTo"/>
        public bool TrySendReliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliableTo(datagram, header, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendReliableTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendReliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetReliableTransport<TTransport>().SendReliableTo(datagram, header, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to reliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliableTo{TTransport}"/>
        public bool TrySendReliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (ReliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendReliableTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow SendTo args methods - Sequential
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendSequentialTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequentialTo(datagram, header, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequentialTo"/>
        public bool TrySendSequentialTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequentialTo(datagram, header, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendSequentialTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendSequentialTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetSequentialTransport<TTransport>().SendSequentialTo(datagram, header, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequentialTo{TTransport}"/>
        public bool TrySendSequentialTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (SequentialTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendSequentialTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow SendTo args methods - Resilient
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendResilientTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilientTo(datagram, header, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilientTo"/>
        public bool TrySendResilientTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilientTo(datagram, header, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendResilientTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendResilientTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetResilientTransport<TTransport>().SendResilientTo(datagram, header, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilientTo{TTransport}"/>
        public bool TrySendResilientTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (ResilientTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendResilientTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                            Transport Management
        /// .                  TODO: Only terminate transport if it was removed from ALL other storages.
        /// .       Note: this might be easier to resolve if we provide custom storage solution, allowing for filtering.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region Any transport management
        /// <summary>
        /// Check whether transport of a specified type is registered.
        /// </summary>
        public bool HasTransport<TTransport>() where TTransport : ITransport
        {
            lock (_lock) return Transports.Has<TTransport>();
        }

        /// <inheritdoc cref="HasTransport{TTransport}()"/>
        protected bool HasTransportCore<TTransport>() where TTransport : ITransport
        {
            return Transports.Has<TTransport>();
        }

        /// <summary>
        /// Checks whether transport of a specified type is registered and belongs to a specific sending mode group.
        /// </summary>
        public bool HasTransport<TTransport>(SendingMode mode) where TTransport : ITransport
        {
            lock (_lock) return mode switch
            {
                SendingMode.Unreliable => UnreliableTransports.Has<TTransport>(),
                SendingMode.Reliable => ReliableTransports.Has<TTransport>(),
                SendingMode.Sequential => SequentialTransports.Has<TTransport>(),
                SendingMode.Resilient => ResilientTransports.Has<TTransport>(),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <inheritdoc cref="HasTransport{TTransport}(SendingMode)"/>
        protected bool HasTransportCore<TTransport>(SendingMode mode) where TTransport : ITransport => mode switch
        {
            SendingMode.Unreliable => UnreliableTransports.Has<TTransport>(),
            SendingMode.Reliable => ReliableTransports.Has<TTransport>(),
            SendingMode.Sequential => SequentialTransports.Has<TTransport>(),
            SendingMode.Resilient => ResilientTransports.Has<TTransport>(),
            _ => throw new SwitchExpressionException(mode),
        };

        /// <summary>
        /// Checks if there is any transport, at all, than will be able to handle a given <see cref="SendingMode"/>.
        /// Used in broadcasting TrySend methods.
        /// </summary>
        public bool HasAnyTransport(SendingMode mode)
        {
            lock (_lock) return mode switch
            {
                SendingMode.Unreliable => UnreliableTransports.Count > 0,
                SendingMode.Reliable => ReliableTransports.Count > 0,
                SendingMode.Sequential => SequentialTransports.Count > 0,
                SendingMode.Resilient => ResilientTransports.Count > 0,
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <inheritdoc cref="HasAnyTransport(SendingMode)"/>
        public bool HasAnyTransportCore(SendingMode mode) => mode switch
        {
            SendingMode.Unreliable => UnreliableTransports.Count > 0,
            SendingMode.Reliable => ReliableTransports.Count > 0,
            SendingMode.Sequential => SequentialTransports.Count > 0,
            SendingMode.Resilient => ResilientTransports.Count > 0,
            _ => throw new SwitchExpressionException(mode),
        };
        #endregion

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
        /// <typeparam name="TTransport">Type of transport to remove.</typeparam>
        /// <param name="transport">Transport to remove.</param>
        /// <returns>
        /// <c>true</c> if transport was present and it was removed.
        /// <c>false</c> if transport was not present and thus - was not removed.
        /// </returns>
        public bool RemoveGeneralTransport<TTransport>(TTransport transport) where TTransport : class,
            IResilientTransport, ISequentialTransport, IUnreliableTransport, IReliableTransport
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether this <see cref="NetworkMember"/> manages a specific reliable transport.
        /// </summary>
        public bool HasGeneralTransport<TTransport>() where TTransport : class,
            IResilientTransport, ISequentialTransport, IUnreliableTransport, IReliableTransport
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to retrieve <see cref="IUnreliableTransport"/> under a given <typeparamref name="TTransport"/> type.
        /// </summary>
        /// <typeparam name="TTransport">Type of transport to look for.</typeparam>
        /// <param name="transport">Transport instance or <c>null</c> when not found.</param>
        /// <returns>
        /// <c>true</c> if found and <paramref name="transport"/> was provided.
        /// <c>false</c> if not found and <paramref name="transport"/> is null.
        /// </returns>
        public bool TryGetGeneralTransport<TTransport>([NotNullWhen(true)] out TTransport? transport) where TTransport : class,
            IResilientTransport, ISequentialTransport, IUnreliableTransport, IReliableTransport
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves <see cref="IUnreliableTransport"/> under a given <typeparamref name="TTransport"/> type.
        /// </summary>
        /// <typeparam name="TTransport">Type of transport to look for.</typeparam>
        /// <returns>Transport instance or <c>null</c> when not found.</returns>
        public TTransport GetGeneralTransport<TTransport>() where TTransport : class,
            IResilientTransport, ISequentialTransport, IUnreliableTransport, IReliableTransport
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Iterates over all <see cref="IUnreliableTransport"/>s using a given <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action to use on all registered <see cref="IUnreliableTransport"/>s.</param>
        public void ForEachGeneralTransport(TransportConsumer<ITransport> action)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all <see cref="IUnreliableTransport"/>s
        /// and calls <see cref="ITransport.Detach(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Detach(NetworkMember)"/>.
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
                    removed.InvokeDetach();
                }

                UnreliableTransports.Add(transport);
                transport.InvokeAttach(this);
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
                    transport.InvokeDetach();
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
                    transport.InvokeDetach();
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
        /// and calls <see cref="ITransport.Detach(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Detach(NetworkMember)"/>.
        /// </returns>
        public bool ClearUnreliableTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in ReliableTransports)
                {
                    anyFailed = !transport.InvokeDetach();
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
                    removed.InvokeDetach();
                }

                ReliableTransports.Add(transport);
                transport.InvokeAttach(this);
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
                    transport.InvokeDetach();
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
                    transport.InvokeDetach();
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
        /// and calls <see cref="ITransport.Detach(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Detach(NetworkMember)"/>.
        /// </returns>
        public bool ClearReliableTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in ReliableTransports)
                {
                    anyFailed = !transport.InvokeDetach();
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
                    removed.InvokeDetach();
                }

                SequentialTransports.Add(transport);
                transport.InvokeAttach(this);
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
                    transport.InvokeDetach();
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
                    transport.InvokeDetach();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Checks whether this <see cref="NetworkMember"/> manages a specific reliable transport.
        /// </summary>
        public bool HasSequentialTransport<T>() where T : class, ISequentialTransport
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
        /// and calls <see cref="ITransport.Detach(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Detach(NetworkMember)"/>.
        /// </returns>
        public bool ClearNotifyTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in SequentialTransports)
                {
                    anyFailed = !transport.InvokeDetach();
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
                    removed.InvokeDetach();
                }

                ResilientTransports.Add(transport);
                transport.InvokeAttach(this);
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
                    transport.InvokeDetach();
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
                    transport.InvokeDetach();
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
        /// and calls <see cref="ITransport.Detach(NetworkMember)"/> of all of them.
        /// </summary>
        /// <returns>
        /// <c>true</c> - all transports were removed successfully.
        /// <c>false</c> - some transports had issues executing <see cref="ITransport.Detach(NetworkMember)"/>.
        /// </returns>
        public bool ClearResilientTransports()
        {
            lock (_lock)
            {
                bool anyFailed = false;
                foreach (var transport in ResilientTransports)
                {
                    anyFailed = !transport.InvokeDetach();
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
        /// Lists all transports in a newly created or rented array.
        /// </summary>
        /// <returns>All current transports.</returns>
        protected virtual ITransport[] RentTransports(out int total)
        {
            lock (_lock)
            {
                // We cache transports inside the lock to avoid race conditions.
                total = Transports.Count;
                ITransport[] transports = ArrayPool<ITransport>.Shared.Rent(NextSize(total));
                Transports.CopyTo(transports);
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
