using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NetCore.Common
{
    /// <summary>
    /// Similar <see cref="QuickMap{T}"/>, but is ordered and are limited to 16 items instead of 19.
    /// </summary>
    /// <remarks>
    /// NOT thread-safe!
    /// </remarks>
    /// <typeparam name="TBase">Base class of an item.</typeparam>
    public struct HashList<TBase> where TBase : class
    {
        static class BaseSet
        {
            public const int MaskBits = 4; // Amount of bits used to encode the mask.
            public const int ItemLimit = 16; // Technical limitation.
            static byte inUse;
            public static void NextSet(out ulong mask, out int shift, out ushort bitFlag)
            {
                if (inUse >= ItemLimit)
                {
                    throw new Exception($"{nameof(HashList<TBase>)} - exhausted all item IDs ({inUse}/{ItemLimit})");
                }

                int index = inUse;
                shift = index * MaskBits;
                mask = 0xFuL << shift;
                bitFlag = (ushort)(1u << index);
                inUse++;
            }
        }

        static class ID<TItem>
        {
            public static readonly int Shift;
            public static readonly ulong Mask;
            public static readonly ushort BitFlag;
            static ID() => BaseSet.NextSet(out Mask, out Shift, out BitFlag);
        }

        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves amount of stored items in this <see cref="HashList{TBase}"/>.
        /// </summary>
        public readonly int Count => stored;
        /// <summary>
        /// Gets or sets the element at the specified <paramref name="index"/>.
        /// </summary>
        public TBase this[int index]
        {
            readonly get
            {
                if (index < 0 || index >= stored)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return items[index]!;
            }

            set => throw new NotImplementedException();
        }



        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private ulong lookup;
        private ushort flags;
        private ushort stored;
        private TBase?[] items = [];




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default constructor initializing the list with initial capacity of 0.
        /// </summary>
        public HashList() : this(0) { }
        /// <summary>
        /// Creates list with a specified initial <paramref name="capacity"/> before array resizing is needed.
        /// </summary>
        public HashList(int capacity)
        {
            if (capacity < 0 || capacity >= BaseSet.ItemLimit)
                throw new ArgumentOutOfRangeException($"{nameof(HashList<TBase>)} {nameof(capacity)} should be within a range: [0:{BaseSet.ItemLimit}]");
            items = capacity == 0 ? [] : new TBase[capacity];
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if item of a type <typeparamref name="TItem"/> exist in the list.
        /// </summary>
        public readonly bool Has<TItem>() where TItem : class, TBase => (flags & ID<TItem>.BitFlag) != 0;

        /// <summary>
        /// Checks if given <typeparamref name="TItem"/> <paramref name="instance"/> exist in the list.
        /// </summary>
        public readonly bool Has<TItem>(TItem instance) where TItem : class, TBase => (flags & ID<TItem>.BitFlag) switch
        {
            0 => false,
            _ => items[(lookup & ID<TItem>.Mask) >> ID<TItem>.Shift] == instance,
        };

        /// <summary>
        /// Attempts to retrieve an item of a type <typeparamref name="TItem"/> from the list.
        /// </summary>
        public readonly bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : class, TBase
        {
            if ((flags & ID<TItem>.BitFlag) == 0)
            {
                item = default;
                return false;
            }

            item = (TItem)items[(lookup & ID<TItem>.Mask) >> ID<TItem>.Shift]!;
            return true;
        }

        /// <summary>
        /// Retrieves <typeparamref name="TItem"/> from the list or returns <c>null</c>.
        /// </summary>
        public readonly TItem? GetSafe<TItem>() where TItem : class, TBase => (flags & ID<TItem>.BitFlag) switch
        {
            0 => default,
            _ => items[(lookup & ID<TItem>.Mask) >> ID<TItem>.Shift] as TItem,
        };

        /// <summary>
        /// Retrieves <typeparamref name="TItem"/> from the list or throws.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><typeparamref name="TItem"/> was not registered.</exception>
        /// <exception cref="InvalidCastException"><typeparamref name="TItem"/> was not registered, or already registered under a different type.</exception>
        public readonly TItem Get<TItem>() where TItem : class, TBase => (TItem)items[(lookup & ID<TItem>.Mask) >> ID<TItem>.Shift]!;

        /// <summary>
        /// Retrieves index on which <typeparamref name="TItem"/> is located, or <c>-1</c> if <typeparamref name="TItem"/> is not listed.
        /// </summary>
        public readonly int IndexOf<TItem>() where TItem : class, TBase => (flags & ID<TItem>.BitFlag) switch
        {
            0 => -1,
            _ => (int)((lookup & ID<TItem>.Mask) >> ID<TItem>.Shift),
        };

        /// <summary>
        /// Adds <typeparamref name="TItem"/> instance to the list.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
        /// <returns>
        /// <c>true</c> if item was added.
        /// <c>false</c> if another item instance was already listed.
        /// </returns>
        public bool Add<TItem>(TItem item) where TItem : class, TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if ((flags & ID<TItem>.BitFlag) != 0)
            {
                return false;
            }

            items[stored] = item;
            lookup |= (ulong)stored << ID<TItem>.Shift;
            flags |= ID<TItem>.BitFlag;
            stored++;
            return true;
        }

        /// <summary>
        /// Adds <typeparamref name="TItem"/> instance to the list, or replaces an existing one under the same type.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
        public void AddOrReplace<TItem>(TItem item) where TItem : class, TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if ((flags & ID<TItem>.BitFlag) != 0)
            {
                Remove<TItem>();
            }

            items[stored] = item;
            lookup |= (ulong)stored << ID<TItem>.Shift;
            flags |= ID<TItem>.BitFlag;
            stored++;
        }

        /// <summary>
        /// Inserts at <paramref name="item"/> as given <paramref name="index"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if item was inserted.
        /// <c>false</c> if another item instance was already listed.
        /// </returns>
        public bool Insert<TItem>(int index, TItem item) where TItem : class, TBase
        {
            if ((flags & ID<TItem>.BitFlag) != 0)
            {
                return false;
            }

            // Updates lookup.
            // Note: It *might* be possible to optimize it with some bit operations, but I couldn't fully figure it out at the time.
            for (int i = 0; i < BaseSet.ItemLimit; i++)
            {
                if ((flags & (1u << i)) == 0)
                {
                    continue;
                }

                int position = i * BaseSet.MaskBits;
                int location = (int)((lookup >> position) & 0xFuL);
                if (location >= index)
                {
                    location++; // Practically, will never exceed [0-15] range due to its environment.
                    lookup &= ~(0xFuL << position);
                    lookup |= (ulong)location << position;
                }
            }

            // Inserts the item.
            ResizeIfNeeded(ref items, stored + 1);
            Array.Copy(items, index, items, index + 1, stored - index);
            items[index] = item;
            flags |= ID<TItem>.BitFlag;
            lookup |= (ulong)index << ID<TItem>.Shift;
            stored++;
            return true;
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <returns>
        /// <c>true</c> if item previously registered and was now removed.
        /// <c>false</c> if item was not registered in the first place.
        /// </returns>
        public bool Remove<TItem>() where TItem : class, TBase
        {
            if ((flags & ID<TItem>.BitFlag) == 0)
            {
                return false;
            }

            RemoveAt((int)((lookup & ID<TItem>.Mask) >> ID<TItem>.Shift));
            return true;
        }

        /// <summary>
        /// Removes an item of a type <typeparamref name="TItem"/> from the list.
        /// </summary>
        /// <returns>
        /// <c>true</c> if item previously registered and was now removed.
        /// <c>false</c> if item was not registered in the first place.
        /// </returns>
        public bool Remove<TItem>(TItem item) where TItem : class, TBase
        {
            if ((flags & ID<TItem>.BitFlag) == 0)
            {
                return false;
            }

            int index = (int)((lookup & ID<TItem>.Mask) >> ID<TItem>.Shift);
            if (items[index] != item)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes an item of a type <typeparamref name="TItem"/>, and return a removed instance as <paramref name="item"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if item previously registered and was now removed.
        /// <c>false</c> if item was not registered in the first place.
        /// </returns>
        public bool Remove<TItem>([NotNullWhen(true)] out TItem? item) where TItem : class, TBase
        {
            if ((flags & ID<TItem>.BitFlag) == 0)
            {
                item = default;
                return false;
            }

            RemoveAt((int)((lookup & ID<TItem>.Mask) >> ID<TItem>.Shift), out TBase result);
            item = (TItem)result;
            return true;
        }

        /// <summary>
        /// Removes item at provided <paramref name="index"/>.
        /// </summary>
        public void RemoveAt(int index)
        {
            Array.Copy(items, index + 1, items, index, stored - index - 1);

            // Updates lookup.
            for (int i = 0; i < BaseSet.ItemLimit; i++)
            {
                if ((flags & (1u << i)) == 0)
                {
                    continue;
                }

                int position = i * BaseSet.MaskBits;
                int location = (int)((lookup >> position) & 0xFuL);
                if (location == index)
                {
                    // Resets index in a lookup to 0.
                    lookup &= ~(0xFuL << position);
                    flags &= (ushort)~(1u << i);
                }
                else if (location > index)
                {
                    location--;
                    lookup &= ~(0xFuL << position);
                    lookup |= (ulong)location << position;
                }
            }

            stored--;
        }

        /// <summary>
        /// Removes item at provided <paramref name="index"/>, and returns removed <paramref name="item"/>.
        /// </summary>
        public void RemoveAt(int index, out TBase item)
        {
            item = items[index]!;
            Array.Copy(items, index + 1, items, index, stored - index - 1);

            // Updates lookup.
            for (int i = 0; i < BaseSet.ItemLimit; i++)
            {
                if ((flags & (1u << i)) == 0)
                {
                    continue;
                }

                int position = i * BaseSet.MaskBits;
                int location = (int)((lookup >> position) & 0xFuL);
                if (location == index)
                {
                    // Resets index in a lookup to 0.
                    lookup &= ~(0xFuL << position);
                    flags &= (ushort)~(1u << i);
                }
                else if (location > index)
                {
                    location--;
                    lookup &= ~(0xFuL << position);
                    lookup |= (ulong)location << position;
                }
            }

            stored--;
        }

        /// <summary>
        /// Clears all items from this list.
        /// </summary>
        public void Clear()
        {
            Array.Fill(items, default);
            lookup = 0uL;
            stored = 0;
            flags = 0;
        }

        /// <summary>
        /// Copies all items from this list to a provided <paramref name="array"/>.
        /// </summary>
        public readonly void CopyTo(TBase[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

        /// <summary>
        /// Retrieves enumerator for iterating over the entire list.
        /// </summary>
        public readonly Enumerator GetEnumerator() => new(items, stored);

        /// <summary>
        /// Struct-based, zero heap allocation enumerator for iterating through the entire list.
        /// </summary>
        public struct Enumerator(TBase?[] items, int stored)
        {
            readonly TBase?[] items = items;
            readonly int stored = stored;
            int index = -1;

            /// <summary>
            /// Retrieves current item from the internal array.
            /// </summary>
            public readonly TBase Current => index >= stored ? default! : items[index]!;

            /// <summary>
            /// Moves the enumerator forward by one index.
            /// </summary>
            public bool MoveNext() => ++index < stored;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResizeIfNeeded(ref TBase?[] items, int targetSize)
        {
            if (items.Length < targetSize)
            {
                Array.Resize(ref items, targetSize);
            }
        }
    }
}
