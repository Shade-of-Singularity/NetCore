using System;
using System.Runtime.CompilerServices;
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
        /// TODO: Increase limit to 14 using 0th bit which always point to a 0th index.
        public const int Limit = 13;
        /// <summary>
        /// Encodes no index.
        /// </summary>
        public static readonly QuickIndex Invalid = default;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Mask within <see cref="QuickIndexing.IndexMask"/>, without extra bits from <see cref="QuickIndexPosition"/>.
        /// </summary>
        public QuickIndexMask Mask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (QuickIndexMask)((ulong)RawMask & QuickIndexing.IndexMask);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Public Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Mask, encoding index in an internal array.
        /// </summary>
        [FieldOffset(0)] public readonly QuickIndexMask RawMask = mask; // [0-7 bytes]
        /// <summary>
        /// Offset we need to apply to a <see cref="Mask"/> to get the index of an item in the array.
        /// </summary>
        [FieldOffset(7)] public readonly QuickIndexPosition position = offset; // [8th byte]




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves <see cref="QuickIndex"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndex"/> encoding position in an array for a given <paramref name="index"/>.</returns>
        public static QuickIndex GetFrom(int index)
        {
            if (!TryGetFrom(index, out QuickIndex result))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{Limit}]. Provided: {index}");
            }

            return result;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickIndex"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <param name="result"><see cref="QuickIndex"/> encoding position in an array for a given <paramref name="index"/>.</param>
        public static bool TryGetFrom(int index, out QuickIndex result)
        {
            switch (index)
            {
                case 0: result = new QuickIndex(QuickIndexMask.One, QuickIndexPosition.One); return true;
                case 1: result = new QuickIndex(QuickIndexMask.Two, QuickIndexPosition.Two); return true;
                case 2: result = new QuickIndex(QuickIndexMask.Three, QuickIndexPosition.Three); return true;
                case 3: result = new QuickIndex(QuickIndexMask.Four, QuickIndexPosition.Four); return true;
                case 4: result = new QuickIndex(QuickIndexMask.Five, QuickIndexPosition.Five); return true;
                case 5: result = new QuickIndex(QuickIndexMask.Six, QuickIndexPosition.Six); return true;
                case 6: result = new QuickIndex(QuickIndexMask.Seven, QuickIndexPosition.Seven); return true;
                case 7: result = new QuickIndex(QuickIndexMask.Eight, QuickIndexPosition.Eight); return true;
                case 8: result = new QuickIndex(QuickIndexMask.Nine, QuickIndexPosition.Nine); return true;
                case 9: result = new QuickIndex(QuickIndexMask.Ten, QuickIndexPosition.Ten); return true;
                case 10: result = new QuickIndex(QuickIndexMask.Eleven, QuickIndexPosition.Eleven); return true;
                case 11: result = new QuickIndex(QuickIndexMask.Twelve, QuickIndexPosition.Twelve); return true;
                case 12: result = new QuickIndex(QuickIndexMask.Thirteen, QuickIndexPosition.Thirteen); return true;
                default: result = default; return false;
            }
        }
    }
}
