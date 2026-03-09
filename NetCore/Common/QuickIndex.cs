using System.Runtime.InteropServices;

namespace NetCore.Common
{
    /// <summary>
    /// Represents a position within a specific array or collection, indexed with <see cref="QuickIndexing"/>.
    /// </summary>
    /// <param name="mask">Mask for a specific element in array.</param>
    /// <param name="offset">Offset to apply for the masked data, to normalize it to array bounds (e.g. for ulong unpacking).</param>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct QuickIndex(QuickIndexMask mask, QuickIndexPosition offset)
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Max amount of items (inclusive) which <see cref="QuickIndexing"/> supports.
        /// </summary>
        public const int Limit = 13;
        /// <summary>
        /// Encodes no index.
        /// </summary>
        public static readonly QuickIndex Invalid = default;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Public Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Mask, encoding index in an internal array.
        /// </summary>
        [FieldOffset(0)] public readonly QuickIndexMask Mask = (QuickIndexMask)((ulong)mask & QuickIndexing.IndexMask); // [0-7 bytes]
        /// <summary>
        /// Offset we need to apply to <see cref="Mask"/> to get the index of an item in the array.
        /// </summary>
        [FieldOffset(7)] public readonly QuickIndexPosition Position = offset; // [8th byte]
    }
}
