using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCore
{
    public partial class NetworkMember
    {
        enum MemberState : byte
        {
            Stopped,
            Starting,
            Stopping,
            Started_Idle,
            Started_Connecting,
            Started_Disconnecting,
            Started_Connected,
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        static readonly UniTask<OperationResult> CompletedTask = UniTask.FromResult(OperationResult.CancelledOrInvalid);
        private volatile MemberState m_State;
        private CancellationTokenSource? m_StartTokenSource;
        private UniTask<OperationResult> m_StartOperation = CompletedTask;
        private UniTask<OperationResult> m_StopOperation = CompletedTask;
        private CancellationTokenSource? m_ConnectionTokenSource;
        private UniTask<OperationResult> m_ConnectionTask = CompletedTask;
        private UniTask<OperationResult> m_DisconnectionTask = CompletedTask;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .  State machine (v2) operations cover: Start, Stop, Connect, Disconnect methods and supplementary methods.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public bool TryStart(out UniTask<OperationResult> task, StartupArgsProvider? provider, bool clear = true)
        {
            lock (_lock)
            {
                if (m_State == MemberState.Stopped)
                {
                    task = StartCoreUnlocked(provider, clear);
                    return true;
                }

                task = CompletedTask;
                return false;
            }
        }

        public UniTask<OperationResult> Restart()
        {
            lock (_lock) return StartCoreUnlocked(null, clear: false);
        }

        public UniTask<OperationResult> Start(StartupArgsProvider? provider, bool clear = true)
        {
            lock (_lock) return StartCoreUnlocked(provider, clear);
        }

        /// <inheritdoc cref="Start"/>
        UniTask<OperationResult> StartCoreUnlocked(StartupArgsProvider? provider, bool clear)
        {
            UniTask<OperationResult> stopping = StopCoreUnlocked();
            m_StartTokenSource = new();
            return m_StartOperation = Create((this, stopping, provider, clear), InvokeStartInternal, m_StartTokenSource);
        }

        // TODO: Consider removing indirection, since we hit an await block anyway before we modify a current m_State.
        static async UniTask<OperationResult> InvokeStartInternal(
            (NetworkMember member, UniTask<OperationResult> stopping, StartupArgsProvider? provider, bool clear) v, CancellationTokenSource source)
        {
            return await v.member.InvokeStartInternal(v.stopping, v.provider, v.clear, source);
        }
        async UniTask<OperationResult> InvokeStartInternal(UniTask<OperationResult> stopping, StartupArgsProvider? provider, bool clear, CancellationTokenSource source)
        {
            if (Settings.UseConcurrentProtections)
            {
                await UniTask.Yield();
            }

            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_StartTokenSource))
                    return OperationResult.CancelledOrInvalid;
            }

            try { await stopping; } catch { }

            StartupArgs args;
            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_StartTokenSource))
                    return OperationResult.CancelledOrInvalid;

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




        public UniTask<OperationResult> Stop()
        {
            lock (_lock) return StopCoreUnlocked();
        }

        /// <inheritdoc cref="Stop"/>
        UniTask<OperationResult> StopCoreUnlocked()
        {
            UniTask<OperationResult> cancelledStarting;
            UniTask<OperationResult> disconnecting;
            switch (m_State)
            {
                case MemberState.Stopped: return CompletedTask;
                case MemberState.Stopping: return m_StopOperation;

                case MemberState.Starting:
                    if (m_StartTokenSource is null)
                    {
                        throw new NetworkMemberStateException($"Member is marked starting, yet no start task was actually started. This is a system bug.");
                    }

                    m_StartTokenSource.Cancel();
                    m_StartTokenSource = null;
                    (cancelledStarting, m_StartOperation) = (m_StartOperation, CompletedTask);
                    break;

                case MemberState.Started_Idle:
                case MemberState.Started_Connecting:
                case MemberState.Started_Disconnecting:
                case MemberState.Started_Connected: cancelledStarting = CompletedTask; break;

                default: throw new SwitchExpressionException(m_State);
            }

            disconnecting = DisconnectCoreUnlocked();

            return m_StopOperation = Create((this, disconnecting, cancelledStarting), InvokeStopInternal).Preserve();
        }

        // TODO: Consider removing indirection, since we hit an await block anyway before we modify a current m_State.
        static async UniTask<OperationResult> InvokeStopInternal((NetworkMember member, UniTask<OperationResult> disconnecting, UniTask<OperationResult> cancelledStarting) v)
        {
            return await v.member.InvokeStopInternal(v.disconnecting, v.cancelledStarting);
        }
        async UniTask<OperationResult> InvokeStopInternal(UniTask<OperationResult> disconnecting, UniTask<OperationResult> cancelledStarting)
        {
            // TODO!!! Make sure that status changes only on actual await.
            try { await disconnecting; } catch { }
            try { await cancelledStarting; } catch { }
            m_State = MemberState.Stopping;

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
                // Note: if all tasks await for stop/disconnect to complete, can this state even override other states invalidly?
                m_State = MemberState.Stopped;
            }
        }




        /// Note: Also ensures to not throw any exceptions.
        public bool TryConnect(out UniTask<OperationResult> task, ConnectionArgsProvider? provider, bool clear = true)
        {
            lock (_lock)
            {
                if (m_State == MemberState.Started_Idle)
                {
                    task = ConnectCoreUnlocked(provider, clear);
                    return true;
                }

                task = CompletedTask;
                return false;
            }
        }

        public UniTask<OperationResult> Reconnect()
        {
            lock (_lock) return ConnectCoreUnlocked(null, clear: true);
        }

        public UniTask<OperationResult> Connect(ConnectionArgsProvider? provider, bool clear = true)
        {
            lock (_lock) return ConnectCoreUnlocked(provider, clear);
        }

        /// <inheritdoc cref="Connect"/>
        UniTask<OperationResult> ConnectCoreUnlocked(ConnectionArgsProvider? provider, bool clear)
        {
            switch (m_State)
            {
                case MemberState.Stopped:
                case MemberState.Starting:
                case MemberState.Stopping: throw new NetworkMemberNotStartedException($"({GetType().Name}) is not started. Connection attempt is aborted.");

                case MemberState.Started_Idle:
                case MemberState.Started_Connecting:
                case MemberState.Started_Disconnecting:
                case MemberState.Started_Connected: break;

                default: throw new SwitchExpressionException(m_State);
            }

            UniTask<OperationResult> disconnection = DisconnectCoreUnlocked();
            m_ConnectionTokenSource = new();
            return m_ConnectionTask = Create((this, disconnection, provider, clear), InvokeConnectInternal, m_ConnectionTokenSource);
        }

        // TODO: Consider removing indirection, since we hit an await block anyway before we modify a current m_State.
        static async UniTask<OperationResult> InvokeConnectInternal(
            (NetworkMember member, UniTask<OperationResult> disconnection, ConnectionArgsProvider? provider, bool clear) v, CancellationTokenSource source)
        {
            return await v.member.InvokeConnectInternal(v.disconnection, v.provider, v.clear, source);
        }

        async UniTask<OperationResult> InvokeConnectInternal(
            UniTask<OperationResult> disconnection, ConnectionArgsProvider? provider, bool clear, CancellationTokenSource source)
        {
            if (Settings.UseConcurrentProtections)
            {
                await UniTask.Yield();
            }

            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_ConnectionTokenSource))
                    return OperationResult.CancelledOrInvalid;
            }

            try { await disconnection; } catch { }

            ConnectionArgs args;
            lock (_lock)
            {
                if (TryDisposeIfCancelledUnlocked(source, identity: ref m_ConnectionTokenSource))
                    return OperationResult.CancelledOrInvalid;

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

        public UniTask<OperationResult> Disconnect()
        {
            lock (_lock) return DisconnectCoreUnlocked();
        }

        /// <inheritdoc cref="Disconnect"/>
        UniTask<OperationResult> DisconnectCoreUnlocked()
        {
            UniTask<OperationResult> cancelledConnection;
            switch (m_State)
            {
                case MemberState.Stopped:
                case MemberState.Starting:
                case MemberState.Stopping:
                case MemberState.Started_Idle: return CompletedTask;
                case MemberState.Started_Disconnecting: return m_DisconnectionTask;

                case MemberState.Started_Connecting:
                    if (m_ConnectionTokenSource is null)
                    {
                        throw new NetworkMemberStateException($"Member is marked as connecting, but no connection operation is running. This is a system bug.");
                    }

                    m_ConnectionTokenSource.Cancel();
                    m_ConnectionTokenSource = null;
                    (cancelledConnection, m_ConnectionTask) = (m_ConnectionTask, CompletedTask);
                    break;

                case MemberState.Started_Connected: cancelledConnection = CompletedTask; break;

                default: throw new SwitchExpressionException(m_State);
            }

            return m_DisconnectionTask = Create((this, cancelledConnection), InvokeDisconnectInternal).Preserve();
        }

        // TODO: Consider removing indirection, since we hit an await block anyway before we modify a current m_State.
        static async UniTask<OperationResult> InvokeDisconnectInternal((NetworkMember member, UniTask<OperationResult> cancelledConnection) v)
        {
            return await v.member.InvokeDisconnectInternal(v.cancelledConnection);
        }
        async UniTask<OperationResult> InvokeDisconnectInternal(UniTask<OperationResult> cancelledConnection)
        {
            // TODO!!! Make sure that status changes only on actual await.
            try { await cancelledConnection; } catch { }
            m_State = MemberState.Started_Disconnecting;

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
                // Note: if all tasks await for stop/disconnect to complete, can this state even override other states invalidly?
                m_State = MemberState.Started_Idle;
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
