using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCore
{
    // TODO: Implement dispatcher at some point.
    // TODO! Fix a bug where due to chaining, disconnect is being called on a Start call, throwing an exception.
    public partial class NetworkMember // TODO: Provide non-provider-based methods for args handling.
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                            Protected Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Current raw <see cref="NetworkMember"/> state, retrieved outside of a <see cref="_lock"/>.
        /// </summary>
        protected MemberState StateUnlocked => m_State;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private CancellationTokenSource? m_StartTokenSource;
        private UniTask<OperationResult> m_StartOperation = StateMachineHelpers.CompletedTask;
        private CancellationTokenSource? m_StopTokenSource;
        private CancellationTokenSource? m_ConnectionTokenSource;
        private UniTask<OperationResult> m_ConnectionTask = StateMachineHelpers.CompletedTask;
        private CancellationTokenSource? m_DisconnectionTokenSource;
        private volatile MemberState m_State;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks whether current member state allows sending messages.
        /// </summary>
        /// <seealso cref="MemberStateExtensions.AllowsSendingMessages(MemberState)"/>
        public bool AllowsSendingMessages()
        {
            lock (_lock) return m_State.AllowsSendingMessages();
        }

        /// <remarks>
        /// Retrieve value outside of a <see langword="lock"/>.
        /// </remarks>
        /// <inheritdoc cref="AllowsSendingMessages"/>
        /// <seealso cref="MemberStateExtensions.AllowsSendingMessages(MemberState)"/>
        protected bool AllowsSendingMessagesCore() => m_State.AllowsSendingMessages();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .  State machine (v2) operations cover: Start, Stop, Connect, Disconnect methods and supplementary methods.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// If <see cref="NetworkMember"/> is currently stopped - starts it using <see cref="NetCore.StartupArgs"/>
        /// provided by <paramref name="provider"/>, and provides a <paramref name="task"/> to await.
        /// </summary>
        /// <param name="task">Task from <see cref="Start"/> to await, or a completed task.</param>
        /// <param name="provider">Provider for <see cref="NetCore.StartupArgs"/>. Can be <see langword="null"/>.</param>
        /// <param name="clear">
        /// Whether to clear <see cref="NetCore.StartupArgs"/> from previous session before providing them to <paramref name="provider"/> or not.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if it was possible to start this <see cref="NetworkMember"/> and <paramref name="task"/> was provided for awaiting.
        /// <see langword="false"/> if <see cref="NetworkMember"/> was already started.
        /// </returns>
        public bool TryStart(out UniTask<OperationResult> task, StartupArgsProvider? provider, bool clear = true)
        {
            lock (_lock)
            {
                if (m_State == MemberState.Stopped)
                {
                    task = StartCoreUnlockedPreserved(provider, clear);
                    return true;
                }

                task = StateMachineHelpers.CompletedTask;
                return false;
            }
        }

        /// <summary>
        /// Restarts <see cref="NetworkMember"/> using <see cref="NetCore.StartupArgs"/> from a previous session.
        /// </summary>
        /// <returns>Task from <see cref="Start(StartupArgsProvider?, bool)"/> method to await.</returns>
        public UniTask<OperationResult> Restart()
        {
            lock (_lock) return StartCoreUnlockedPreserved(null, clear: false);
        }

        /// <summary>
        /// Stops <see cref="NetworkMember"/> if it currently runs, and start it again with <see cref="NetCore.StartupArgs"/> provided by <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">Provider for <see cref="NetCore.StartupArgs"/>. Can be <see langword="null"/>.</param>
        /// <param name="clear">
        /// Whether to clear <see cref="NetCore.StartupArgs"/> from previous session before providing them to <paramref name="provider"/> or not.
        /// </param>
        /// <returns>Startup task to await.</returns>
        public UniTask<OperationResult> Start(StartupArgsProvider? provider, bool clear = true)
        {
            lock (_lock) return StartCoreUnlockedPreserved(provider, clear);
        }

        /// <inheritdoc cref="Start"/>
        /// Note: Returned task IS preserved, as it is cached in <see cref="m_StartOperation"/>.
        UniTask<OperationResult> StartCoreUnlockedPreserved(StartupArgsProvider? provider, bool clear)
        {
            return m_StartOperation = InvokeStartInternal(StopCoreUnlockedUnpreserved(), provider, clear, m_StartTokenSource = new()).Preserve();
        }

        async UniTask<OperationResult> InvokeStartInternal(UniTask<OperationResult> stopping, StartupArgsProvider? provider, bool clear, CancellationTokenSource source)
        {
            try { await stopping; } catch { }

            StartupArgs args;
            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_StartTokenSource))
                    return OperationResult.CancelledOrInvalid;

                // Note: State should only change after await block, because the task above can mutate the state as well.
                m_State = MemberState.Starting;
                args = m_StartupArgs;
                if (clear) args.Clear();
                provider?.Invoke(args);
            }

            OperationResult result = OperationResult.Failed;
            try
            {
                return result = await StartOperation(args, source.Token);
            }
            catch (Exception exception)
            {
                LogException(exception); // TODO: Standardize and import ServiceCoreLogger, and use it here.
                return OperationResult.Failed;
            }
            finally
            {
                lock (_lock)
                {
                    if (IdentifyTokenSourceWithDisposal(source, ref m_StartTokenSource)) // Identity check.
                    {
                        m_State = result == OperationResult.Success ? MemberState.Started_Idle : MemberState.Stopped;
                    }
                }
            }
        }




        /// <summary>
        /// Stops <see cref="NetworkMember"/> if it is currently running.
        /// </summary>
        /// <returns>Stop task to await a completion of.</returns>
        public UniTask<OperationResult> Stop()
        {
            lock (_lock) return StopCoreUnlockedUnpreserved().Preserve();
        }

        /// <inheritdoc cref="Stop"/>
        /// Note: Returned task is NOT preserved, as it is not cached anywhere.
        UniTask<OperationResult> StopCoreUnlockedUnpreserved()
        {
            UniTask<OperationResult> cancelledStarting;
            if (m_StartTokenSource is null) cancelledStarting = StateMachineHelpers.CompletedTask;
            else
            {
                m_StartTokenSource.Cancel();
                m_StartTokenSource = null;
                (cancelledStarting, m_StartOperation) = (m_StartOperation, StateMachineHelpers.CompletedTask);
            }

            m_StopTokenSource?.Cancel();
            m_StopTokenSource = new();
            return InvokeStopInternal(DisconnectCoreUnlockedUnpreserved(), cancelledStarting, m_StopTokenSource);
        }

        async UniTask<OperationResult> InvokeStopInternal(UniTask<OperationResult> disconnecting, UniTask<OperationResult> cancelledStarting, CancellationTokenSource source)
        {
            try { await disconnecting; } catch { }
            try { await cancelledStarting; } catch { }
            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_StopTokenSource))
                    return OperationResult.CancelledOrInvalid;

                // Note: State should only change after await block, because tasks above can mutate the state as well.
                m_State = MemberState.Stopping;
            }

            try
            {
                return await StopOperation();
            }
            catch (Exception exception)
            {
                LogException(exception); // TODO: Standardize and import ServiceCoreLogger, and use it here.
                return OperationResult.Failed;
            }
            finally
            {
                lock (_lock)
                {
                    if (IdentifyTokenSourceWithDisposal(source, ref m_StopTokenSource))
                        m_State = MemberState.Stopped;
                }
            }
        }




        /// <summary>
        /// If <see cref="NetworkMember"/> is started but idling - connects it to a remote host, using <see cref="NetCore.ConnectionArgs"/>
        /// provided by <paramref name="provider"/>, and provides a <paramref name="task"/> to await.
        /// </summary>
        /// <remarks>
        /// Additionally ensures to not throw a <see cref="MemberIsNotStartedException"/> - <see langword="false"/> if returned instead.
        /// </remarks>
        /// <param name="task">Task, returned by a <see cref="Connect(ConnectionArgsProvider?, bool)"/> method.</param>
        /// <param name="provider">Provider for <see cref="NetCore.ConnectionArgs"/>. Can be <see langword="null"/>.</param>
        /// <param name="clear">
        /// Whether to clear <see cref="NetCore.ConnectionArgs"/> from previous session before providing them to <paramref name="provider"/> or not.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if it was possible to start connecting this <see cref="NetworkMember"/> and <paramref name="task"/> was provided for awaiting.
        /// <see langword="false"/> if <see cref="NetworkMember"/> is already disconnected.
        /// </returns>
        public bool TryConnect(out UniTask<OperationResult> task, ConnectionArgsProvider? provider, bool clear = true)
        {
            lock (_lock)
            {
                if (m_State == MemberState.Started_Idle)
                {
                    task = ConnectCoreUnlockedPreserved(provider, clear);
                    return true;
                }

                task = StateMachineHelpers.CompletedTask;
                return false;
            }
        }

        /// <summary>
        /// If <see cref="NetworkMember"/> is started - reconnects it to a remote host, using <see cref="NetCore.ConnectionArgs"/>
        /// from a previous session, and provides a <paramref name="task"/> to await.
        /// </summary>
        /// <remarks>
        /// Additionally ensures to not throw a <see cref="MemberIsNotStartedException"/> - <see langword="false"/> if returned instead.
        /// </remarks>
        /// <param name="task">Task from <see cref="Connect"/> to await, or a completed task.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="NetworkMember"/> is restarted and <paramref name="task"/> was provided for awaiting.
        /// <see langword="false"/> if <see cref="NetworkMember"/> is stopped.
        /// </returns>
        public bool TryReconnect(out UniTask<OperationResult> task)
        {
            lock (_lock)
            {
                switch (m_State)
                {
                    case MemberState.Stopped:
                    case MemberState.Starting:
                    case MemberState.Stopping: task = StateMachineHelpers.CompletedTask; return false;

                    case MemberState.Started_Idle:
                    case MemberState.Started_Connecting:
                    case MemberState.Started_Disconnecting:
                    case MemberState.Started_Connected: task = ConnectCoreUnlockedPreserved(null, clear: false); return true;

                    default: throw new SwitchExpressionException(m_State);
                }
            }
        }

        /// <summary>
        /// Reconnects this <see cref="NetworkMember"/> to a remote host using <see cref="NetCore.ConnectionArgs"/> from a previous session.
        /// </summary>
        /// <returns>Connection operation to await.</returns>
        /// <exception cref="MemberIsNotStartedException">Member was not started before attempting a connection.</exception>
        public UniTask<OperationResult> Reconnect()
        {
            lock (_lock) return ConnectCoreUnlockedPreserved(null, clear: false);
        }

        /// <summary>
        /// Reconnects this <see cref="NetworkMember"/> to a remote host using <see cref="NetCore.ConnectionArgs"/> provided by <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">Provider for <see cref="NetCore.ConnectionArgs"/>. Can be <see langword="null"/>.</param>
        /// <param name="clear">
        /// Whether to clear <see cref="NetCore.ConnectionArgs"/> from previous session before providing them to <paramref name="provider"/> or not.
        /// </param>
        /// <returns>Connection operation to await.</returns>
        /// <exception cref="MemberIsNotStartedException">Member was not started before attempting a connection.</exception>
        public UniTask<OperationResult> Connect(ConnectionArgsProvider? provider, bool clear = true)
        {
            lock (_lock) return ConnectCoreUnlockedPreserved(provider, clear);
        }

        /// <inheritdoc cref="Connect"/>
        /// Note: Returned task IS preserved, as it is cached in <see cref="m_ConnectionTask"/>.
        UniTask<OperationResult> ConnectCoreUnlockedPreserved(ConnectionArgsProvider? provider, bool clear)
        {
            switch (m_State)
            {
                case MemberState.Stopped:
                case MemberState.Starting:
                case MemberState.Stopping: throw new MemberIsNotStartedException($"({GetType().Name}) is not started. Connection attempt is aborted.");

                case MemberState.Started_Idle:
                case MemberState.Started_Connecting:
                case MemberState.Started_Disconnecting:
                case MemberState.Started_Connected: break;

                default: throw new SwitchExpressionException(m_State);
            }

            m_ConnectionTokenSource = new();
            return m_ConnectionTask = InvokeConnectInternal(DisconnectCoreUnlockedUnpreserved(), provider, clear, m_ConnectionTokenSource).Preserve();
        }

        async UniTask<OperationResult> InvokeConnectInternal(
            UniTask<OperationResult> disconnection, ConnectionArgsProvider? provider, bool clear, CancellationTokenSource source)
        {
            try { await disconnection; } catch { }

            ConnectionArgs args;
            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_ConnectionTokenSource))
                    return OperationResult.CancelledOrInvalid;

                // Note: State should only change after await block, because the task above can mutate the state as well.
                m_State = MemberState.Started_Connecting;
                args = m_ConnectionArgs;
                if (clear) args.Clear();
                provider?.Invoke(args);
            }

            OperationResult result = OperationResult.Failed;
            try
            {
                return result = await ConnectOperation(args, source.Token);
            }
            catch (Exception exception)
            {
                LogException(exception); // TODO: Standardize and import ServiceCoreLogger, and use it here.
                return OperationResult.Failed;
            }
            finally
            {
                lock (_lock)
                {
                    if (IdentifyTokenSourceWithDisposal(source, identity: ref m_ConnectionTokenSource)) // Identity check.
                    {
                        m_State = result == OperationResult.Success ? MemberState.Started_Connected : MemberState.Started_Idle;
                    }
                }
            }
        }




        /// <summary>
        /// If <see cref="NetworkMember"/> if currently connected or connecting - disconnects a this <see cref="NetworkMember"/>
        /// from a remote host, and provides a <paramref name="task"/> to await.
        /// </summary>
        /// <remarks>
        /// Additionally ensures to not throw a <see cref="MemberIsNotStartedException"/> - <see langword="false"/> if returned instead.
        /// </remarks>
        /// <param name="task">Task from <see cref="Disconnect"/> to await, or a completed task.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="NetworkMember"/> was connected, disconnection operation started and <paramref name="task"/> was provided for awaiting.
        /// <see langword="false"/> if <see cref="NetworkMember"/> was not connected nor connecting, and no operation was started.
        /// </returns>
        public bool TryDisconnect(out UniTask<OperationResult> task)
        {
            lock (_lock)
            {
                switch (m_State)
                {
                    case MemberState.Stopped:
                    case MemberState.Starting:
                    case MemberState.Stopping:
                    case MemberState.Started_Idle:
                    case MemberState.Started_Disconnecting: task = StateMachineHelpers.CompletedTask; return false;

                    case MemberState.Started_Connecting:
                    case MemberState.Started_Connected: task = DisconnectCoreUnlockedUnpreserved().Preserve(); return true;

                    default: throw new SwitchExpressionException(m_State);
                }
            }
        }

        /// <summary>
        /// Disconnects <see cref="NetworkMember"/> from a remote host.
        /// </summary>
        /// <returns>Disconnect operation to await.</returns>
        /// <exception cref="MemberIsNotStartedException">Member was not started before attempting a disconnection.</exception>
        public UniTask<OperationResult> Disconnect()
        {
            lock (_lock) return DisconnectCoreUnlockedUnpreserved().Preserve();
        }

        /// <inheritdoc cref="Disconnect"/>
        /// Note: Returned task is NOT preserved, as it is not cached anywhere.
        UniTask<OperationResult> DisconnectCoreUnlockedUnpreserved()
        {
            switch (m_State)
            {
                case MemberState.Stopped:
                case MemberState.Starting:
                case MemberState.Stopping: throw new MemberIsNotStartedException($"({GetType().Name}) is not started. Disconnection attempt is aborted.");

                case MemberState.Started_Idle: return StateMachineHelpers.CompletedTask;
                case MemberState.Started_Connecting:
                case MemberState.Started_Disconnecting:
                case MemberState.Started_Connected: break;

                default: throw new SwitchExpressionException(m_State);
            }

            UniTask<OperationResult> cancelledConnection;
            if (m_ConnectionTokenSource is null) cancelledConnection = StateMachineHelpers.CompletedTask;
            else
            {
                m_ConnectionTokenSource.Cancel();
                m_ConnectionTokenSource = null;
                (cancelledConnection, m_ConnectionTask) = (m_ConnectionTask, StateMachineHelpers.CompletedTask);
            }

            m_DisconnectionTokenSource?.Cancel();
            m_DisconnectionTokenSource = new();
            return InvokeDisconnectInternal(cancelledConnection, m_DisconnectionTokenSource);
        }

        async UniTask<OperationResult> InvokeDisconnectInternal(UniTask<OperationResult> cancelledConnection, CancellationTokenSource source)
        {
            try { await cancelledConnection; } catch { }

            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_DisconnectionTokenSource))
                    return OperationResult.CancelledOrInvalid;

                // Note: State should only change after await block, because the task above can mutate the state as well.
                m_State = MemberState.Started_Disconnecting;
            }

            try
            {
                return await DisconnectOperation();
            }
            catch (Exception exception)
            {
                LogException(exception); // TODO: Standardize and import ServiceCoreLogger, and use it here.
                return OperationResult.Failed;
            }
            finally
            {
                lock (_lock)
                {
                    if (IdentifyTokenSourceWithDisposal(source, ref m_DisconnectionTokenSource))
                        m_State = MemberState.Started_Idle;
                }
            }
        }




        /// <summary>
        /// Resets <paramref name="identity"/> to <see langword="null"/> if <paramref name="target"/> is disposed.
        /// </summary>
        /// <remarks>
        /// Requires caller to use it inside a <see cref="_lock"/> block.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if <paramref name="target"/> was cancelled (now disposed), and <paramref name="identity"/> was set to <see langword="null"/>.
        /// <see langword="false"/> if <paramref name="target"/> was not cancelled and nothing was made.
        /// </returns>
        private static bool TryDisposeIfCancelledUnlocked(CancellationTokenSource target, ref CancellationTokenSource? identity)
        {
            if (!target.IsCancellationRequested)
            {
                return false;
            }

            target.Dispose();
            if (ReferenceEquals(identity, target))
                identity = null;

            return true;
        }

        /// <summary>
        /// Disposes <paramref name="target"/> and resets <paramref name="identity"/> to <see langword="null"/> if their references/identities match.
        /// </summary>
        /// <remarks>
        /// Requires caller to use it inside a <see cref="_lock"/> block.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if identities match, <paramref name="target"/> was disposed and <paramref name="identity"/> was set to <see langword="null"/>.
        /// <see langword="false"/> if identities doesn't match, <paramref name="target"/> was disposed but <paramref name="identity"/> was not affected.
        /// </returns>
        private static bool IdentifyTokenSourceWithDisposal(CancellationTokenSource target, ref CancellationTokenSource? identity)
        {
            target.Dispose();
            if (ReferenceEquals(target, identity))
            {
                identity = null;
                return true;
            }

            return false;
        }
    }
}
