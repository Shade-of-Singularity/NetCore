using NetCore.Common;
using NetCore.Transports;
using System.Buffers;
using System.Runtime.CompilerServices;

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
    public abstract partial class NetworkMember : INetworkMemberStatistics, ISendToConnectionNetworkMessaging, ITransportBasedSendToConnectionNetworkMessaging
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
        /// Constructs <see cref="NetworkMember"/> with a specified initial transport capacity - <paramref name="capacity"/>.
        /// </summary>
        /// <param name="capacity">Initial capacity for transports to pre-initialize.</param>
        public NetworkMember(int capacity = DefaultInitialTransportCapacity)
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
        protected virtual async OperationResultTask StartOperation(StartupArgs args, CancellationToken token)
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
                        for (int j = i - 1; j >= 0; j--)
                        {
                            await transports[i].InvokeStop();
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
        protected virtual async OperationResultTask StopOperation()
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
        protected virtual async OperationResultTask ConnectOperation(ConnectionArgs args, CancellationToken token)
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
                        for (int j = i - 1; j >= 0; j--)
                        {
                            await transports[i].InvokeDisconnect();
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
        protected virtual async OperationResultTask DisconnectOperation()
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
