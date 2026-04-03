using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace NetCore.Transports
{
    // TODO: Implement dispatcher at some point.
    public partial interface ITransport // TODO: Provide provider-based methods for args handling.
    {
        /// <summary>
        /// Current state of the <see cref="ITransport"/>.
        /// </summary>
        MemberState State { get; protected set; }
        /// <summary>
        /// Lock object used in case of concurrent interactions with this <see cref="ITransport"/>.
        /// </summary>
        protected object Lock { get; }
        /// <summary>
        /// Token source for cancelling <see cref="StartOperation"/>.
        /// </summary>
        /// <remarks>
        /// Managed by <see cref="ITransport"/>'s state machine!
        /// </remarks>
        protected CancellationTokenSource? StartTokenSource { get; set; }
        /// <summary>
        /// An <see cref="InvokeStart"/> operation.
        /// </summary>
        /// <remarks>
        /// Managed completely by <see cref="ITransport"/>'s state machine!
        /// </remarks>
        protected OperationResultTask StartOperation { get; set; }
        /// <summary>
        /// Token source for cancelling an internal <see cref="InvokeStop"/> operation.
        /// </summary>
        /// <remarks>
        /// Managed completely by <see cref="ITransport"/>'s state machine!
        /// </remarks>
        protected CancellationTokenSource? StopTokenSource { get; set; }
        /// <summary>
        /// Token source for cancelling <see cref="ConnectionOperation"/>.
        /// </summary>
        /// <remarks>
        /// Managed completely by <see cref="ITransport"/>'s state machine!
        /// </remarks>
        protected CancellationTokenSource? ConnectionTokenSource { get; set; }
        /// <summary>
        /// An <see cref="InvokeConnect"/> operation.
        /// </summary>
        /// <remarks>
        /// Managed completely by <see cref="ITransport"/>'s state machine!
        /// </remarks>
        protected OperationResultTask ConnectionOperation { get; set; }
        /// <summary>
        /// Token source for cancelling an internal <see cref="InvokeDisconnect"/> operation.
        /// </summary>
        /// <remarks>
        /// Managed completely by <see cref="ITransport"/>'s state machine!
        /// </remarks>
        protected CancellationTokenSource? DisconnectionTokenSource { get; set; }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .  State machine (v2) operations cover: Start, Stop, Connect, Disconnect methods and supplementary methods.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// If <see cref="ITransport"/> is stopped - starts it using given <paramref name="args"/>, and provides a <paramref name="task"/> to await.
        /// </summary>
        /// <param name="task">Task from <see cref="InvokeStart"/> to await, or a completed task.</param>
        /// <param name="args"><see cref="StartupArgs"/> to use for starting this <see cref="ITransport"/></param>
        /// <returns>
        /// <see langword="true"/> if it was possible to start this <see cref="ITransport"/> and <paramref name="task"/> was provided for awaiting.
        /// <see langword="false"/> if <see cref="ITransport"/> was already started.
        /// </returns>
        public bool TryInvokeStart(out OperationResultTask task, IReadOnlyStartupArgs args)
        {
            lock (Lock)
            {
                if (State == MemberState.Stopped)
                {
                    task = StartCoreUnlockedPreserved(args);
                    return true;
                }

                task = StateMachineHelpers.CompletedTask;
                return false;
            }
        }

        /// <summary>
        /// Stops <see cref="ITransport"/> if it currently runs, and start it again with provided <paramref name="args"/>.
        /// </summary>
        /// <param name="args"><see cref="StartupArgs"/> to use for starting this <see cref="ITransport"/></param>
        /// <returns>Startup task to await.</returns>
        public OperationResultTask InvokeStart(IReadOnlyStartupArgs args)
        {
            lock (Lock) return StartCoreUnlockedPreserved(args);
        }

        /// <inheritdoc cref="Start"/>
        /// Note: Returned task IS preserved, as it is cached in <see cref="StartOperation"/>.
        private OperationResultTask StartCoreUnlockedPreserved(IReadOnlyStartupArgs args)
        {
            var task = StopCoreUnlockedUnpreserved();
            // Creates token after calling a stop operation, because it will immediately cancel a startup token.
            StartTokenSource = new();
#if PRESERVE
            return StartOperation = InvokeStartInternal(task, args, StartTokenSource).Preserve();
#else
            return StartOperation = InvokeStartInternal(task, args, StartTokenSource);
#endif
        }

        private async OperationResultTask InvokeStartInternal(OperationResultTask stopping, IReadOnlyStartupArgs args, CancellationTokenSource source)
        {
            try { await stopping; } catch { /* Stop operation, of there is any, has to complete (finish, cancel) first. */ }

            lock (Lock)
            {
                if (source.IsCancellationRequested)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                        StartTokenSource = null;

                    return OperationResult.CancelledOrInvalid;
                }

                // Note: State should only change after await block, because the task above can mutate the state as well.
                State = MemberState.Starting;
            }

            OperationResult result = OperationResult.Failed;
            try
            {
                await Start(args, source.Token);
                return result = OperationResult.Success;
            }
            catch (Exception exception)
            {
                // TODO: Standardize and import ServiceCoreLogger, and use it here.
                Console.ForegroundColor = ConsoleColor.Red;
                if (exception is SocketException se)
                    Console.WriteLine($"SocketException Code: {se.ErrorCode}");
                Console.WriteLine(exception);
                Console.ForegroundColor = ConsoleColor.White;

                try
                {
                    await Stop();
                }
                catch (Exception inner)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    if (exception is SocketException se2)
                        Console.WriteLine($"SocketException Code: {se2.ErrorCode}");
                    Console.WriteLine(inner);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                return OperationResult.Failed;
            }
            finally
            {
                lock (Lock)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                    {
                        StartTokenSource = null;
                        State = result == OperationResult.Success ? MemberState.Started_Idle : MemberState.Stopped;
                    }
                }
            }
        }




        /// <summary>
        /// Stops <see cref="ITransport"/> if it is currently running.
        /// </summary>
        /// <returns>Stop task to await a completion of.</returns>
        public OperationResultTask InvokeStop()
        {
#if PRESERVE
            lock (Lock) return StopCoreUnlockedUnpreserved().Preserve();
#else
            lock (Lock) return StopCoreUnlockedUnpreserved();
#endif
        }

        /// <inheritdoc cref="InvokeStop"/>
        /// Note: Returned task is NOT preserved, as it is not cached anywhere.
        private OperationResultTask StopCoreUnlockedUnpreserved()
        {
            OperationResultTask cancelledStarting;
            if (StartTokenSource is null) cancelledStarting = StateMachineHelpers.CompletedTask;
            else
            {
                StartTokenSource.Cancel();
                StartTokenSource = null;
                (cancelledStarting, StartOperation) = (StartOperation, StateMachineHelpers.CompletedTask);
            }

            StopTokenSource?.Cancel();
            CancellationTokenSource source = new();
            StopTokenSource = source;
            return InvokeStopInternal(DisconnectCoreUnlockedUnpreserved(), cancelledStarting, source);
        }

        private async OperationResultTask InvokeStopInternal(OperationResultTask disconnecting, OperationResultTask cancelledStarting, CancellationTokenSource source)
        {
            try { await disconnecting; } catch { /* Disconnect operation, of there is any, has to complete (finish, cancel) first. */ }
            try { await cancelledStarting; } catch { /* Any cancelled operations must wait as well. */ }
            lock (Lock)
            {
                if (source.IsCancellationRequested)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                        StartTokenSource = null;

                    return OperationResult.CancelledOrInvalid;
                }

                // Note: State should only change after await block, because tasks above can mutate the state as well.
                State = MemberState.Stopping;
            }

            try
            {
                await Stop();
                return OperationResult.Success;
            }
            catch (Exception exception)
            {
                // TODO: Standardize and import ServiceCoreLogger, and use it here.
                Console.ForegroundColor = ConsoleColor.Red;
                if (exception is SocketException se)
                    Console.WriteLine($"SocketException Code: {se.ErrorCode}");
                Console.WriteLine(exception);
                Console.ForegroundColor = ConsoleColor.White;

                return OperationResult.Failed;
            }
            finally
            {
                lock (Lock)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                    {
                        StartTokenSource = null;
                        State = MemberState.Stopped;
                    }
                }
            }
        }




        /// <summary>
        /// If <see cref="ITransport"/> is started but idling - connects it to a remote host,
        /// using given <see cref="ConnectionArgs"/>, and provides a <paramref name="task"/> to await.
        /// </summary>
        /// <remarks>
        /// Additionally ensures to not throw a <see cref="MemberIsNotStartedException"/> - <see langword="false"/> if returned instead.
        /// </remarks>
        /// <param name="task">Task, returned by a <see cref="InvokeConnect(IReadOnlyConnectionArgs)"/> method.</param>
        /// <param name="args"><see cref="ConnectionArgs"/> to use for connecting this <see cref="ITransport"/> to a remote host.</param>
        /// <returns>
        /// <see langword="true"/> if it was possible to start connecting this <see cref="ITransport"/> and <paramref name="task"/> was provided for awaiting.
        /// <see langword="false"/> if <see cref="ITransport"/> is already disconnected.
        /// </returns>
        public bool TryInvokeConnect(out OperationResultTask task, IReadOnlyConnectionArgs args)
        {
            lock (Lock)
            {
                if (State == MemberState.Started_Idle)
                {
                    task = ConnectCoreUnlockedPreserved(args);
                    return true;
                }

                task = StateMachineHelpers.CompletedTask;
                return false;
            }
        }

        /// <summary>
        /// Reconnects this <see cref="ITransport"/> to a remote host using given <see cref="ConnectionArgs"/>.
        /// </summary>
        /// <param name="args"><see cref="ConnectionArgs"/> to use for connecting this <see cref="ITransport"/> to a remote host.</param>
        /// <returns>Connection operation to await.</returns>
        /// <exception cref="MemberIsNotStartedException">Member was not started before attempting a connection.</exception>
        public OperationResultTask InvokeConnect(IReadOnlyConnectionArgs args)
        {
            lock (Lock) return ConnectCoreUnlockedPreserved(args);
        }

        /// <inheritdoc cref="InvokeConnect"/>
        /// Note: Returned task IS preserved, as it is cached in <see cref="ConnectionOperation"/>.
        private OperationResultTask ConnectCoreUnlockedPreserved(IReadOnlyConnectionArgs args)
        {
            switch (State)
            {
                case MemberState.Stopped:
                case MemberState.Starting:
                case MemberState.Stopping: throw new MemberIsNotStartedException($"({GetType().Name}) is not started. Connection attempt is aborted.");

                case MemberState.Started_Idle:
                case MemberState.Started_Connecting:
                case MemberState.Started_Disconnecting:
                case MemberState.Started_Connected: break;

                default: throw new SwitchExpressionException(State);
            }

            CancellationTokenSource source = new();
            ConnectionTokenSource = source;
#if PRESERVE
            return ConnectionOperation = InvokeConnectInternal(DisconnectCoreUnlockedUnpreserved(), args, source).Preserve();
#else
            return ConnectionOperation = InvokeConnectInternal(DisconnectCoreUnlockedUnpreserved(), args, source);
#endif
        }

        private async OperationResultTask InvokeConnectInternal(OperationResultTask disconnection, IReadOnlyConnectionArgs args, CancellationTokenSource source)
        {
            try { await disconnection; } catch { /* Disconnection operation, of there is any, has to complete (finish, cancel) first. */ }

            lock (Lock)
            {
                if (source.IsCancellationRequested)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                        StartTokenSource = null;

                    return OperationResult.CancelledOrInvalid;
                }

                // Note: State should only change after await block, because the task above can mutate the state as well.
                State = MemberState.Started_Connecting;
            }

            OperationResult result = OperationResult.Failed;
            try
            {
                await Connect(args, source.Token);
                return result = OperationResult.Success;
            }
            catch (Exception exception)
            {
                // TODO: Standardize and import ServiceCoreLogger, and use it here.
                Console.ForegroundColor = ConsoleColor.Red;
                if (exception is SocketException se)
                    Console.WriteLine($"SocketException Code: {se.ErrorCode}");
                Console.WriteLine(exception);
                Console.ForegroundColor = ConsoleColor.White;

                try
                {
                    await Disconnect();
                }
                catch (Exception inner)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    if (exception is SocketException se2)
                        Console.WriteLine($"SocketException Code: {se2.ErrorCode}");
                    Console.WriteLine(inner);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                return OperationResult.Failed;
            }
            finally
            {
                lock (Lock)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                    {
                        StartTokenSource = null;
                        State = result == OperationResult.Success ? MemberState.Started_Connected : MemberState.Started_Idle;
                    }
                }
            }
        }




        /// <summary>
        /// If <see cref="ITransport"/> if currently connected or connecting - disconnects a this <see cref="ITransport"/>
        /// from a remote host, and provides a <paramref name="task"/> to await.
        /// </summary>
        /// <remarks>
        /// Additionally ensures to not throw a <see cref="MemberIsNotStartedException"/> - <see langword="false"/> if returned instead.
        /// </remarks>
        /// <param name="task">Task from <see cref="InvokeDisconnect"/> to await, or a completed task.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="ITransport"/> was connected, disconnection operation started and <paramref name="task"/> was provided for awaiting.
        /// <see langword="false"/> if <see cref="ITransport"/> was not connected nor connecting, and no operation was started.
        /// </returns>
        public bool TryInvokeDisconnect(out OperationResultTask task)
        {
            lock (Lock)
            {
                switch (State)
                {
                    case MemberState.Stopped:
                    case MemberState.Starting:
                    case MemberState.Stopping:
                    case MemberState.Started_Idle:
                    case MemberState.Started_Disconnecting: task = StateMachineHelpers.CompletedTask; return false;

                    case MemberState.Started_Connecting:
                    case MemberState.Started_Connected:

#if PRESERVE
                        task = DisconnectCoreUnlockedUnpreserved().Preserve();
#else
                        task = DisconnectCoreUnlockedUnpreserved();
#endif
                        return true;

                    default: throw new SwitchExpressionException(State);
                }
            }
        }

        /// <summary>
        /// Disconnects <see cref="ITransport"/> from a remote host.
        /// </summary>
        /// <returns>Disconnect operation to await.</returns>
        /// <exception cref="MemberIsNotStartedException">Member was not started before attempting a disconnection.</exception>
        public OperationResultTask InvokeDisconnect()
        {
#if PRESERVE
            lock (Lock) return DisconnectCoreUnlockedUnpreserved().Preserve();
#else
            lock (Lock) return DisconnectCoreUnlockedUnpreserved();
#endif
        }

        /// <inheritdoc cref="Disconnect"/>
        /// Note: Returned task is NOT preserved, as it is not cached anywhere.
        private OperationResultTask DisconnectCoreUnlockedUnpreserved()
        {
            switch (State)
            {
                case MemberState.Stopped:
                case MemberState.Starting:
                case MemberState.Stopping: // throw new MemberIsNotStartedException($"({GetType().Name}) is not started. Disconnection attempt is aborted.");

                case MemberState.Started_Idle: return StateMachineHelpers.CompletedTask;
                case MemberState.Started_Connecting:
                case MemberState.Started_Disconnecting:
                case MemberState.Started_Connected: break;

                default: throw new SwitchExpressionException(State);
            }

            OperationResultTask cancelledConnection;
            if (ConnectionTokenSource is null) cancelledConnection = StateMachineHelpers.CompletedTask;
            else
            {
                ConnectionTokenSource.Cancel();
                ConnectionTokenSource = null;
                (cancelledConnection, ConnectionOperation) = (ConnectionOperation, StateMachineHelpers.CompletedTask);
            }

            DisconnectionTokenSource?.Cancel();
            CancellationTokenSource source = new();
            DisconnectionTokenSource = source;
            return InvokeDisconnectInternal(cancelledConnection, source);
        }

        private async OperationResultTask InvokeDisconnectInternal(OperationResultTask cancelledConnection, CancellationTokenSource source)
        {
            try { await cancelledConnection; } catch { }

            lock (Lock)
            {
                if (source.IsCancellationRequested)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                        StartTokenSource = null;

                    return OperationResult.CancelledOrInvalid;
                }

                // Note: State should only change after await block, because the task above can mutate the state as well.
                State = MemberState.Started_Disconnecting;
            }

            try
            {
                await Disconnect();
                return OperationResult.Success;
            }
            catch (Exception exception)
            {
                // TODO: Standardize and import ServiceCoreLogger, and use it here.
                Console.ForegroundColor = ConsoleColor.Red;
                if (exception is SocketException se)
                    Console.WriteLine($"SocketException Code: {se.ErrorCode}");
                Console.WriteLine(exception);
                Console.ForegroundColor = ConsoleColor.White;
                return OperationResult.Failed;
            }
            finally
            {
                lock (Lock)
                {
                    source.Dispose();
                    if (ReferenceEquals(source, StartTokenSource)) // Identity check.
                    {
                        StartTokenSource = null;
                        State = MemberState.Started_Idle;
                    }
                }
            }
        }
    }
}
