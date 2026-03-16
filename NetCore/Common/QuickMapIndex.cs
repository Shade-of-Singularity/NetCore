using System;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
    /// <summary>
    /// Represents a position within a specific array or collection, indexed with <see cref="QuickMapIndexing"/>.
    /// </summary>
    /// <param name="mask">Mask for a specific element in array.</param>
    /// <param name="offset">Offset to apply for the masked data, to normalize it to array bounds (e.g. for ulong unpacking).</param>
    /// <param name="order">Order in which this <see cref="QuickMapIndex"/> was created in its local group.</param>
    /// Note: Since we increase <see cref="QuickMapIndex"/> size limit from 8 bytes to 16 recently -
    ///  it might be a good time to revisit the concept!
    ///  Maybe we will be able to increase the limit to over 15 items)
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct QuickMapIndex(QuickMapIndexMask mask, QuickMapIndexPosition offset, ushort order)
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Max amount of items (inclusive) which <see cref="QuickMapIndexing"/> supports.
        /// </summary>
        public const int Limit = 19;
        /// <summary>
        /// Encodes no index.
        /// </summary>
        public static readonly QuickMapIndex Invalid = default;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Public Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Mask, encoding index in an internal array.
        /// </summary>
        [FieldOffset(0)] public readonly QuickMapIndexMask Mask = mask;
        /// <summary>
        /// Order in which this <see cref="QuickMapIndex"/> was initialized in its category.
        /// </summary>
        [FieldOffset(8)] public readonly ushort Order = order;
        /// <summary>
        /// Flag covering a specific bit, based on when QuickIndex was initialized
        /// </summary>
        [FieldOffset(10)] public readonly uint BitFlag = 1u << order;
        /// <summary>
        /// Offset we need to apply to a <see cref="Mask"/> to get the index of an item in the array.
        /// </summary>
        [FieldOffset(14)] public readonly QuickMapIndexPosition Position = offset;





        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves <see cref="QuickMapIndex"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <returns><see cref="QuickMapIndex"/> encoding position in an array for a given <paramref name="order"/>.</returns>
        public static QuickMapIndex GetFrom(ushort order)
        {
            if (!TryGetFrom(order, out QuickMapIndex result))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{Limit}]. Provided: {order}");
            }

            return result;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickMapIndex"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <param name="result"><see cref="QuickMapIndex"/> encoding position in an array for a given <paramref name="order"/>.</param>
        public static bool TryGetFrom(ushort order, out QuickMapIndex result)
        {
            switch (order)
            {
                case 0: result = new QuickMapIndex(QuickMapIndexMask.One, QuickMapIndexPosition.One, order); return true;
                case 1: result = new QuickMapIndex(QuickMapIndexMask.Two, QuickMapIndexPosition.Two, order); return true;
                case 2: result = new QuickMapIndex(QuickMapIndexMask.Three, QuickMapIndexPosition.Three, order); return true;
                case 3: result = new QuickMapIndex(QuickMapIndexMask.Four, QuickMapIndexPosition.Four, order); return true;
                case 4: result = new QuickMapIndex(QuickMapIndexMask.Five, QuickMapIndexPosition.Five, order); return true;
                case 5: result = new QuickMapIndex(QuickMapIndexMask.Six, QuickMapIndexPosition.Six, order); return true;
                case 6: result = new QuickMapIndex(QuickMapIndexMask.Seven, QuickMapIndexPosition.Seven, order); return true;
                case 7: result = new QuickMapIndex(QuickMapIndexMask.Eight, QuickMapIndexPosition.Eight, order); return true;
                case 8: result = new QuickMapIndex(QuickMapIndexMask.Nine, QuickMapIndexPosition.Nine, order); return true;
                case 9: result = new QuickMapIndex(QuickMapIndexMask.Ten, QuickMapIndexPosition.Ten, order); return true;
                case 10: result = new QuickMapIndex(QuickMapIndexMask.Eleven, QuickMapIndexPosition.Eleven, order); return true;
                case 11: result = new QuickMapIndex(QuickMapIndexMask.Twelve, QuickMapIndexPosition.Twelve, order); return true;
                case 12: result = new QuickMapIndex(QuickMapIndexMask.Thirteen, QuickMapIndexPosition.Thirteen, order); return true;
                case 13: result = new QuickMapIndex(QuickMapIndexMask.Fourteen, QuickMapIndexPosition.Fourteen, order); return true;
                case 14: result = new QuickMapIndex(QuickMapIndexMask.Fifteen, QuickMapIndexPosition.Fifteen, order); return true;
                default: result = default; return false;
            }
        }
    }
}
