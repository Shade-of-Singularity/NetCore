using System.Runtime.InteropServices;

namespace NetCore.Transports.Loopback
{
    /// <summary>
    /// Implements native loopback connection, without overhead of any in-between systems, like TCP or UDP.
    /// Useful if you rely on client sending messages to itself at development time.
    /// </summary>
    /// TODO: Replace with native routing if needed. Or continue support for testing of encoding/decoding methods.
    public class LoopbackTransport : Transport, IReliableTransport, IUnreliableTransport
    {
        private readonly record struct LoopbackEntry(LoopbackTransport Transport, ConnectionID RemoteCID)
        {
            /// <inheritdoc/>
            public override int GetHashCode() => Transport.GetHashCode();
        }


        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private readonly Dictionary<ConnectionID, LoopbackEntry> m_Loopbacks = [];




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Implementations
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public override AsyncTask Connect(IReadOnlyConnectionArgs args, CancellationToken token)
        {
            base.Connect(args, token);
            if (IsServerSide)
            {
                return AsyncTask.CompletedTask;
            }

            var ep = args.RemoteIPEndPoint;
            if (ep is null) return AsyncTask.CompletedTask;
            if (!Servers.TryGet(ep, out var server))
            {
                throw new KeyNotFoundException($"ClientData-side {nameof(LoopbackTransport)} cannot find an active server under a port ({ep.Port}).");
            }

            if (!server.TryGetReliableTransport(out LoopbackTransport? remote))
            {
                return AsyncTask.CompletedTask;
            }

            lock (m_Loopbacks)
            {
                lock (remote.m_Loopbacks)
                {
                    ConnectionID sourceID = Holder!.ConnectionIDProvider.NextCID();
                    ConnectionID remoteID = server.ConnectionIDProvider.NextCID();
                    remote.m_Loopbacks[remoteID] = new(this, sourceID);
                    m_Loopbacks[sourceID] = new(remote, remoteID);
                }
            }

            return AsyncTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override AsyncTask Disconnect()
        {
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    var remoteLoopbacks = entry.Value.Transport.m_Loopbacks;
                    lock (remoteLoopbacks)
                    {
                        remoteLoopbacks.Remove(entry.Key);
                    }
                }

                m_Loopbacks.Clear();
            }

            return base.Disconnect();
        }

        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection)
        {
            lock (m_Loopbacks)
            {
                return m_Loopbacks.ContainsKey(connection);
            }
        }

        /// <inheritdoc/>
        public void SendReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    entry.Value.Transport.HandleReliable(datagram, header, flags, entry.Value.RemoteCID);
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    if (entry.Key != toExclude)
                    {
                        entry.Value.Transport.HandleReliable(datagram, header, flags, entry.Value.RemoteCID);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                if (m_Loopbacks.TryGetValue(target, out LoopbackEntry entry))
                {
                    entry.Transport.HandleReliable(datagram, header, flags, target);
                }
            }
        }

        /// <inheritdoc/>
        public void HandleReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleReliable)}(sourceID: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    entry.Value.Transport.HandleUnreliable(datagram, header, flags, entry.Value.RemoteCID);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    if (entry.Key != toExclude)
                    {
                        entry.Value.Transport.HandleUnreliable(datagram, header, flags, entry.Value.RemoteCID);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                if (m_Loopbacks.TryGetValue(target, out LoopbackEntry entry))
                {
                    entry.Transport.HandleUnreliable(datagram, header, flags, target);
                }
            }
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleUnreliable)}(sourceID: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendReliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }
    }
}
