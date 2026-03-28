using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCore
{
    /// <summary>
    /// Common class for working with headers, and <see cref="Header"/> and <see cref="Header"/> structs.
    /// </summary>
    /// TODO: Maybe pack a header version as well? So we can modify a reading mode based on a version.
    /// TODO: Consider using BinaryPrimitives.ReadUInt32LittleEndian(span) or similar things.
    /// TODO: Use <see cref="Unsafe"/> instead of <see cref="MemoryMarshal"/> for consistency.
    public static class HeaderHelpers
    {
        /// <summary>
        /// Increments amount of locks on a given <paramref name="header"/> instance.
        /// </summary>
        /// <returns>
        /// The <paramref name="header"/> instance itself for easier inlining.
        /// </returns>
        public static ref Header Lock(this ref Header header)
        {
            header.IncrementLocks();
            return ref header;
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        /// <remarks>
        /// Encodes it as a single bit.
        /// </remarks>
        public static void Set<T>(this in Header header, bool value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            span[0] = (byte)(value ? 1 : 0);
            header.Set<T>(span);
        }

        #region Set(value) extensions
        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, byte value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            span[0] = value;
            header.Set<T>(span);
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, sbyte value) where T : CustomHeader<T>, new()
        {
            Span<sbyte> span = stackalloc sbyte[1];
            span[0] = value;
            header.Set<T>(MemoryMarshal.AsBytes(span));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, short value) where T : CustomHeader<T>, new()
        {
            Span<short> span = stackalloc short[1];
            span[0] = value;
            header.Set<T>(MemoryMarshal.AsBytes(span));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, ushort value) where T : CustomHeader<T>, new()
        {
            Span<ushort> span = stackalloc ushort[1];
            span[0] = value;
            header.Set<T>(MemoryMarshal.AsBytes(span));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, int value) where T : CustomHeader<T>, new()
        {
            Span<int> span = stackalloc int[1];
            span[0] = value;
            header.Set<T>(MemoryMarshal.AsBytes(span));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, uint value) where T : CustomHeader<T>, new()
        {
            Span<uint> span = stackalloc uint[1];
            span[0] = value;
            header.Set<T>(MemoryMarshal.AsBytes(span));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, long value) where T : CustomHeader<T>, new()
        {
            Span<long> span = stackalloc long[1];
            span[0] = value;
            header.Set<T>(MemoryMarshal.AsBytes(span));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        public static void Set<T>(this in Header header, ulong value) where T : CustomHeader<T>, new()
        {
            Span<ulong> span = stackalloc ulong[1];
            span[0] = value;
            header.Set<T>(MemoryMarshal.AsBytes(span));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as content under the current <see cref="CustomHeader{T}"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="CustomHeader{T}"/> type to reference.</typeparam>
        /// <typeparam name="TValue">Enum to set.</typeparam>
        public static void Set<T, TValue>(this in Header header, TValue value)
            where T : CustomHeader<T>, new()
            where TValue : Enum
        {
            // Only one stackalloc to reduce stack pre-reservation size.
            Span<byte> bytes = stackalloc byte[8];
            switch (EnumInfo<TValue>.ByteSize)
            {
                case 1:
                    bytes[0] = Unsafe.As<TValue, byte>(ref value);
                    header.Set<T>(bytes[..1]);
                    break;

                case 2:
                    Unsafe.WriteUnaligned(ref bytes[0], Unsafe.As<TValue, ushort>(ref value));
                    header.Set<T>(bytes[..2]);
                    break;

                case 4:
                    Unsafe.WriteUnaligned(ref bytes[0], Unsafe.As<TValue, uint>(ref value));
                    header.Set<T>(bytes[..4]);
                    break;

                case 8:
                    Unsafe.WriteUnaligned(ref bytes[0], Unsafe.As<TValue, ulong>(ref value));
                    header.Set<T>(bytes[..8]);
                    break;

                default: throw new SwitchExpressionException(EnumInfo<TValue>.ByteSize);
            }
        }
        #endregion

        #region TryGet extensions
        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out bool value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            bool result = header.TryGet<T>(span);
            value = span[0] != 0;
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out byte value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            bool result = header.TryGet<T>(span);
            value = span[0];
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out sbyte value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            bool result = header.TryGet<T>(span);
            value = MemoryMarshal.Cast<byte, sbyte>(span)[0];
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out short value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[2];
            bool result = header.TryGet<T>(span);
            value = MemoryMarshal.Cast<byte, short>(span)[0];
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out ushort value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[2];
            bool result = header.TryGet<T>(span);
            value = MemoryMarshal.Cast<byte, ushort>(span)[0];
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out int value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[4];
            bool result = header.TryGet<T>(span);
            value = MemoryMarshal.Cast<byte, int>(span)[0];
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out uint value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[4];
            bool result = header.TryGet<T>(span);
            value = MemoryMarshal.Cast<byte, uint>(span)[0];
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out long value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[8];
            bool result = header.TryGet<T>(span);
            value = MemoryMarshal.Cast<byte, long>(span)[0];
            return result;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool TryGet<T>(this in Header header, out ulong value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[8];
            bool result = header.TryGet<T>(span);
            value = MemoryMarshal.Cast<byte, ulong>(span)[0];
            return result;
        }

        /// <summary>
        /// Reads <typeparamref name="TValue"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="CustomHeader{T}"/> type to reference.</typeparam>
        /// <typeparam name="TValue">Enum to read.</typeparam>
        public static bool TryGet<T, TValue>(this in Header header, out TValue value)
            where T : CustomHeader<T>, new()
            where TValue : Enum
        {
            // Only one stackalloc to reduce stack pre-reservation size.
            Span<byte> bytes = stackalloc byte[8];
            bool result;
            switch (EnumInfo<TValue>.ByteSize)
            {
                case 1:
                    result = header.TryGet<T>(bytes[..1]);
                    value = Unsafe.As<byte, TValue>(ref bytes[0]);
                    return result;

                case 2:
                    result = header.TryGet<T>(bytes[..2]);
                    ushort s = Unsafe.ReadUnaligned<ushort>(ref bytes[0]);
                    value = Unsafe.As<ushort, TValue>(ref s);
                    return result;

                case 4:
                    result = header.TryGet<T>(bytes[..4]);
                    uint i = Unsafe.ReadUnaligned<uint>(ref bytes[0]);
                    value = Unsafe.As<uint, TValue>(ref i);
                    return result;

                case 8:
                    result = header.TryGet<T>(bytes[..8]);
                    ulong l = Unsafe.ReadUnaligned<ulong>(ref bytes[0]);
                    value = Unsafe.As<ulong, TValue>(ref l);
                    return result;

                default: throw new SwitchExpressionException(EnumInfo<TValue>.ByteSize);
            }
        }
        #endregion

        #region Get(out value) extensions
        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out bool value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            header.Get<T>(span);
            value = span[0] != 0;
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out byte value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            header.Get<T>(span);
            value = span[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out sbyte value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            header.Get<T>(span);
            value = MemoryMarshal.Cast<byte, sbyte>(span)[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out short value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[2];
            header.Get<T>(span);
            value = MemoryMarshal.Cast<byte, short>(span)[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out ushort value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[2];
            header.Get<T>(span);
            value = MemoryMarshal.Cast<byte, ushort>(span)[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out int value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[4];
            header.Get<T>(span);
            value = MemoryMarshal.Cast<byte, int>(span)[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out uint value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[4];
            header.Get<T>(span);
            value = MemoryMarshal.Cast<byte, uint>(span)[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out long value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[8];
            header.Get<T>(span);
            value = MemoryMarshal.Cast<byte, long>(span)[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static void Get<T>(this in Header header, out ulong value) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[8];
            header.Get<T>(span);
            value = MemoryMarshal.Cast<byte, ulong>(span)[0];
        }

        /// <summary>
        /// Reads <paramref name="value"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="CustomHeader{T}"/> type to reference.</typeparam>
        /// <typeparam name="TValue">Enum to read.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Get<T, TValue>(this in Header header, out TValue value)
            where T : CustomHeader<T>, new()
            where TValue : Enum => value = GetEnum<T, TValue>(header);
        #endregion

        #region GetValue() extensions
        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static bool GetBool<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            header.Get<T>(span);
            return span[0] != 0;
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static byte GetByte<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            header.Get<T>(span);
            return span[0];
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static sbyte GetSByte<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[1];
            header.Get<T>(span);
            return MemoryMarshal.Cast<byte, sbyte>(span)[0];
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static short GetShort<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[2];
            header.Get<T>(span);
            return MemoryMarshal.Cast<byte, short>(span)[0];
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static ushort GetUShort<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[2];
            header.Get<T>(span);
            return MemoryMarshal.Cast<byte, ushort>(span)[0];
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static int GetInt<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[4];
            header.Get<T>(span);
            return MemoryMarshal.Cast<byte, int>(span)[0];
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static uint GetUInt<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[4];
            header.Get<T>(span);
            return MemoryMarshal.Cast<byte, uint>(span)[0];
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static long GetLong<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[8];
            header.Get<T>(span);
            return MemoryMarshal.Cast<byte, long>(span)[0];
        }

        /// <summary>
        /// Reads value from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        public static ulong GetULong<T>(this in Header header) where T : CustomHeader<T>, new()
        {
            Span<byte> span = stackalloc byte[8];
            header.Get<T>(span);
            return MemoryMarshal.Cast<byte, ulong>(span)[0];
        }

        /// <summary>
        /// Reads <typeparamref name="TValue"/> from the <see cref="CustomHeader{T}"/> content in provided <paramref name="header"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="CustomHeader{T}"/> type to reference.</typeparam>
        /// <typeparam name="TValue">Enum to read.</typeparam>
        public static TValue GetEnum<T, TValue>(this in Header header)
            where T : CustomHeader<T>, new()
            where TValue : Enum
        {
            // Only one stackalloc to reduce stack pre-reservation size.
            Span<byte> bytes = stackalloc byte[8];
            switch (EnumInfo<TValue>.ByteSize)
            {
                case 1:
                    header.Get<T>(bytes[..1]);
                    return Unsafe.As<byte, TValue>(ref bytes[0]);

                case 2:
                    header.Get<T>(bytes[..2]);
                    ushort s = Unsafe.ReadUnaligned<ushort>(ref bytes[0]);
                    return Unsafe.As<ushort, TValue>(ref s);

                case 4:
                    header.Get<T>(bytes[..4]);
                    uint i = Unsafe.ReadUnaligned<uint>(ref bytes[0]);
                    return Unsafe.As<uint, TValue>(ref i);

                case 8:
                    header.Get<T>(bytes[..8]);
                    ulong l = Unsafe.ReadUnaligned<ulong>(ref bytes[0]);
                    return Unsafe.As<ulong, TValue>(ref l);

                default: throw new SwitchExpressionException(EnumInfo<TValue>.ByteSize);
            }
        }
        #endregion


        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                  Internal
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        internal static class EnumInfo<T> where T : Enum
        {
            /// <summary>
            /// Possible values: 1, 2, 4, 8.
            /// </summary>
            public static readonly byte ByteSize = (byte)Unsafe.SizeOf<T>();
        }
    }
}
