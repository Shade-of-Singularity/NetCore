using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
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
        public const ulong IndexMask = 0b00000000_0_1111111_1111111_111111_111111_11111_11111_1111_1111_111_111_11_11_1uL;
        /// <summary>
        /// Mask, covering the last 8th byte, which encodes <see cref="QuickIndexPosition"/>.
        /// </summary>
        public const ulong PositionMask = 0b11111111_0_0000000_0000000_000000_000000_00000_00000_0000_0000_000_000_00_00_0uL;
        /// <summary>
        /// Mask, covering single unused bit. Not used internally, but we provide the constant if you need it.
        /// </summary>
        public const ulong LooseBitMask = 0b00000000_1_0000000_0000000_000000_000000_00000_00000_0000_0000_000_000_00_00_0uL;




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
        public static bool TryGetNextIndex(ref int occupied, out QuickIndex index)
        {
            if (occupied < QuickIndex.Limit)
            {
                index = GetIndex(occupied++);
                return true;
            }

            index = default;
            return false;
        }

        /// <summary>
        /// Retrieves <see cref="QuickIndex"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndex"/> encoding position in an array for a given <paramref name="index"/>.</returns>
        public static QuickIndex GetIndex(int index)
        {
            if (!TryGetIndex(index, out QuickIndex result))
            {
                throw new ArgumentOutOfRangeException($"ID of an quickly indexed item should be in a range [0:{QuickIndex.Limit}]. Provided: {index}");
            }

            return result;
        }

        /// <summary>
        /// Tries to retrieve <see cref="QuickIndex"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <param name="result"><see cref="QuickIndex"/> encoding position in an array for a given <paramref name="index"/>.</param>
        public static bool TryGetIndex(int index, out QuickIndex result)
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

        /// <summary>
        /// Retrieves <see cref="QuickIndexMask"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndexMask"/> encoding position under a given <paramref name="index"/>.</returns>
        public static QuickIndexMask GetMask(int index)
        {
            if (!TryGetMask(index, out QuickIndexMask mask))
            {
                throw new ArgumentOutOfRangeException($"ID of an quickly indexed item should be in a range [0:{QuickIndex.Limit}]. Provided: {index}");
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
                _ => QuickIndexMask.None,
            };

            return position != QuickIndexMask.None;
        }

        /// <summary>
        /// Retrieves <see cref="QuickIndexPosition"/> for a given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of an item in a collection.</param>
        /// <returns><see cref="QuickIndexPosition"/> encoding position in an array for a given <paramref name="index"/>.</returns>
        public static QuickIndexMask GetPosition(int index)
        {
            if (!TryGetMask(index, out QuickIndexMask position))
            {
                throw new ArgumentOutOfRangeException($"ID of an quickly indexed item should be in a range [0:{QuickIndex.Limit}]. Provided: {index}");
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
                default: position = default; return false;
            }
        }

        public struct LookupBuilder
        {
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Static Fields
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            public static readonly QuickIndexMask[] Masks =
            [
                QuickIndexMask.One, QuickIndexMask.Two, QuickIndexMask.Three, QuickIndexMask.Four,
                QuickIndexMask.Five, QuickIndexMask.Six, QuickIndexMask.Seven, QuickIndexMask.Eight, QuickIndexMask.Nine,
                QuickIndexMask.Ten, QuickIndexMask.Eleven, QuickIndexMask.Twelve, QuickIndexMask.Thirteen,
            ];

            public static readonly QuickIndexPosition[] Offsets =
            [
                QuickIndexPosition.One, QuickIndexPosition.Two, QuickIndexPosition.Three,
                QuickIndexPosition.Four, QuickIndexPosition.Five, QuickIndexPosition.Six, QuickIndexPosition.Seven,
                QuickIndexPosition.Eight, QuickIndexPosition.Nine, QuickIndexPosition.Ten, QuickIndexPosition.Eleven,
                QuickIndexPosition.Twelve, QuickIndexPosition.Thirteen,
            ];




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Private Fields
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            private QuickIndexMask mask;
            private bool zero;




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Public Methods
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            //private bool IsFinished() => (mask & IndexMask.Invalid) == IndexMask.Invalid;

            /// <summary>
            /// Lists index of an item in a lookup list.
            /// </summary>
            public void List(QuickIndex index)
            {
                if ((mask & QuickIndexMask.Invalid) != QuickIndexMask.Invalid)
                {
                    zero |= index.Mask == QuickIndexMask.None;
                    mask |= index.Mask;
                }
            }

            /// <summary>
            /// Builds a lookup blob from a mask, formed using <see cref="List(QuickIndexMask)"/> inputs.
            /// </summary>
            /// <returns>An lookup blob to be used in indexing.</returns>
            public ulong Lookup()
            {
                if ((mask & QuickIndexMask.Invalid) == QuickIndexMask.Invalid)
                {
                    return (ulong)(mask & QuickIndexMask.All);
                }

                ulong lookup = 0; // Adds "Completed/Finished" flag to the blob prematurely.
                ulong stored = zero ? 1ul : 0ul; // How many items were stored in the mask. Calculated during finishing for more reliability.
                for (byte i = 1; i < QuickIndex.Limit; i++) // Start from '1', since Index.Zero doesn't do anything.
                {
                    QuickIndexMask region = Masks[i];

                    /// Keep in mind that <see cref="QuickIndexMask.None"/> will never pass this section.
                    region &= mask;
                    if (region != 0)
                    {
                        lookup |= stored << (int)Offsets[i];
                        stored++;
                    }
                }

                // Marks builder as "Finished".
                mask = (QuickIndexMask)lookup | QuickIndexMask.Invalid;
                return lookup;
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public static void Run()
        {
            // Indexing items for later.
            var i = Start();

            // Each 'Index' here will occupy only 8 bytes.
            QuickIndex a = i.Index();
            QuickIndex b = i.Index();
            QuickIndex c = i.Index();
            QuickIndex d = i.Index();
            QuickIndex e = i.Index();
            QuickIndex f = i.Index();
            Console.WriteLine($"Size: {Marshal.SizeOf(typeof(QuickIndex))}");

            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            Console.WriteLine(d);
            Console.WriteLine(e);
            Console.WriteLine(f);

            // Creates an array we want to index.
            object[] array = new object[3]
            {
                "Item (a) #1",
                "Item (d) #2",
                "Item (e) #3"
            };

            // Saturates array in random order ()
            var l = Lookup();
            l.List(a);
            l.List(e);
            l.List(d);
            ulong lookup = l.Lookup();

            // Lookups for those specific items.
            Console.WriteLine(Lookup(array, a, lookup));
            Console.WriteLine(Lookup(array, d, lookup));
            Console.WriteLine(Lookup(array, e, lookup));

            // Remarks:
            // QIndexing will return a first value of array by default.
            // Initially QIndexing was designed with casting and pattern matching in mind.
            // As such, environment of QIndexing was handling type mismatching by throwing an error automatically.
            // Lookup will only throw if you have array with '0' items.
            //
            // Maybe later I will be able to fit a "kill bit" in each IndexMask,
            // Which will forcefully throw any array access unless you have a very large array (billions items long)
            // Alternatively, I can just offset everything by '1' to the left be default, to make '0' index a 'null' instead.
            // It will break zero-based indexing though.
            Console.WriteLine(Lookup(array, b, lookup)); // Will return element at '0' index.
        }

        /// <summary>
        /// Starts new <see cref="Indexer"/>.
        /// </summary>
        /// <returns></returns>
        public static Indexer Start() => new Indexer();

        /// <summary>
        /// Starts new <see cref="LookupBuilder"/>.
        /// </summary>
        public static LookupBuilder Lookup() => new();

        /// <summary>
        /// Looks-up a value for you from <paramref name="array"/>, based on a unique <paramref name="index"/> of your item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">Index, retrieved using <see cref="Indexer"/> (see also: <see cref="Start"/>).</param>
        /// <param name="lookup">Look-up blob, provided by <see cref="LookupBuilder.Lookup"/>.</param>
        public static T Lookup<T>(T[] array, QuickIndex index, ulong lookup)
        {
            return array[(int)((lookup & (ulong)index.Mask) >> (int)index.Position)];
        }

        /// <summary>
        /// Looks-up a value for you from <paramref name="list"/>, based on a unique <paramref name="index"/> of your item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">Index, retrieved using <see cref="Indexer"/> (see also: <see cref="Start"/>).</param>
        /// <param name="lookup">Look-up blob, provided by <see cref="LookupBuilder.Lookup"/>.</param>
        public static T Lookup<T>(IList<T> list, QuickIndex index, ulong lookup)
        {
            return list[(int)((lookup & (ulong)index.Mask) >> (int)index.Position)];
        }
    }
}
