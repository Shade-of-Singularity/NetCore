using System;

namespace NetCore.Common
{
    /// <summary>
    /// Stores methods for quick indexing.
    /// </summary>
    /// <seealso cref="QuickMap{T}"/>
    public static class QuickMapIndexing
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// <see cref="QuickMapIndexPosition"/>s mapped with an array.
        /// Array has length of <see cref="QuickMapIndex.Limit"/>, covering all possible states.
        /// </summary>
        public static readonly QuickMapIndexMask[] Masks =
        [
            QuickMapIndexMask.One, QuickMapIndexMask.Two, QuickMapIndexMask.Three,
            QuickMapIndexMask.Four, QuickMapIndexMask.Five, QuickMapIndexMask.Six,
            QuickMapIndexMask.Seven, QuickMapIndexMask.Eight, QuickMapIndexMask.Nine,
            QuickMapIndexMask.Ten, QuickMapIndexMask.Eleven, QuickMapIndexMask.Twelve,
            QuickMapIndexMask.Thirteen, QuickMapIndexMask.Fourteen, QuickMapIndexMask.Fifteen,
            QuickMapIndexMask.Sixteen, QuickMapIndexMask.Seventeen, QuickMapIndexMask.Eightteen,
            QuickMapIndexMask.Nineteen,
        ];

        /// <summary>
        /// <see cref="QuickMapIndexPosition"/>s mapped with an array.
        /// Array has length of <see cref="QuickMapIndex.Limit"/>, covering all possible states.
        /// </summary>
        public static readonly QuickMapIndexPosition[] Positions =
        [
            QuickMapIndexPosition.One, QuickMapIndexPosition.Two, QuickMapIndexPosition.Three,
            QuickMapIndexPosition.Four, QuickMapIndexPosition.Five, QuickMapIndexPosition.Six,
            QuickMapIndexPosition.Seven, QuickMapIndexPosition.Eight, QuickMapIndexPosition.Nine,
            QuickMapIndexPosition.Ten, QuickMapIndexPosition.Eleven, QuickMapIndexPosition.Twelve,
            QuickMapIndexPosition.Thirteen, QuickMapIndexPosition.Fourteen, QuickMapIndexPosition.Fifteen,
            QuickMapIndexPosition.Sixteen, QuickMapIndexPosition.Seventeen, QuickMapIndexPosition.Eightteen,
            QuickMapIndexPosition.Nineteen,
        ];




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves next available <see cref="QuickMapIndex"/>.
        /// </summary>
        public static QuickMapIndex GetNextIndex(ref ushort inUse)
        {
            if (!TryGetNextIndex(ref inUse, out QuickMapIndex index))
            {
                throw new Exception("Exhausted all possible IDs for a Quick indexable object.");
            }

            return index;
        }

        /// <summary>
        /// Attempts to retrieve next available <see cref="QuickMapIndex"/>.
        /// </summary>
        public static bool TryGetNextIndex(ref ushort isUse, out QuickMapIndex index)
        {
            if (isUse < QuickMapIndex.Limit)
            {
                index = QuickMapIndex.GetFrom(isUse++);
                return true;
            }

            index = default;
            return false;
        }

        /// <summary>
        /// Retrieves <see cref="QuickMapIndexMask"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <returns><see cref="QuickMapIndexMask"/> encoding position under a given <paramref name="order"/>.</returns>
        public static QuickMapIndexMask GetMask(ushort order)
        {
            if (!TryGetMask(order, out QuickMapIndexMask mask))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{QuickMapIndex.Limit}]. Provided: {order}");
            }

            return mask;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickMapIndexMask"/> for a given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Index of an item in a collection.</param>
        /// <param name="mask"><see cref="QuickMapIndexMask"/> encoding position under a given <paramref name="order"/>.</param>
        public static bool TryGetMask(ushort order, out QuickMapIndexMask mask)
        {
            switch (order)
            {
                case 0: mask = QuickMapIndexMask.One; return true;
                case 1: mask = QuickMapIndexMask.Two; return true;
                case 2: mask = QuickMapIndexMask.Three; return true;
                case 3: mask = QuickMapIndexMask.Four; return true;
                case 4: mask = QuickMapIndexMask.Five; return true;
                case 5: mask = QuickMapIndexMask.Six; return true;
                case 6: mask = QuickMapIndexMask.Seven; return true;
                case 7: mask = QuickMapIndexMask.Eight; return true;
                case 8: mask = QuickMapIndexMask.Nine; return true;
                case 9: mask = QuickMapIndexMask.Ten; return true;
                case 10: mask = QuickMapIndexMask.Eleven; return true;
                case 11: mask = QuickMapIndexMask.Twelve; return true;
                case 12: mask = QuickMapIndexMask.Thirteen; return true;
                case 13: mask = QuickMapIndexMask.Fourteen; return true;
                case 14: mask = QuickMapIndexMask.Fifteen; return true;
                case 15: mask = QuickMapIndexMask.Sixteen; return true;
                case 16: mask = QuickMapIndexMask.Seventeen; return true;
                case 17: mask = QuickMapIndexMask.Eightteen; return true;
                case 18: mask = QuickMapIndexMask.Nineteen; return true;
                default: mask = default; return false;
            }
        }

        /// <summary>
        /// Retrieves <see cref="QuickMapIndexPosition"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickMapIndexPosition"/> encoding position in an array for a given <paramref name="index"/>.</returns>
        public static QuickMapIndexPosition GetPosition(uint index)
        {
            if (!TryGetPosition(index, out QuickMapIndexPosition position))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{QuickMapIndex.Limit}]. Provided: {index}");
            }

            return position;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickMapIndexPosition"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <param name="position"><see cref="QuickMapIndexPosition"/> encoding position in an array for a given <paramref name="index"/>.</param>
        public static bool TryGetPosition(uint index, out QuickMapIndexPosition position)
        {
            switch (index)
            {
                case 0: position = QuickMapIndexPosition.One; return true;
                case 1: position = QuickMapIndexPosition.Two; return true;
                case 2: position = QuickMapIndexPosition.Three; return true;
                case 3: position = QuickMapIndexPosition.Four; return true;
                case 4: position = QuickMapIndexPosition.Five; return true;
                case 5: position = QuickMapIndexPosition.Six; return true;
                case 6: position = QuickMapIndexPosition.Seven; return true;
                case 7: position = QuickMapIndexPosition.Eight; return true;
                case 8: position = QuickMapIndexPosition.Nine; return true;
                case 9: position = QuickMapIndexPosition.Ten; return true;
                case 10: position = QuickMapIndexPosition.Eleven; return true;
                case 11: position = QuickMapIndexPosition.Twelve; return true;
                case 12: position = QuickMapIndexPosition.Thirteen; return true;
                case 13: position = QuickMapIndexPosition.Fourteen; return true;
                case 14: position = QuickMapIndexPosition.Fifteen; return true;
                case 15: position = QuickMapIndexPosition.Sixteen; return true;
                case 16: position = QuickMapIndexPosition.Seventeen; return true;
                case 17: position = QuickMapIndexPosition.Eightteen; return true;
                case 18: position = QuickMapIndexPosition.Nineteen; return true;
                default: position = default; return false;
            }
        }
    }
}
