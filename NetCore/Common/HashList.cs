using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NetCore.Common
{
    /// <summary>
    /// Exception for handling when you use <see cref="HashList{T}.GetLookup{TItem}"/>
    /// with TItem matching a base type of the <see cref="HashList{T}"/>.
    /// </summary>
    /// <param name="type">Target item type.</param>
    public sealed class InvalidLookupTypeException(Type type)
        : Exception($"Cannot use ({type.Name}) as a type for a Lookup, as it matches the type of a HashList. Use HashList directly instead.");

    /// <summary>
    /// Thrown when you try to register a type, which does not inherit a base type, used for <see cref="HashList{TBase}"/>.
    /// </summary>
    /// <param name="itemType">Type of an item.</param>
    /// <param name="baseType">Base type from <see cref="HashList{TBase}"/>.</param>
    public sealed class InvalidIndexingTypeException(Type itemType, Type baseType)
        : Exception($"Provided type ({itemType.Name}) does not inherit a base type ({baseType.Name}), and cannot be stored in a HashList.");

    /// <summary>
    /// Thrown when a specific item you were looking for was not found in the <see cref="HashList{TBase}"/>.
    /// </summary>
    /// <param name="itemType">Type of an item.</param>
    public sealed class ItemNotFoundException(Type itemType)
        : Exception($"Item of a type ({itemType.Name}) is not found in a HashList.");

    /// <summary>
    /// (Not thread-safe! Used must ensure safety)
    /// Struct-based hash list, for storing unique strong-typed items.
    /// Optimized to be a lot faster than <see cref="Dictionary{TKey, TValue}"/> using CRTP.
    /// </summary>
    /// <remarks>
    /// <para>Supports up to 65535 items. Throws an <see cref="OverflowException"/> on attempt to register more.</para>
    /// Internally, encodes invalid indexes as '0' instead of '-1' and such.
    /// Because of that, mapping resizing will not happen when adding 17th item, but a 16th instead.
    /// Another resize will happen when adding 256th item instead of 257th.
    /// </remarks>
    /// <typeparam name="TBase">Base type of an item.</typeparam>
    /// Note: Implementation is x86 compatible.
    /// TODO: Use Dictionary as a fallback for weak-type indexing.
    /// TODO: Use remaining 4 bytes the struct can hold without resizing, to encode a load factor.
    public struct HashList<TBase>
    {
        static class Indexing
        {
            // TODO: Consider implementing a custom data structure for weakly-typed indexing as well, instead of a dictionary.
            static readonly Dictionary<RuntimeTypeHandle, ushort> Indexes = [];
            static ushort Iterator = 0;
            /// <remarks>
            /// Doesn't check if <paramref name="type"/> inherits <typeparamref name="TBase"/>.
            /// Use <see cref="TryGetOrder(Type, out ushort)"/> to check if it inherits <typeparamref name="TBase"/>.
            /// </remarks>
            public static ushort GetOrderUnchecked(Type type)
            {
                var handle = type.TypeHandle;
                if (!Indexes.TryGetValue(type.TypeHandle, out ushort order))
                {
                    Indexes[handle] = order = checked(Iterator++);
                }

                return order;
            }
            /// <returns>
            /// <c>false</c> if <paramref name="type"/> does not inherit <typeparamref name="TBase"/>.
            /// <c>true</c> if it does, and an <paramref name="order"/> was provided.
            /// </returns>
            public static bool TryGetOrder(Type type, out ushort order)
            {
                var handle = type.TypeHandle;
                if (!Indexes.TryGetValue(type.TypeHandle, out order))
                {
                    if (!typeof(TBase).IsAssignableFrom(type))
                    {
                        return false;
                    }

                    Indexes[handle] = order = checked(Indexing.Iterator++);
                }

                return true;
            }
        }

        static class ID<TItem> where TItem : TBase
        {
            /// <summary>
            /// Initialization order of this item.
            /// </summary>
            public static readonly ushort Order = Indexing.GetOrderUnchecked(typeof(TItem));
            /// <summary>
            /// Flag region describing this item.
            /// </summary>
            public static readonly ushort Region = (ushort)(Order >> 2);
            public static readonly uint Mask = (Order & 0b11) switch
            {
                0 => ItemMask1,
                1 => ItemMask2,
                2 => ItemMask3,
                3 => ItemMask4,
                _ => throw new SwitchExpressionException(),
            };
            public static readonly uint Flag = (Order & 0b11) switch
            {
                0 => ItemFlag1,
                1 => ItemFlag2,
                2 => ItemFlag3,
                3 => ItemFlag4,
                _ => throw new SwitchExpressionException(),
            };
            public static readonly int Shift = (Order & 0b11) switch
            {
                0 => ItemShift1,
                1 => ItemShift2,
                2 => ItemShift3,
                3 => ItemShift4,
                _ => throw new SwitchExpressionException(),
            };
        }

        /// <summary>
        /// Lookup struct for managing items of a specific type in a base <see cref="HashList{T}"/>.
        /// </summary>
        /// <typeparam name="TFilter">Type for which to filter.</typeparam>
        /// <param name="list"></param>
        public struct Lookup<TFilter>(ref HashList<TBase> list) where TFilter : TBase
        {
            private byte[] lookups = [];
            private readonly ref HashList<TBase> List = list;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// Note (for the future): By using ulong it's possible to change the structure:
        /// Layout: [Offset - 32 bits][flag+position - 4 bits][f+p][f+p][f+p][f+p][f+p][f+p][f+p]
        /// Size:   [32bits][4bits][4bits][4bits][4bits][4bits][4bits][4bits][4bits]
        /// Features: Max item amount - 4294967296 (<seealso cref="uint.MaxValue"/> + 1).
        /// 
        /// Current system (uint-based):
        /// Layout: [Offset - 16 bits][unused - 4 bits][flag+position - 3 bits][flag+position][flag+position][flag+position]
        /// Size:   [16bits][4bits][3bits][3bits][3bits][3bits] = 32bits (7bits per item)(technically 8bits per item).
        /// Features: Max item amount - 65536 (<seealso cref="ushort.MaxValue"/> + 1).
        const uint ItemMask1 = 0b000_000_000_011u; // shift: 0
        const uint ItemMask2 = 0b000_000_011_000u; // shift: 3
        const uint ItemMask3 = 0b000_011_000_000u; // shift: 6
        const uint ItemMask4 = 0b011_000_000_000u; // shift: 9
        const uint ItemFlag1 = 0b000_000_000_100u;
        const uint ItemFlag2 = 0b000_000_100_000u;
        const uint ItemFlag3 = 0b000_100_000_000u;
        const uint ItemFlag4 = 0b100_000_000_000u;
        const int ShiftToOrigin = 16;
        const int ItemShift1 = 0;
        const int ItemShift2 = 3;
        const int ItemShift3 = 6;
        const int ItemShift4 = 9;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private uint stored;
        private uint[] flags;
        private TBase[] items;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if <see cref="HashList{TBase}"/> has <typeparamref name="TItem"/> defined.
        /// </summary>
        public readonly bool Has<TItem>() where TItem : TBase
        {
            if (ID<TItem>.Region >= flags.Length)
            {
                return false; // Handles out-of-range.
            }

            return (flags[ID<TItem>.Region] & ID<TItem>.Flag) != 0;
        }

        /// <summary>
        /// Checks if a given item type is defined in a list.
        /// </summary>
        /// <param name="itemType">Type of an target item to check for.</param>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// </remarks>
        public readonly bool Has(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out ushort order))
            {
                return Has(order);
            }

            return false;
        }

        /// <inheritdoc cref="Has(Type)"/>
        readonly bool Has(int order)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to retrieve an <typeparamref name="TItem"/> from this <see cref="HashList{TBase}"/>
        /// </summary>
        /// <typeparam name="TItem">Item, inheriting <typeparamref name="TBase"/> to look for.</typeparam>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found and was provided.
        /// <c>false</c> otherwise.
        /// </returns>
        public readonly bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TBase
        {
            if (ID<TItem>.Region >= this.flags.Length)
            {
                item = default;
                return false; // Handles out-of-range.
            }

            uint flags = this.flags[ID<TItem>.Region];
            if ((flags & ID<TItem>.Flag) == 0)
            {
                item = default;
                return false; // Item is not defined.
            }

            item = (TItem)items[(flags >> ShiftToOrigin) + ((flags & ID<TItem>.Mask) >> ID<TItem>.Shift)]!;
            return true;
        }

        /// <summary>
        /// Attempts to retrieve an <paramref name="item"/> of a given <paramref name="itemType"/> from this <see cref="HashList{TBase}"/>
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found and was provided.
        /// <c>false</c> otherwise.
        /// </returns>
        public readonly bool TryGet(Type itemType, [NotNullWhen(true)] out TBase? item)
        {
            if (Indexing.TryGetOrder(itemType, out ushort order))
            {
                return TryGet(order, out item);
            }

            item = default;
            return false;
        }

        /// <inheritdoc cref="TryGet(Type, out TBase)"/>.
        readonly bool TryGet(int order, [NotNullWhen(true)] out TBase? item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves an item of a type <typeparamref name="TItem"/> from this <see cref="HashList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// </summary>
        /// <typeparam name="TItem">Item type to look for.</typeparam>
        /// <exception cref="KeyNotFoundException"><typeparamref name="TItem"/> is not defined in this list.</exception>
        public readonly TItem Get<TItem>() where TItem : TBase
        {
            if (ID<TItem>.Region >= this.flags.Length)
            {
                throw new ItemNotFoundException(typeof(TItem));
            }

            uint flags = this.flags[ID<TItem>.Region];
            if ((flags & ID<TItem>.Flag) == 0)
            {
                throw new ItemNotFoundException(typeof(TItem));
            }

            return (TItem)items[(flags >> ShiftToOrigin) + ((flags & ID<TItem>.Mask) >> ID<TItem>.Shift)]!;
        }

        /// <summary>
        /// Retrieves an item of a given <paramref name="itemType"/> from this <see cref="HashList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// </remarks>
        /// <param name="itemType">Item type to look for.</param>
        /// <exception cref="KeyNotFoundException">Item of a given <paramref name="itemType"/> is not defined in this list.</exception>
        public readonly TBase Get(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out ushort order))
            {
                return Get(order);
            }

            throw new ItemNotFoundException(itemType);
        }

        /// <inheritdoc cref="Get(Type)"/>
        readonly TBase Get(int order)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves an item of a type <typeparamref name="TItem"/> from this <see cref="HashList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// </summary>
        /// <typeparam name="TItem">Item type to look for.</typeparam>
        /// <exception cref="KeyNotFoundException"><typeparamref name="TItem"/> is not defined in this list.</exception>
        public readonly TItem? GetSafe<TItem>() where TItem : TBase
        {
            if (ID<TItem>.Region >= this.flags.Length)
            {
                return default;
            }

            uint flags = this.flags[ID<TItem>.Region];
            if ((flags & ID<TItem>.Flag) == 0)
            {
                return default;
            }

            return (TItem)items[(flags >> ShiftToOrigin) + ((flags & ID<TItem>.Mask) >> ID<TItem>.Shift)]!;
        }

        /// <summary>
        /// Retrieves an item of a given <paramref name="itemType"/> from this <see cref="HashList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// </remarks>
        /// <param name="itemType">Item type to look for.</param>
        /// <exception cref="KeyNotFoundException">Item of a given <paramref name="itemType"/> is not defined in this list.</exception>
        public readonly TBase? GetSafe(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out ushort order))
            {
                return GetSafe(order);
            }

            return default;
        }

        /// <inheritdoc cref="GetSafe(Type)"/>
        readonly TBase? GetSafe(int order)
        {
            throw new NotImplementedException();
        }




        /// <summary>
        /// Attempts to add an item to the list.
        /// </summary>
        /// <typeparam name="TItem">Item type to register.</typeparam>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <returns>
        /// <c>true</c> if added successfully.
        /// <c>false</c> if item already exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        public bool Add<TItem>(TItem item) where TItem : TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (ID<TItem>.Region >= this.flags.Length)
            {
                // Too small - resize:
                Array.Resize(ref items, ID<TItem>.Region + 1);
            }

            ref uint flags = ref this.flags[ID<TItem>.Region];
            if ((flags & ID<TItem>.Flag) != 0)
            {
                return false; // Item already exist.
            }

            throw new NotImplementedException();
        }





        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>





            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Static Methods
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public static Lookup<TItem> GetLookup<TItem>(ref HashList<TBase> list) where TItem : TBase
        {
            if (typeof(TItem) == typeof(TBase))
            {
                throw new InvalidLookupTypeException(typeof(TItem));
            }

            return new Lookup<TItem>(ref list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetModeCapacity(PackingMode mode) => mode switch
        {
            PackingMode.Size0 => 0,
            PackingMode.Size1to8 => Packing1to8.Capacity,
            PackingMode.Size9to128 => Packing9to128.Capacity,
            PackingMode.Size129to32768 => Packing129to32768.Capacity,
            _ => Packing32769to2147483647.Capacity,
        };
    }

    /// <summary>
    /// Useful extension methods when working with <see cref="HashList{T}"/>.
    /// </summary>
    public static class HashListExtensions
    {
        /// <inheritdoc cref="HashList{T}.GetLookup{TItem}(ref HashList{T})"/>
        public static HashList<T>.Lookup<TItem> GetLookup<T, TItem>(this ref HashList<T> list) where TItem : T
        {
            return HashList<T>.GetLookup<TItem>(ref list);
        }
    }
}
