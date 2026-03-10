using NetCore.Loopback;
using System;
using System.Runtime.InteropServices;

namespace NetCore.UDP
{
    /// <summary>
    /// Transport for UDP messages.
    /// </summary>
    /// <remarks>
    /// Does not implement networking functionality at the moment.
    /// </remarks>
    public class UDPTransport : Transport, IUnreliableTransport
    {
        /// <inheritdoc/>
        public override bool HasCID(ulong CID)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendUnreliableExclusive(ReadOnlySpan<byte> datagram, ulong CIDToExclude)
        {
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableExclusive)}(exclude: ({CIDToExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, ulong targetCID)
        {
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableTo)}(target: ({targetCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void HandleUnreliable(ReadOnlySpan<byte> datagram, ulong sourceCID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleUnreliable)}(source: ({sourceCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
