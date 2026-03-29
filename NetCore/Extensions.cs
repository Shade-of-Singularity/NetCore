using System;
using System.Runtime.InteropServices;

namespace NetCore
{
    /// <summary>
    /// Useful extensions for working with <see cref="NetCore"/> generally.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Turns input <paramref name="value"/> in a <see cref="ReadOnlySpan{T}"/> of bytes instead of chars.
        /// </summary>
        public static ReadOnlySpan<byte> AsByteSpan(this string value) => MemoryMarshal.AsBytes(value.AsSpan());
        /// <summary>
        /// Turns input <paramref name="span"/> in a <see cref="ReadOnlySpan{T}"/> of bytes instead of chars.
        /// </summary>
        public static ReadOnlySpan<byte> AsByteSpan(this ReadOnlySpan<char> span) => MemoryMarshal.AsBytes(span);
    }
}
