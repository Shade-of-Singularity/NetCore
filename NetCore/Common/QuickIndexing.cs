using System;

namespace NetCore.Common
{
    /// <summary>
    /// Stores methods for quick indexing.
    /// </summary>
    /// <seealso cref="QuickMap{T}"/>
    public static class QuickIndexing
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// <see cref="QuickIndexPosition"/>s mapped with an array.
        /// Array has length of <see cref="QuickIndex.Limit"/>, covering all possible states.
        /// </summary>
        public static readonly QuickIndexMask[] Masks =
        [
            QuickIndexMask.One, QuickIndexMask.Two, QuickIndexMask.Three,
            QuickIndexMask.Four, QuickIndexMask.Five, QuickIndexMask.Six,
            QuickIndexMask.Seven, QuickIndexMask.Eight, QuickIndexMask.Nine,
            QuickIndexMask.Ten, QuickIndexMask.Eleven, QuickIndexMask.Twelve,
            QuickIndexMask.Thirteen, QuickIndexMask.Fourteen, QuickIndexMask.Fifteen,
            QuickIndexMask.Sixteen, QuickIndexMask.Seventeen, QuickIndexMask.Eightteen,
            QuickIndexMask.Nineteen,
        ];

        /// <summary>
        /// <see cref="QuickIndexPosition"/>s mapped with an array.
        /// Array has length of <see cref="QuickIndex.Limit"/>, covering all possible states.
        /// </summary>
        public static readonly QuickIndexPosition[] Positions =
        [
            QuickIndexPosition.One, QuickIndexPosition.Two, QuickIndexPosition.Three,
            QuickIndexPosition.Four, QuickIndexPosition.Five, QuickIndexPosition.Six,
            QuickIndexPosition.Seven, QuickIndexPosition.Eight, QuickIndexPosition.Nine,
            QuickIndexPosition.Ten, QuickIndexPosition.Eleven, QuickIndexPosition.Twelve,
            QuickIndexPosition.Thirteen, QuickIndexPosition.Fourteen, QuickIndexPosition.Fifteen,
            QuickIndexPosition.Sixteen, QuickIndexPosition.Seventeen, QuickIndexPosition.Eightteen,
            QuickIndexPosition.Nineteen,
        ];




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves next available <see cref="QuickIndex"/>.
        /// </summary>
        public static QuickIndex GetNextIndex(ref ushort inUse)
        {
            if (!TryGetNextIndex(ref inUse, out QuickIndex index))
            {
                throw new Exception("Exhausted all possible IDs for a Quick indexable object.");
            }

            return index;
        }

        /// <summary>
        /// Attempts to retrieve next available <see cref="QuickIndex"/>.
        /// </summary>
        public static bool TryGetNextIndex(ref ushort isUse, out QuickIndex index)
        {
            if (isUse < QuickIndex.Limit)
            {
                index = QuickIndex.GetFrom(isUse++);
                return true;
            }

            index = default;
            return false;
        }

        /// <summary>
        /// Retrieves <see cref="QuickIndexMask"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndexMask"/> encoding position under a given <paramref name="order"/>.</returns>
        public static QuickIndexMask GetMask(ushort order)
        {
            if (!TryGetMask(order, out QuickIndexMask mask))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{QuickIndex.Limit}]. Provided: {order}");
            }

            return mask;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickIndexMask"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <param name="mask"><see cref="QuickIndexMask"/> encoding position under a given <paramref name="order"/>.</param>
        public static bool TryGetMask(ushort order, out QuickIndexMask mask)
        {
            switch (order)
            {
                case 0: mask = QuickIndexMask.One; return true;
                case 1: mask = QuickIndexMask.Two; return true;
                case 2: mask = QuickIndexMask.Three; return true;
                case 3: mask = QuickIndexMask.Four; return true;
                case 4: mask = QuickIndexMask.Five; return true;
                case 5: mask = QuickIndexMask.Six; return true;
                case 6: mask = QuickIndexMask.Seven; return true;
                case 7: mask = QuickIndexMask.Eight; return true;
                case 8: mask = QuickIndexMask.Nine; return true;
                case 9: mask = QuickIndexMask.Ten; return true;
                case 10: mask = QuickIndexMask.Eleven; return true;
                case 11: mask = QuickIndexMask.Twelve; return true;
                case 12: mask = QuickIndexMask.Thirteen; return true;
                case 13: mask = QuickIndexMask.Fourteen; return true;
                case 14: mask = QuickIndexMask.Fifteen; return true;
                case 15: mask = QuickIndexMask.Sixteen; return true;
                case 16: mask = QuickIndexMask.Seventeen; return true;
                case 17: mask = QuickIndexMask.Eightteen; return true;
                case 18: mask = QuickIndexMask.Nineteen; return true;
                default: mask = default; return false;
            }
        }

        /// <summary>
        /// Retrieves <see cref="QuickIndexPosition"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndexPosition"/> encoding position in an array for a given <paramref name="index"/>.</returns>
        public static QuickIndexPosition GetPosition(uint index)
        {
            if (!TryGetPosition(index, out QuickIndexPosition position))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{QuickIndex.Limit}]. Provided: {index}");
            }

            return position;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickIndexPosition"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <param name="position"><see cref="QuickIndexPosition"/> encoding position in an array for a given <paramref name="index"/>.</param>
        public static bool TryGetPosition(uint index, out QuickIndexPosition position)
        {
            switch (index)
            {
                case 0: position = QuickIndexPosition.One; return true;
                case 1: position = QuickIndexPosition.Two; return true;
                case 2: position = QuickIndexPosition.Three; return true;
                case 3: position = QuickIndexPosition.Four; return true;
                case 4: position = QuickIndexPosition.Five; return true;
                case 5: position = QuickIndexPosition.Six; return true;
                case 6: position = QuickIndexPosition.Seven; return true;
                case 7: position = QuickIndexPosition.Eight; return true;
                case 8: position = QuickIndexPosition.Nine; return true;
                case 9: position = QuickIndexPosition.Ten; return true;
                case 10: position = QuickIndexPosition.Eleven; return true;
                case 11: position = QuickIndexPosition.Twelve; return true;
                case 12: position = QuickIndexPosition.Thirteen; return true;
                case 13: position = QuickIndexPosition.Fourteen; return true;
                case 14: position = QuickIndexPosition.Fifteen; return true;
                case 15: position = QuickIndexPosition.Sixteen; return true;
                case 16: position = QuickIndexPosition.Seventeen; return true;
                case 17: position = QuickIndexPosition.Eightteen; return true;
                case 18: position = QuickIndexPosition.Nineteen; return true;
                default: position = default; return false;
            }
        }
    }
}
