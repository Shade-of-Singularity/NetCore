using System;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
    /// <summary>
    /// Represents a position within a specific array or collection, indexed with <see cref="QuickIndexing"/>.
    /// </summary>
    /// <param name="mask">Mask for a specific element in array.</param>
    /// <param name="offset">Offset to apply for the masked data, to normalize it to array bounds (e.g. for ulong unpacking).</param>
    /// <param name="order">Order in which this <see cref="QuickIndex"/> was created in its local group.</param>
    /// Note: Since we increase <see cref="QuickIndex"/> size limit from 8 bytes to 16 recently -
    ///  it might be a good time to revisit the concept!
    ///  Maybe we will be able to increase the limit to over 15 items)
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct QuickIndex(QuickIndexMask mask, QuickIndexPosition offset, ushort order)
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Max amount of items (inclusive) which <see cref="QuickIndexing"/> supports.
        /// </summary>
        public const int Limit = 19;
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
        [FieldOffset(0)] public readonly QuickIndexMask Mask = mask;
        /// <summary>
        /// Order in which this <see cref="QuickIndex"/> was initialized in its category.
        /// </summary>
        [FieldOffset(8)] public readonly ushort Order = order;
        /// <summary>
        /// Flag covering a specific bit, based on when QuickIndex was initialized
        /// </summary>
        [FieldOffset(10)] public readonly uint BitFlag = 1u << order;
        /// <summary>
        /// Offset we need to apply to a <see cref="Mask"/> to get the index of an item in the array.
        /// </summary>
        [FieldOffset(14)] public readonly QuickIndexPosition Position = offset;





        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves <see cref="QuickIndex"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndex"/> encoding position in an array for a given <paramref name="order"/>.</returns>
        public static QuickIndex GetFrom(ushort order)
        {
            if (!TryGetFrom(order, out QuickIndex result))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{Limit}]. Provided: {order}");
            }

            return result;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickIndex"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <param name="result"><see cref="QuickIndex"/> encoding position in an array for a given <paramref name="order"/>.</param>
        public static bool TryGetFrom(ushort order, out QuickIndex result)
        {
            switch (order)
            {
                case 0: result = new QuickIndex(QuickIndexMask.One, QuickIndexPosition.One, order); return true;
                case 1: result = new QuickIndex(QuickIndexMask.Two, QuickIndexPosition.Two, order); return true;
                case 2: result = new QuickIndex(QuickIndexMask.Three, QuickIndexPosition.Three, order); return true;
                case 3: result = new QuickIndex(QuickIndexMask.Four, QuickIndexPosition.Four, order); return true;
                case 4: result = new QuickIndex(QuickIndexMask.Five, QuickIndexPosition.Five, order); return true;
                case 5: result = new QuickIndex(QuickIndexMask.Six, QuickIndexPosition.Six, order); return true;
                case 6: result = new QuickIndex(QuickIndexMask.Seven, QuickIndexPosition.Seven, order); return true;
                case 7: result = new QuickIndex(QuickIndexMask.Eight, QuickIndexPosition.Eight, order); return true;
                case 8: result = new QuickIndex(QuickIndexMask.Nine, QuickIndexPosition.Nine, order); return true;
                case 9: result = new QuickIndex(QuickIndexMask.Ten, QuickIndexPosition.Ten, order); return true;
                case 10: result = new QuickIndex(QuickIndexMask.Eleven, QuickIndexPosition.Eleven, order); return true;
                case 11: result = new QuickIndex(QuickIndexMask.Twelve, QuickIndexPosition.Twelve, order); return true;
                case 12: result = new QuickIndex(QuickIndexMask.Thirteen, QuickIndexPosition.Thirteen, order); return true;
                case 13: result = new QuickIndex(QuickIndexMask.Fourteen, QuickIndexPosition.Fourteen, order); return true;
                case 14: result = new QuickIndex(QuickIndexMask.Fifteen, QuickIndexPosition.Fifteen, order); return true;
                default: result = default; return false;
            }
        }
    }
}
