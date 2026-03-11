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
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Mask, covering all 7-ish bytes encoding <see cref="QuickIndexMask"/>.
        /// </summary>
        public const ulong IndexMask = 0b00000000_1111111_1111111_111111_111111_11111_11111_1111_1111_111_111_11_11_1_0uL;
        /// <summary>
        /// Mask, covering the last 8th byte, which encodes <see cref="QuickIndexPosition"/>.
        /// </summary>
        public const ulong PositionMask = 0b11111111_0000000_0000000_000000_000000_00000_00000_0000_0000_000_000_00_00_0_0uL;   




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves next available <see cref="QuickIndex"/>.
        /// </summary>
        public static QuickIndex GetNextIndex(ref int inUse)
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
        public static bool TryGetNextIndex(ref int isUse, out QuickIndex index)
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
        /// Retrieves <see cref="QuickIndexMask"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndexMask"/> encoding position under a given <paramref name="index"/>.</returns>
        public static QuickIndexMask GetMask(int index)
        {
            if (!TryGetMask(index, out QuickIndexMask mask))
            {
                throw new ArgumentOutOfRangeException($"ConnectionID of an quickly indexed item should be in a range [0:{QuickIndex.Limit}]. Provided: {index}");
            }

            return mask;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickIndexMask"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <param name="position"><see cref="QuickIndexMask"/> encoding position under a given <paramref name="index"/>.</param>
        public static bool TryGetMask(int index, out QuickIndexMask position)
        {
            position = index switch
            {
                0 => QuickIndexMask.One,
                1 => QuickIndexMask.Two,
                2 => QuickIndexMask.Three,
                3 => QuickIndexMask.Four,
                4 => QuickIndexMask.Five,
                5 => QuickIndexMask.Six,
                6 => QuickIndexMask.Seven,
                7 => QuickIndexMask.Eight,
                8 => QuickIndexMask.Nine,
                9 => QuickIndexMask.Ten,
                10 => QuickIndexMask.Eleven,
                11 => QuickIndexMask.Twelve,
                12 => QuickIndexMask.Thirteen,
                13 => QuickIndexMask.Fourteen,
                _ => QuickIndexMask.None,
            };

            return position != QuickIndexMask.None;
        }

        /// <summary>
        /// Retrieves <see cref="QuickIndexPosition"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndexPosition"/> encoding position in an array for a given <paramref name="index"/>.</returns>
        public static QuickIndexPosition GetPosition(int index)
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
        public static bool TryGetPosition(int index, out QuickIndexPosition position)
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
                default: position = default; return false;
            }
        }
    }
}
