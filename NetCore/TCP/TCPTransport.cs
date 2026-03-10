using NetCore.Loopback;
using System;
using System.Runtime.InteropServices;

namespace NetCore.TCP
{
    /// <summary>
    /// Transport for TCP messages.
    /// </summary>
    /// <remarks>
    /// Does not implement networking functionality at the moment.
    /// </remarks>
    public class TCPTransport : Transport, IReliableTransport
    {
        /// <inheritdoc/>
        public override bool HasCID(ulong CID)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SendReliable(ReadOnlySpan<byte> datagram)
        {
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendReliableExclusive(ReadOnlySpan<byte> datagram, ulong CIDToExclude)
        {
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliableExclusive)}(exclude: ({CIDToExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, ulong targetCID)
        {
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliableTo)}(target: ({targetCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, ulong sourceCID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleReliable)}(source: ({sourceCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
