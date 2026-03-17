using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
    /// <summary>
    /// Encodes/Decodes a set of flags using a quad map.
    /// </summary>
    internal static class QuadFlagsEncoding
    {
        /// <param name="flags">Flags to encode.</param>
        /// <param name="bits">Amount of bits to process.</param>
        /// TODO: Add startBit
        public static Encoder GetEncode(ReadOnlySpan<byte> flags, ushort bits)
        {
            Span<ushort> layers = stackalloc ushort[sizeof(byte) * 8];
            ushort targets = bits;
            ushort depth = 0;
            do
            {
                layers[depth++] = targets;
                targets >>= 2; // 2 - quad-map capacity.
            }
            while (targets > 0);
            //ulong higher = 0, lower = 0;
            // TODO: Encode.
            return new Encoder(flags, depth);
        }

        // Note: encoder is not an option.
        // Having branching on each flag is horrible.
        public ref struct Encoder(ReadOnlySpan<byte> flags, ushort depth)
        {
            public readonly ReadOnlySpan<byte> flags = flags;
            public readonly ushort depth = depth;
            private IEnumerator<bool>? enumerator;
            private int iterator = 0;
            public readonly byte Current => throw new NotImplementedException();
            public bool MoveNext()
            {
                throw new NotImplementedException();
            }
        }

        //[StructLayout(LayoutKind.Explicit)]
        //public ref struct Encoder(ulong higher, ulong lower, ushort depth)
        //{
        //    [FieldOffset(0)] public readonly ulong higher = higher;
        //    [FieldOffset(8)] public readonly ulong lower = lower;
        //    [FieldOffset(16)] public readonly ulong toggles;
        //    [FieldOffset(16)] public byte d1 = (byte)(depth >= 1 ? 0 : 0b11);
        //    [FieldOffset(17)] public byte d2 = (byte)(depth >= 2 ? 0 : 0b11);
        //    [FieldOffset(18)] public byte d3 = (byte)(depth >= 3 ? 0 : 0b11);
        //    [FieldOffset(19)] public byte d4 = (byte)(depth >= 4 ? 0 : 0b11);
        //    [FieldOffset(20)] public byte d5 = (byte)(depth >= 5 ? 0 : 0b11);
        //    [FieldOffset(21)] public byte d6 = (byte)(depth >= 6 ? 0 : 0b11);
        //    [FieldOffset(22)] public byte d7 = (byte)(depth >= 7 ? 0 : 0b11);
        //    [FieldOffset(23)] public byte d8 = (byte)(depth >= 8 ? 0 : 0b11);
        //    public byte Current =>
        //    public bool MoveNext
        //}
    }
}
