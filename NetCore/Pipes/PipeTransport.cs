using NetCore.Loopback;
using System;
using System.Runtime.InteropServices;

namespace NetCore.Pipes
{
    /// <summary>
    /// Transport for C# Pipes.
    /// </summary>
    /// <remarks>
    /// Does not implement networking functionality at the moment.
    /// </remarks>
    public class PipeTransport : Transport, IUnreliableTransport, IReliableTransport
    {
        /// <inheritdoc/>
        public override bool HasCID(ulong CID)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SendReliable(ReadOnlySpan<byte> datagram)
        {
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendReliableExclusive(ReadOnlySpan<byte> datagram, ulong CIDToExclude)
        {
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendReliableExclusive)}(exclude: ({CIDToExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, ulong targetCID)
        {
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendReliableTo)}(target: ({targetCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, ulong sourceCID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleReliable)}(source: ({sourceCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendUnreliableExclusive(ReadOnlySpan<byte> datagram, ulong CIDToExclude)
        {
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendUnreliableExclusive)}(exclude: ({CIDToExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, ulong targetCID)
        {
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendUnreliableTo)}(target: ({targetCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
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
