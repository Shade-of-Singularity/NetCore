using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
    /// <summary>
    /// Exception for handling when you use <see cref="CRTPList{T}.GetLookup{TItem}"/>
    /// with TItem matching a base type of the <see cref="CRTPList{T}"/>.
    /// </summary>
    /// <param name="type">Target item type.</param>
    public sealed class InvalidLookupTypeException(Type type)
        : Exception($"Cannot use ({type.Name}) as a type for a Lookup, as it matches the type of a HashList. Use HashList directly instead.");

    /// <summary>
    /// Thrown when you try to register a type, which does not inherit a base type, used for <see cref="CRTPList{TBase}"/>.
    /// </summary>
    /// <param name="itemType">Type of an item.</param>
    /// <param name="baseType">Base type from <see cref="CRTPList{TBase}"/>.</param>
    public sealed class InvalidIndexingTypeException(Type itemType, Type baseType)
        : Exception($"Provided type ({itemType.Name}) does not inherit a base type ({baseType.Name}), and cannot be stored in a HashList.");

    /// <summary>
    /// Thrown when a specific item you were looking for was not found in the <see cref="CRTPList{TBase}"/>.
    /// </summary>
    /// <param name="itemType">Type of an item.</param>
    public sealed class ItemNotFoundException(Type itemType)
        : Exception($"Item of a type ({itemType.Name}) is not found in a HashList.");

    /// <summary>
    /// Thrown when you try to use an insert item method in a list, using item of a type, that was already defined.
    /// </summary>
    /// <param name="itemType">Type of an item that was attempted to be added.</param>
    public sealed class ItemAlreadyExistException(Type itemType)
        : Exception($"Item of a type ({itemType.Name}) is already defined in a HashList. Duplicates are not allowed.");

    /// <summary>
    /// Thrown when you attempt to add/insert/remove an item, providing type which doesn't match an actual type of the item.
    /// </summary>
    /// <param name="providedType">Provided type.</param>
    /// <param name="realType">Type of the real item instance.</param>
    internal sealed class ItemTypeMismatchException(Type providedType, Type realType)
        : Exception($"Specified item type ({providedType.Name}) and type of a real item ({realType.Name}) does not match.");

    /// <summary>
    /// Thrown when you (somehow) exceed a maximum capacity of a <see cref="CRTPList{TBase}"/> - <see cref="int.MaxValue"/>.
    /// </summary>
    /// <remarks>
    /// Simply how.
    /// </remarks>
    public sealed class CRTPListTooLargeException()
        : Exception($"Attempted to resize a HashList beyond a current limit of ({CRTPLists.MaxCapacity}).");

    /// <summary>
    /// Stores some trivia about <see cref="CRTPList{TBase}"/>.
    /// </summary>
    public static class CRTPLists
    {
        /// <summary>
        /// Max possible capacity an <see cref="CRTPList{TBase}"/> can have.
        /// </summary>
        public const int MaxCapacity = int.MaxValue;
    }

    /// <summary>
    /// (Not thread-safe!)
    /// CRTP list, for storing unique strong-typed items.
    /// Optimized to be a lot faster than <see cref="Dictionary{TKey, TValue}"/> using CRTP.
    /// </summary>
    /// <typeparam name="TBase">Base type of an item.</typeparam>
    /// Note: Implementation is x86 compatible.
    /// Note: Was turned into a class to support <see cref="Lookup{TFilter}"/> structs.
    /// Note (for maintainers):
    ///  All <see langword="default"/> <see langword="case"/>s in <see langword="switch"/> statements return an exception.
    ///  
    ///  This was made not for convention, or stuff like that - but for branch prediction.
    ///  If you use <![CDATA["3 or _ => ..."]]> or similar syntax - <see langword="switch"/> gets optimized to a jump-table, 
    ///  but the state "3" is merged with a <see langword="default"/> case, both in Debug and Release modes (as of 2Q, 2026).
    ///  Jump-tables are optimized by checking if value falls within a specific range:
    ///  - If range check succeeds - it will jump to a case (e.g. 0, 1, 2, ...).
    ///  - But if range check fails - it will jump to a default case.
    ///  This might create branch unpredictability with highest switch case.
    ///  Thus - we manually define highest case, and provide *literally anything* as a fallback option.
    ///  This pattern enforces branch prediction better.
    ///  - - -
    ///  TL;DR: If you use default case with *any other value* in a switch statement - it creates unpredictable jump-table (if at all).
    ///   That's why we define default cases separately, and will them with garbage (<see cref="SwitchExpressionException"/>s, in this case). 
    ///  
    /// TODO: Consider providing a thread-safe alternative around v1 release.
    /// TODO: Test for edge-cases: <see cref="sbyte.MaxValue"/>, <see cref="short.MaxValue"/> and <see cref="int.MaxValue"/>, and update the constants.
    /// TODO: Add IndexOf methods.
    /// TODO: Rework growth strategy by introducing a load factor.
    public sealed partial class CRTPList<TBase> where TBase : class
    {
        /// Note (for maintainers): exposed as <see langword="internal"/> for usage in <see cref="CRTPListExtensions"/>.
        internal static class Indexing
        {
            // TODO: Consider implementing a custom data structure for weakly-typed indexing as well, instead of a dictionary.
            static readonly Dictionary<RuntimeTypeHandle, int> Indexes = [];
            static int Iterator = 0;
            /// <remarks>
            /// Doesn't check if <paramref name="type"/> inherits <typeparamref name="TBase"/>.
            /// Use <see cref="TryGetOrder(Type, out int)"/> to check if it inherits <typeparamref name="TBase"/>.
            /// </remarks>
            public static int GetOrderUnchecked(Type type)
            {
                lock (Indexes)
                {
                    var handle = type.TypeHandle;
                    if (!Indexes.TryGetValue(type.TypeHandle, out int order))
                    {
                        Indexes[handle] = order = checked(Iterator++);
                    }

                    return order;
                }
            }
            /// <returns>
            /// <c>false</c> if <paramref name="type"/> does not inherit <typeparamref name="TBase"/>.
            /// <c>true</c> if it does, and an <paramref name="order"/> was provided.
            /// </returns>
            public static bool TryGetOrder(Type type, out int order)
            {
                lock (Indexes)
                {
                    var handle = type.TypeHandle;
                    if (!Indexes.TryGetValue(type.TypeHandle, out order))
                    {
                        if (!typeof(TBase).IsAssignableFrom(type))
                        {
                            return false;
                        }

                        Indexes[handle] = order = checked(Iterator++);
                    }

                    return true;
                }
            }
        }

        /// Note (for maintainers): exposed as <see langword="internal"/> for usage in <see cref="CRTPListExtensions"/>.
        internal static class ID<TItem> where TItem : TBase
        {
            /// <summary>
            /// Initialization order of this item.
            /// </summary>
            /// <remarks>
            /// Initialization order might be different depending on the app, and as such - order should not be sent over the network.
            /// Assume that this value is never guaranteed to be either equal or different from the one on a remote host.
            /// </remarks>
            public static readonly int Order = Indexing.GetOrderUnchecked(typeof(TItem));
            /// <summary>
            /// Region in which <see cref="Flag"/> resides.
            /// </summary>
            public static readonly int FlagRegion = Order >> 5;
            /// <summary>
            /// Flag used for encoding in <see cref="Lookup{TFilter}"/>.
            /// </summary>
            public static readonly uint Flag = 1u << (Order & 0b11111);
        }

        /// Note (for maintainers): exposed as <see langword="internal"/> for usage in <see cref="Lookup{TFilter}.Enumerator"/>.
        internal enum PackingMode : byte
        {
            /// <summary>
            /// Completely empty list. Does not store or encode any values.
            /// </summary>
            Size0,
            /// <summary>
            /// Uses 8bits per entry. <see cref="uint"/> encodes up to 4 such entries.
            /// </summary>
            SizeByte,
            /// <summary>
            /// Uses 16bits per entry. <see cref="uint"/> encodes up to 2 such entries.
            /// </summary>
            SizeUShort,
            /// <summary>
            /// Uses 32bits per entry. <see cref="uint"/> encodes only one such entry.
            /// </summary>
            /// <remarks>
            /// If you ever reach this point - let us know. You will be the first, LoL :D
            /// </remarks>
            SizeUInt,
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        static class PackingByte
        {
            public const int Capacity = sbyte.MaxValue + 1;

            public const uint ItemMask1 = 0b00000000_00000000_00000000_11111111u;
            public const uint ItemMask2 = 0b00000000_00000000_11111111_00000000u;
            public const uint ItemMask3 = 0b00000000_11111111_00000000_00000000u;
            public const uint ItemMask4 = 0b11111111_00000000_00000000_00000000u;

            public const uint ItemFlag1 = 0b00000000_00000000_00000000_10000000u;
            public const uint ItemFlag2 = 0b00000000_00000000_10000000_00000000u;
            public const uint ItemFlag3 = 0b00000000_10000000_00000000_00000000u;
            public const uint ItemFlag4 = 0b10000000_00000000_00000000_00000000u;

            public const uint ItemValue1 = 0b00000000_00000000_00000000_01111111u;
            public const uint ItemValue2 = 0b00000000_00000000_01111111_00000000u;
            public const uint ItemValue3 = 0b00000000_01111111_00000000_00000000u;
            public const uint ItemValue4 = 0b01111111_00000000_00000000_00000000u;

            public const byte ByteMask = 0b11111111;
            public const byte ByteFlag = 0b10000000;
            public const byte ByteValue = 0b01111111;

            public const int ItemShift1 = 0;
            public const int ItemShift2 = 8;
            public const int ItemShift3 = 16;
            public const int ItemShift4 = 24;
        }

        static class PackingUShort
        {
            public const int Capacity = short.MaxValue + 1;
            public const uint ItemMask1 = 0b0000000000000000_1111111111111111u;
            public const uint ItemMask2 = 0b1111111111111111_0000000000000000u;

            public const uint ItemFlag1 = 0b0000000000000000_1000000000000000u;
            public const uint ItemFlag2 = 0b1000000000000000_0000000000000000u;

            public const uint ItemValue1 = 0b0000000000000000_0111111111111111u;
            public const uint ItemValue2 = 0b0111111111111111_0000000000000000u;

            public const ushort UShortMask = 0b1111111111111111;
            public const ushort UShortFlag = 0b1000000000000000;
            public const ushort UShortValue = 0b0111111111111111;

            public const int ItemShift1 = 0;
            public const int ItemShift2 = 16;
        }

        static class PackingUInt
        {
            public const int Capacity = int.MaxValue;
            public const uint ItemMask = 0b11111111111111111111111111111111u;
            public const uint ItemFlag = 0b10000000000000000000000000000000u;
            public const uint ItemValue = 0b01111111111111111111111111111111u;
            public const int ItemShift = 0;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Amount of items stored in this <see cref="CRTPList{TBase}"/>.
        /// </summary>
        public int Count => stored;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Delegates
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <param name="item">If <see langword="null"/> - all items were removed.</param>
        /// <param name="added">When <paramref name="item"/> is not null - either item that was added or removed.</param>
        private delegate void OnItemChangedHandler(TBase? item, int order, bool added);




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                   Events
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private event OnItemChangedHandler? ItemChanged;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private int stored;
        private PackingMode mode;
        private uint[] flags;
        private TBase[] items;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default .ctor, specifying 0 initial capacity.
        /// </summary>
        public CRTPList()
        {
            mode = PackingMode.Size0;
            flags = [];
            items = [];
        }
        /// <summary>
        /// .ctor for <see cref="CRTPList{TBase}"/>, allowing you to specify an initial capacity.
        /// </summary>
        public CRTPList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            switch (capacity)
            {
                case 0:
                    mode = PackingMode.Size0;
                    flags = [];
                    items = [];
                    break;

                case <= PackingByte.Capacity:
                    mode = PackingMode.SizeByte;
                    flags = new uint[(capacity + 3) / 4]; // Division with rounding up.
                    items = new TBase[capacity];
                    break;

                case <= PackingUShort.Capacity:
                    mode = PackingMode.SizeUShort;
                    flags = new uint[(capacity + 1) / 2]; // Division with rounding up.
                    items = new TBase[capacity];
                    break;
                default:
                    mode = PackingMode.SizeUInt;
                    flags = new uint[capacity];
                    items = new TBase[capacity];
                    break;
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if <see cref="CRTPList{TBase}"/> has <typeparamref name="TItem"/> defined.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        public bool Has<TItem>() where TItem : TBase => HasInternal(ID<TItem>.Order);
        /// <summary>
        /// Checks if a given item type is defined in a list.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <param name="itemType">Type of an target item to check for.</param>
        public bool Has(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return HasInternal(order);
            }

            return false;
        }

        /// <inheritdoc cref="Has(Type)"/>
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal bool HasInternal(int order)
        {
            int flagsIndex;
            switch (mode)
            {
                case PackingMode.Size0: return false;
                case PackingMode.SizeByte:
                    flagsIndex = order >> 2;
                    if (flagsIndex >= flags.Length)
                        return false; // Handles out-of-bound.

                    return (flags[flagsIndex] & (order & 0b11) switch
                    {
                        0 => PackingByte.ItemFlag1,
                        1 => PackingByte.ItemFlag2,
                        2 => PackingByte.ItemFlag3,
                        3 => PackingByte.ItemFlag4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }) != 0;

                case PackingMode.SizeUShort:
                    flagsIndex = order >> 1;
                    if (flagsIndex >= flags.Length)
                        return false; // Handles out-of-bound.

                    return (flags[flagsIndex] & (order & 0b1) switch
                    {
                        0 => PackingUShort.ItemFlag1,
                        1 => PackingUShort.ItemFlag2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    }) != 0;

                case PackingMode.SizeUInt:
                    flagsIndex = order;
                    if (flagsIndex >= flags.Length)
                        return false; // Handles out-of-bound.

                    return (flags[flagsIndex] & PackingUInt.ItemFlag) != 0;

                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Attempts to retrieve an <typeparamref name="TItem"/> from this <see cref="CRTPList{TBase}"/>
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <typeparam name="TItem">Item, inheriting <typeparamref name="TBase"/> to look for.</typeparam>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found and was provided.
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TBase => TryGetInternal(ID<TItem>.Order, out item);
        /// <summary>
        /// Attempts to retrieve an <paramref name="item"/> of a given <paramref name="itemType"/> from this <see cref="CRTPList{TBase}"/>
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found and was provided.
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryGet(Type itemType, [NotNullWhen(true)] out TBase? item)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return TryGetInternal(order, out item);
            }

            item = default;
            return false;
        }

        /// <inheritdoc cref="TryGet(Type, out TBase)"/>.
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal bool TryGetInternal<TItem>(int order, [NotNullWhen(true)] out TItem? item) where TItem : TBase
        {
            int flagsIndex;
            uint flags;
            switch (mode)
            {
                case PackingMode.Size0: item = default; return false;
                case PackingMode.SizeByte:
                    flagsIndex = order >> 2;
                    if (flagsIndex >= this.flags.Length)
                    {
                        item = default;
                        return false; // Handles out-of-bound.
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & (order & 0b11) switch
                    {
                        0 => PackingByte.ItemFlag1,
                        1 => PackingByte.ItemFlag2,
                        2 => PackingByte.ItemFlag3,
                        3 => PackingByte.ItemFlag4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }) == 0)
                    {
                        item = default;
                        return false; // Item not found.
                    }

                    return TryGetInternalUnsafeByte(order, out item, flags);

                case PackingMode.SizeUShort:
                    flagsIndex = order >> 1;
                    if (flagsIndex >= this.flags.Length)
                    {
                        item = default;
                        return false; // Handles out-of-bound.
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & (order & 0b1) switch
                    {
                        0 => PackingUShort.ItemFlag1,
                        1 => PackingUShort.ItemFlag2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    }) == 0)
                    {
                        item = default;
                        return false; // Item not found.
                    }

                    return TryGetInternalUnsafeUShort(order, out item, flags);

                case PackingMode.SizeUInt:
                    flagsIndex = order;
                    if (flagsIndex >= this.flags.Length)
                    {
                        item = default;
                        return false; // Handles out-of-bound.
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & PackingUInt.ItemFlag) == 0)
                    {
                        item = default;
                        return false; // Item not found.
                    }

                    return TryGetInternalUnsafeUInt(out item, flags);

                default: throw new SwitchExpressionException(mode);
            }
        }

        /// Note (for maintainers): exposed as <see langword="internal"/> for usage in <see cref="Lookup{TFilter}"/>.
        internal bool TryGetInternalUnsafeByte<TItem>(int order, out TItem? item, uint flags) where TItem : TBase
        {
            if (items[(order & 0b11) switch
            {
                0 => (flags & PackingByte.ItemValue1) >> PackingByte.ItemShift1,
                1 => (flags & PackingByte.ItemValue2) >> PackingByte.ItemShift2,
                2 => (flags & PackingByte.ItemValue3) >> PackingByte.ItemShift3,
                3 => (flags & PackingByte.ItemValue4) >> PackingByte.ItemShift4,
                _ => throw new SwitchExpressionException(order & 0b11),
            }] is TItem result1)
            {
                item = result1;
                return true;
            }

            item = default;
            return false;
        }

        /// Note (for maintainers): exposed as <see langword="internal"/> for usage in <see cref="Lookup{TFilter}"/>.
        internal bool TryGetInternalUnsafeUShort<TItem>(int order, out TItem? item, uint flags) where TItem : TBase
        {
            if (items[(order & 0b1) switch
            {
                0 => (flags & PackingUShort.ItemValue1) >> PackingUShort.ItemShift1,
                1 => (flags & PackingUShort.ItemValue2) >> PackingUShort.ItemShift2,
                _ => throw new SwitchExpressionException(order & 0b1),
            }] is TItem result2)
            {
                item = result2;
                return true;
            }

            item = default;
            return false;
        }

        /// Note (for maintainers): exposed as <see langword="internal"/> for usage in <see cref="Lookup{TFilter}"/>.
        internal bool TryGetInternalUnsafeUInt<TItem>(out TItem? item, uint flags) where TItem : TBase
        {
            if (items[(flags & PackingUInt.ItemValue) >> PackingUInt.ItemShift] is TItem result3)
            {
                item = result3;
                return true;
            }

            item = default;
            return false;
        }

        /// <summary>
        /// Retrieves an item of a type <typeparamref name="TItem"/> from this <see cref="CRTPList{TBase}"/>.
        /// </summary>
        /// <remarks>
        /// <para>Throws a <see cref="KeyNotFoundException"/> if <typeparamref name="TItem"/> is not defined in this list.</para>
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <typeparam name="TItem">Item type to look for.</typeparam>
        /// <exception cref="KeyNotFoundException"><typeparamref name="TItem"/> is not defined in this list.</exception>
        public TItem Get<TItem>() where TItem : TBase => GetInternal<TItem>(ID<TItem>.Order);
        /// <summary>
        /// Retrieves an item of a given <paramref name="itemType"/> from this <see cref="CRTPList{TBase}"/>.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Throws a <see cref="KeyNotFoundException"/> if item under given <paramref name="itemType"/> is not defined in this list.</para>
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <param name="itemType">Item type to look for.</param>
        /// <exception cref="KeyNotFoundException">Item under a given <paramref name="itemType"/> is not defined in this list.</exception>
        public TBase Get(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return GetInternal<TBase>(order);
            }

            throw new ItemNotFoundException(itemType);
        }

        /// <inheritdoc cref="Get(Type)"/>
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal TItem GetInternal<TItem>(int order) where TItem : TBase
        {
            int flagsIndex;
            uint flags;
            switch (mode)
            {
                case PackingMode.Size0: throw new ItemNotFoundException(typeof(TItem));
                case PackingMode.SizeByte:
                    flagsIndex = order >> 2;
                    if (flagsIndex >= this.flags.Length)
                    {
                        throw new ItemNotFoundException(typeof(TItem));
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & (order & 0b11) switch
                    {
                        0 => PackingByte.ItemFlag1,
                        1 => PackingByte.ItemFlag2,
                        2 => PackingByte.ItemFlag3,
                        3 => PackingByte.ItemFlag4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }) == 0)
                    {
                        throw new ItemNotFoundException(typeof(TItem));
                    }

                    return (TItem)items[(order & 0b11) switch
                    {
                        0 => (flags & PackingByte.ItemValue1) >> PackingByte.ItemShift1,
                        1 => (flags & PackingByte.ItemValue2) >> PackingByte.ItemShift2,
                        2 => (flags & PackingByte.ItemValue3) >> PackingByte.ItemShift3,
                        3 => (flags & PackingByte.ItemValue4) >> PackingByte.ItemShift4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }]!;

                case PackingMode.SizeUShort:
                    flagsIndex = order >> 1;
                    if (flagsIndex >= this.flags.Length)
                    {
                        throw new ItemNotFoundException(typeof(TItem));
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & (order & 0b1) switch
                    {
                        0 => PackingUShort.ItemFlag1,
                        1 => PackingUShort.ItemFlag2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    }) == 0)
                    {
                        throw new ItemNotFoundException(typeof(TItem));
                    }

                    return (TItem)items[(order & 0b1) switch
                    {
                        0 => (flags & PackingUShort.ItemValue1) >> PackingUShort.ItemShift1,
                        1 => (flags & PackingUShort.ItemValue2) >> PackingUShort.ItemShift2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    }]!;

                case PackingMode.SizeUInt:
                    flagsIndex = order;
                    if (flagsIndex >= this.flags.Length)
                    {
                        throw new ItemNotFoundException(typeof(TItem));
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & PackingUInt.ItemFlag) == 0)
                    {
                        throw new ItemNotFoundException(typeof(TItem));
                    }

                    return (TItem)items[(flags & PackingUInt.ItemValue) >> PackingUInt.ItemShift]!;

                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Retrieves an item of a type <typeparamref name="TItem"/> from this <see cref="CRTPList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to look for.</typeparam>
        /// <exception cref="KeyNotFoundException"><typeparamref name="TItem"/> is not defined in this list.</exception>
        public TItem? SafeGet<TItem>() where TItem : class, TBase => SafeGetInternal<TItem>(ID<TItem>.Order);
        /// <summary>
        /// Retrieves an item of a given <paramref name="itemType"/> from this <see cref="CRTPList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <param name="itemType">Item type to look for.</param>
        /// <exception cref="KeyNotFoundException">Item of a given <paramref name="itemType"/> is not defined in this list.</exception>
        public TBase? SafeGet(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return SafeGetInternal<TBase>(order);
            }

            return default;
        }

        /// <inheritdoc cref="SafeGet(Type)"/>
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal TItem? SafeGetInternal<TItem>(int order) where TItem : class, TBase
        {
            int flagsIndex;
            uint flags;
            switch (mode)
            {
                case PackingMode.Size0: return default;
                case PackingMode.SizeByte:
                    flagsIndex = order >> 2;
                    if (flagsIndex >= this.flags.Length)
                    {
                        return default; // Handles out-of-bound.
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & (order & 0b11) switch
                    {
                        0 => PackingByte.ItemFlag1,
                        1 => PackingByte.ItemFlag2,
                        2 => PackingByte.ItemFlag3,
                        3 => PackingByte.ItemFlag4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }) == 0)
                    {
                        return default; // Item not found.
                    }

                    return items[(order & 0b11) switch
                    {
                        0 => (flags & PackingByte.ItemValue1) >> PackingByte.ItemShift1,
                        1 => (flags & PackingByte.ItemValue2) >> PackingByte.ItemShift2,
                        2 => (flags & PackingByte.ItemValue3) >> PackingByte.ItemShift3,
                        3 => (flags & PackingByte.ItemValue4) >> PackingByte.ItemShift4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }] as TItem;

                case PackingMode.SizeUShort:
                    flagsIndex = order >> 1;
                    if (flagsIndex >= this.flags.Length)
                    {
                        return default; // Handles out-of-bound.
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & (order & 0b1) switch
                    {
                        0 => PackingUShort.ItemFlag1,
                        1 => PackingUShort.ItemFlag2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    }) == 0)
                    {
                        return default; // Item not found.
                    }

                    return items[(order & 0b1) switch
                    {
                        0 => (flags & PackingUShort.ItemValue1) >> PackingUShort.ItemShift1,
                        1 => (flags & PackingUShort.ItemValue2) >> PackingUShort.ItemShift2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    }] as TItem;

                case PackingMode.SizeUInt:
                    flagsIndex = order;
                    if (flagsIndex >= this.flags.Length)
                    {
                        return default; // Handles out-of-bound.
                    }

                    flags = this.flags[flagsIndex];
                    if ((flags & PackingUInt.ItemFlag) == 0)
                    {
                        return default; // Item not found.
                    }

                    return items[(flags & PackingUInt.ItemValue) >> PackingUInt.ItemShift] as TItem;

                default: throw new SwitchExpressionException(mode);
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Modification
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Attempts to add given <paramref name="item"/> to the list.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to register.</typeparam>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <returns>
        /// <c>true</c> if added successfully.
        /// <c>false</c> if item already exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TItem"/> does not match a real type of the <paramref name="item"/>.</exception>
        public bool Add<TItem>(TItem item) where TItem : TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TItem) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

            return AddInternal(ID<TItem>.Order, item);
        }

        /// <summary>
        /// Attempts to add given <paramref name="item"/> to the list, without throwing exceptions.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to register.</typeparam>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <returns>
        /// <c>true</c> if added successfully.
        /// <c>false</c> if item already exist.
        /// </returns>
        public bool SafeAdd<TItem>(TItem? item) where TItem : TBase
        {
            if (item is null || typeof(TItem) != item.GetType())
                return false;

            return AddInternal(ID<TItem>.Order, item);
        }

        /// <inheritdoc cref="Add{TItem}(TItem)"/>.
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal bool AddInternal(int order, TBase item)
        {
            int flagsIndex;
            uint index;
            switch (mode)
            {
                case PackingMode.Size0:
                    EnsureFlagsCapacity(order);
                    EnsureItemsCapacity(1);
                    switch (mode)
                    {
                        case PackingMode.Size0: throw new SwitchExpressionException(mode);
                        case PackingMode.SizeByte:
                            flags[order >> 2] = 
                            break;
                        case PackingMode.SizeUShort:
                            break;
                        case PackingMode.SizeUInt:
                            break;
                        default: throw new SwitchExpressionException(mode);
                    }
                    ref uint flags1 = ref flags[flagsIndex];
                    flags[mode switch
                    {
                        PackingMode.Size0 => throw new SwitchExpressionException(mode),
                        PackingMode.SizeByte => order >> 2,
                        PackingMode.SizeUShort => order >> 1,
                        PackingMode.SizeUInt => order
                        _ => throw new NotImplementedException(),
                    }]
                    items[0] = item;
                    stored = 1;
                    ItemChanged?.Invoke(item, order, added: true);
                    return true;

                case PackingMode.SizeByte:
                    flagsIndex = order >> 2;
                    if (flagsIndex >= flags.Length)
                    {
                        // Too small - resize & remap (if needed):
                        throw new NotImplementedException();
                    }

                    ref uint flags2 = ref flags[flagsIndex];
                    if ((flags2 & (order & 0b11) switch
                    {
                        0 => PackingByte.ItemFlag1,
                        1 => PackingByte.ItemFlag2,
                        2 => PackingByte.ItemFlag3,
                        3 => PackingByte.ItemFlag4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }) != 0)
                    {
                        return false; // Item already exist.
                    }

                    index = (uint)stored++; // TODO: Resize array to account for "stored + 1"
                    items[index] = item;
                    flags2 |= (order & 0b11) switch
                    {
                        0 => PackingByte.ItemFlag1 | (index << PackingByte.ItemShift1),
                        1 => PackingByte.ItemFlag2 | (index << PackingByte.ItemShift2),
                        2 => PackingByte.ItemFlag3 | (index << PackingByte.ItemShift3),
                        3 => PackingByte.ItemFlag4 | (index << PackingByte.ItemShift4),
                        _ => throw new SwitchExpressionException(order & 0b11),
                    };
                    ItemChanged?.Invoke(item, order, added: true);
                    return true;

                case PackingMode.SizeUShort:
                    flagsIndex = order >> 1;
                    if (flagsIndex >= flags.Length)
                    {
                        // Too small - resize & remap (if needed):
                        throw new NotImplementedException();
                    }

                    ref uint flags3 = ref flags[flagsIndex];
                    if ((flags3 & (order & 0b1) switch
                    {
                        0 => PackingUShort.ItemFlag1,
                        1 => PackingUShort.ItemFlag2,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }) != 0)
                    {
                        return false; // Item already exist.
                    }

                    index = (uint)stored++; // TODO: Resize array to account for "stored + 1"
                    items[index] = item;
                    flags3 |= (order & 0b1) switch
                    {
                        0 => PackingUShort.ItemFlag1 | (index << PackingUShort.ItemShift1),
                        1 => PackingUShort.ItemFlag2 | (index << PackingUShort.ItemShift2),
                        _ => throw new SwitchExpressionException(order & 0b11),
                    };
                    ItemChanged?.Invoke(item, order, added: true);
                    return true;

                case PackingMode.SizeUInt:
                    flagsIndex = order;
                    if (flagsIndex >= flags.Length)
                    {
                        // Too small - resize & remap (if needed):
                        throw new NotImplementedException();
                    }

                    ref uint flags4 = ref flags[flagsIndex];
                    if ((flags4 & PackingUInt.ItemFlag) != 0)
                    {
                        return false; // Item already exist.
                    }

                    index = (uint)stored++; // TODO: Resize array to account for "stored + 1"
                    items[index] = item;
                    flags4 |= PackingUInt.ItemFlag | (index << PackingUInt.ItemShift);
                    ItemChanged?.Invoke(item, order, added: true);
                    return true;

                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Attempts to insert given <paramref name="item"/> into the list at given <paramref name="index"/>.
        /// </summary>
        /// <remarks>
        /// <para>Throws if item of a type <typeparamref name="TItem"/> is already defined in the list.</para>
        /// <para>Throws if <paramref name="index"/> is too far from current set of items (i.e. larger than <see cref="Count"/>).</para>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// <para>O(n) branchless - if <paramref name="index"/> == 0.</para>
        /// <para>O(1) - if <paramref name="index"/> == <see cref="Count"/>.</para>
        /// </remarks>
        /// <typeparam name="TItem">Item type to register.</typeparam>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <param name="index">Index at which to insert given <paramref name="item"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is larger than <see cref="Count"/>.</exception>
        /// <exception cref="ItemAlreadyExistException">An item of a type <typeparamref name="TItem"/> is already listed.</exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TItem"/> does not match a real type of the <paramref name="item"/>.</exception>
        public void Insert<TItem>(TItem item, int index) where TItem : TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (index < 0 || index > stored)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (typeof(TItem) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

            if (Has<TItem>())
                throw new ItemAlreadyExistException(typeof(TItem));

            InsertInternalUnchecked(ID<TItem>.Order, item, index);
        }

        /// <summary>
        /// Attempts to insert given <paramref name="item"/> into the list at given <paramref name="index"/>, without throwing exceptions.
        /// </summary>
        /// <remarks>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// <para>O(n) branchless - if <paramref name="index"/> == 0.</para>
        /// <para>O(1) - if <paramref name="index"/> == <see cref="Count"/>.</para>
        /// </remarks>
        /// <typeparam name="TItem">Item type to register.</typeparam>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <param name="index">Index at which to insert given <paramref name="item"/>.</param>
        /// <returns>
        /// <see langword="true"/> if item was successfully inserted.
        /// <see langword="false"/> if item under the same type is already on the list.
        /// </returns>
        public bool SafeInsert<TItem>(TItem? item, int index) where TItem : TBase
        {
            if (item is null || index < 0 || index > stored || typeof(TItem) != item.GetType() || Has<TItem>())
                return false;

            InsertInternalUnchecked(ID<TItem>.Order, item, index);
            return true;
        }

        /// <remarks>
        /// Doesn't do bounds checks (Specifically "index > stored" - "index == stored" and "index less than stored" is fine)
        /// and doesn't check if item already exist or not - you need to handle it externally.
        /// </remarks>
        /// <inheritdoc cref="Insert{TItem}(TItem, int)"/>.
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal void InsertInternalUnchecked(int order, TBase item, int index)
        {
            if (index == stored)
            {
                AddInternal(order, item);
                return;
            }

            Span<uint> span = flags.AsSpan();
            switch (mode)
            {
                case PackingMode.Size0: AddInternal(order, item); return;
                case PackingMode.SizeByte:
                    if ((order >> 2) >= flags.Length || index >= items.Length || ((index + 3) >> 2) >= flags.Length)
                    {
                        // TODO: Finish and rework conditions.
                        // Note: If possible - increment item indexes as well, immediately.
                        // Too small - resize & remap (if needed):
                        throw new NotImplementedException();
                    }

                    Array.Copy(items, index, items, index + 1, items.Length - index - 1);
                    Span<byte> bytes = MemoryMarshal.AsBytes(span);
                    if (index == 0)
                    {
                        foreach (ref uint flags in span)
                        {
                            // Adds 1 only in entries that have an flag defined (a.k.a. entries which encode an index).
                            flags += (flags & 0x80808080) >> 15;
                        }
                    }
                    else
                    {
                        foreach (ref byte flags in bytes)
                        {
                            uint position = (uint)(flags & PackingByte.ByteValue);
                            if (position >= index)
                            {
                                // +1 here will never overflow into a flag, as remapping (or too large exception) should happen sooner.
                                flags = (byte)(PackingByte.ByteFlag | (position + 1));
                            }
                        }
                    }

                    ItemChanged?.Invoke(item, order, added: true);
                    bytes[order] = (byte)(PackingByte.ByteFlag | index);
                    stored++;
                    return;

                case PackingMode.SizeUShort:
                    if ((order >> 1) >= flags.Length || index >= items.Length || ((index + 1) >> 1) >= flags.Length)
                    {
                        // TODO: Finish and rework conditions.
                        // Note: If possible - increment item indexes as well, immediately.
                        // Too small - resize & remap (if needed):
                        throw new NotImplementedException();
                    }

                    Array.Copy(items, index, items, index + 1, items.Length - index - 1);
                    Span<ushort> ushorts = MemoryMarshal.Cast<uint, ushort>(span);
                    if (index == 0)
                    {
                        foreach (ref uint flags in span)
                        {
                            // Adds 1 only in entries that have an flag defined (a.k.a. entries which encode an index).
                            flags += (flags & 0x80008000) >> 31;
                        }
                    }
                    else
                    {
                        foreach (ref ushort flags in ushorts)
                        {
                            uint position = (uint)(flags & PackingUShort.UShortValue);
                            if (position >= index)
                            {
                                // +1 here will never overflow into a flag, as remapping (or too large exception) should happen sooner.
                                flags = (ushort)(PackingUShort.UShortFlag | (position + 1));
                            }
                        }
                    }

                    ItemChanged?.Invoke(item, order, added: true);
                    ushorts[order] = (ushort)(PackingUShort.UShortFlag | index);
                    stored++;
                    return;

                case PackingMode.SizeUInt:
                    if (order >= flags.Length || index >= items.Length || index >= flags.Length)
                    {
                        // TODO: Finish and rework conditions.
                        // Note: If possible - increment item indexes as well, immediately.
                        // Too small - resize & remap (if needed):
                        throw new NotImplementedException();
                    }

                    Array.Copy(items, index, items, index + 1, items.Length - index - 1);
                    if (index == 0)
                    {
                        foreach (ref uint flags in span)
                        {
                            // Adds 1 only in entries that have an flag defined (a.k.a. entries which encode an index).
                            flags += (flags & 0x80000000) >> 7;
                        }
                    }
                    else
                    {
                        foreach (ref uint flags in span)
                        {
                            uint position = flags & PackingUInt.ItemValue;
                            if (position >= index)
                            {
                                // +1 here will never overflow into a flag, as remapping (or too large exception) should happen sooner.
                                flags = PackingUInt.ItemFlag | (position + 1);
                            }
                        }
                    }

                    ItemChanged?.Invoke(item, order, added: true);
                    flags[order] = (uint)(PackingUInt.ItemFlag | index);
                    stored++;
                    return;

                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Removes item under a type <typeparamref name="TItem"/> from the list.
        /// </summary>
        /// <typeparam name="TItem">Item type to remove from the list.</typeparam>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public bool Remove<TItem>() where TItem : TBase => RemoveInternal<TItem>(ID<TItem>.Order, default, out var _);
        /// <summary>
        /// Removes item under a type <typeparamref name="TItem"/> from the list, and returns it as <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="TItem">Item type to remove from the list.</typeparam>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public bool Remove<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TBase => RemoveInternal(ID<TItem>.Order, default, out item);
        /// <summary>
        /// Removes specified <paramref name="item"/> from the list.
        /// </summary>
        /// <typeparam name="TItem">Item type to remove from the list.</typeparam>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TItem"/> does not match a real type of the <paramref name="item"/>.</exception>
        public bool Remove<TItem>(TItem item) where TItem : TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TItem) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

            return RemoveInternal(ID<TItem>.Order, item, out var _);
        }

        /// <summary>
        /// Removes specified <paramref name="item"/> from the list, without throwing exceptions.
        /// </summary>
        /// <typeparam name="TItem">Item type to remove from the list.</typeparam>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public bool SafeRemove<TItem>(TItem? item) where TItem : TBase
        {
            if (item is null || typeof(TItem) != item.GetType())
                return false;

            return RemoveInternal(ID<TItem>.Order, item, out var _);
        }

        /// <inheritdoc cref="Remove{TItem}()"/>.
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal bool RemoveInternal<TItem>(int order, TItem? criteria, [NotNullWhen(true)] out TItem? item) where TItem : TBase
        {
            int index;
            uint flags;
            switch (mode)
            {
                case PackingMode.Size0: item = default; return false;
                case PackingMode.SizeByte:
                    if ((order >> 2) >= this.flags.Length)
                    {
                        item = default;
                        return false; // Handles out-of-bounds.
                    }

                    flags = this.flags[order >> 2];
                    if ((flags & (order & 0b11) switch
                    {
                        0 => PackingByte.ItemFlag1,
                        1 => PackingByte.ItemFlag2,
                        2 => PackingByte.ItemFlag3,
                        3 => PackingByte.ItemFlag4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    }) == 0)
                    {
                        item = default;
                        return false; // Item is not defined.
                    }

                    index = (int)((order & 0b11) switch
                    {
                        0 => (flags & PackingByte.ItemValue1) >> PackingByte.ItemShift1,
                        1 => (flags & PackingByte.ItemValue2) >> PackingByte.ItemShift2,
                        2 => (flags & PackingByte.ItemValue3) >> PackingByte.ItemShift3,
                        3 => (flags & PackingByte.ItemValue4) >> PackingByte.ItemShift4,
                        _ => throw new SwitchExpressionException(order & 0b11),
                    });

                    if (items[index] is TItem result1 && (criteria is null || EqualityComparer<TItem>.Default.Equals(result1, criteria)))
                    {
                        item = result1;
                        RemoveAtInternalUnchecked(index, out var _);
                        ItemChanged?.Invoke(item, order, added: false);
                        return true;
                    }

                    item = default;
                    return false;

                case PackingMode.SizeUShort:
                    if ((order >> 1) >= this.flags.Length)
                    {
                        item = default;
                        return false; // Handles out-of-bounds.
                    }

                    flags = this.flags[order >> 1];
                    if ((flags & (order & 0b1) switch
                    {
                        0 => PackingUShort.ItemFlag1,
                        1 => PackingUShort.ItemFlag2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    }) == 0)
                    {
                        item = default;
                        return false; // Item is not defined.
                    }

                    index = (int)((order & 0b1) switch
                    {
                        0 => (flags & PackingUShort.ItemValue1) >> PackingUShort.ItemShift1,
                        1 => (flags & PackingUShort.ItemValue2) >> PackingUShort.ItemShift2,
                        _ => throw new SwitchExpressionException(order & 0b1),
                    });

                    if (items[index] is TItem result2 && (criteria is null || EqualityComparer<TItem>.Default.Equals(result2, criteria)))
                    {
                        item = result2;
                        RemoveAtInternalUnchecked(index, out var _);
                        ItemChanged?.Invoke(item, order, added: false);
                        return true;
                    }

                    item = default;
                    return false;

                case PackingMode.SizeUInt:
                    if (order >= this.flags.Length)
                    {
                        item = default;
                        return false; // Handles out-of-bounds.
                    }

                    flags = this.flags[order];
                    if ((flags & PackingUInt.ItemFlag) == 0)
                    {
                        item = default;
                        return false; // Item is not defined.
                    }

                    index = (int)((flags & PackingUInt.ItemValue) >> PackingUInt.ItemShift);
                    if (items[index] is TItem result3 && (criteria is null || EqualityComparer<TItem>.Default.Equals(result3, criteria)))
                    {
                        item = result3;
                        RemoveAtInternalUnchecked(index, out var _);
                        ItemChanged?.Invoke(item, order, added: false);
                        return true;
                    }

                    item = default;
                    return false;

                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Attempts to remove an item at a given <paramref name="index"/>.
        /// </summary>
        /// <remarks>
        /// <para>Throws if <paramref name="index"/> is beyond a current set of items (i.e. larger or equal to <see cref="Count"/>).</para>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// </remarks>
        /// <param name="index">Index at which to remove an item.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is larger than <see cref="Count"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= stored)
                throw new ArgumentOutOfRangeException(nameof(index));

            RemoveAtInternalUnchecked(index, out var _);
        }

        /// <summary>
        /// Attempts to remove an item at a given <paramref name="index"/>, and returns it as <paramref name="item"/> variable.
        /// </summary>
        /// <remarks>
        /// <para>Throws if <paramref name="index"/> is beyond a current set of items (i.e. larger or equal to <see cref="Count"/>).</para>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// </remarks>
        /// <param name="index">Index at which to remove an item.</param>
        /// <param name="item">Item that was under a given <paramref name="index"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is larger than <see cref="Count"/>.</exception>
        public bool RemoveAt(int index, [NotNullWhen(true)] out TBase? item)
        {
            if (index < 0 || index >= stored)
                throw new ArgumentOutOfRangeException(nameof(index));

            return RemoveAtInternalUnchecked(index, out item);
        }

        /// <summary>
        /// Attempts to remove an item at a given <paramref name="index"/>.
        /// </summary>
        /// <remarks>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// </remarks>
        /// <param name="index">Index at which to remove an item.</param>
        /// <returns>
        /// <see langword="true"/> if item was successfully remove.
        /// <see langword="false"/> if <paramref name="index"/> is invalid.
        /// </returns>
        public bool SafeRemoveAt(int index)
        {
            if (index < 0 || index >= stored)
                return false;

            return RemoveAtInternalUnchecked(index, out var _);
        }

        /// <summary>
        /// Attempts to remove an item at a given <paramref name="index"/>, and returns it as <paramref name="item"/> variable.
        /// </summary>
        /// <remarks>
        /// <para>Throws if <paramref name="index"/> is beyond a current set of items (i.e. larger or equal to <see cref="Count"/>).</para>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// </remarks>
        /// <param name="index">Index at which to remove an item.</param>
        /// <param name="item">Item that was under a given <paramref name="index"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is larger than <see cref="Count"/>.</exception>
        public bool SafeRemoveAt(int index, [NotNullWhen(true)] out TBase? item)
        {
            if (index < 0 || index >= stored)
                throw new ArgumentOutOfRangeException(nameof(index));

            return RemoveAtInternalUnchecked(index, out item);
        }

        /// <remarks>
        /// Doesn't do *any* bounds checks (for both size and <see cref="flags"/> length)
        /// and doesn't check if item actually exist or not - you need to handle it externally.
        /// </remarks>
        /// <inheritdoc cref="Insert{TItem}(TItem, int)"/>.
        /// Note (for maintainers): exposed as <see langword="internal"/>
        /// for usage in <see cref="CRTPListExtensions"/> and <see cref="Lookup{TFilter}"/>.
        internal bool RemoveAtInternalUnchecked(int index, [NotNullWhen(true)] out TBase? item)
        {
            switch (mode)
            {
                case PackingMode.Size0: item = default; return false;
                case PackingMode.SizeByte:
                    item = items[index];
                    Array.Copy(items, index + 1, items, index, items.Length - index - 1);
                    items[stored - 1] = default!;
                    Span<byte> bytes = MemoryMarshal.AsBytes(flags.AsSpan());
                    foreach (ref byte flags in bytes)
                    {
                        uint position = (uint)(flags & PackingByte.ByteValue);
                        if (position == index)
                        {
                            flags = 0;
                        }
                        else if (position > index)
                        {
                            flags = (byte)(PackingByte.ByteFlag | (position - 1));
                        }
                    }
                    stored--;
                    return true;

                case PackingMode.SizeUShort:
                    item = items[index];
                    Array.Copy(items, index + 1, items, index, items.Length - index - 1);
                    items[stored - 1] = default!;
                    Span<ushort> ushorts = MemoryMarshal.Cast<uint, ushort>(flags.AsSpan());
                    foreach (ref ushort flags in ushorts)
                    {
                        uint position = (uint)(flags & PackingUShort.UShortValue);
                        if (position == index)
                        {
                            flags = 0;
                        }
                        else if (position > index)
                        {
                            flags = (ushort)(PackingUShort.UShortFlag | (position - 1));
                        }
                    }
                    stored--;
                    return true;

                case PackingMode.SizeUInt:
                    item = items[index];
                    Array.Copy(items, index + 1, items, index, items.Length - index - 1);
                    items[stored - 1] = default!;
                    foreach (ref uint flags in flags.AsSpan())
                    {
                        uint position = flags & PackingUInt.ItemValue;
                        if (position == index)
                        {
                            flags = 0;
                        }
                        else if (position > index)
                        {
                            flags = PackingUInt.ItemFlag | (position - 1);
                        }
                    }
                    stored--;
                    return true;

                default: throw new SwitchExpressionException(mode);
            }
        }





        /// <summary>
        /// (WIP - not implemented yet).
        /// Trims internal item array to the smallest possible size, while keeping all items intact.
        /// </summary>
        /// <remarks>
        /// If <paramref name="remap"/> if <see langword="true"/> - list is allowed to change a size of underlying flags map.
        /// This will drastically reduce memory usage, BUT you will need to allocate larger map if you ever fill-up the map back.
        /// Unless you know what you are doing, it is recommended to set this value to <see langword="false"/>
        /// </remarks>
        /// <param name="remap">Whether to resize a flags map as well.
        /// <para>If set to <see langword="true"/> - flags map will get trimmed and might get remapped, if size allows.</para>
        /// <para>If set to <see langword="false"/> - flags map will get trimmed only to the size, where remapping is not needed.</para>
        /// </param>
        /// TODO: Implement extension methods allowing you to specify "minimal resize level", e.g.:
        /// - Allow resizing to <see cref="PackingMode.SizeUShort"/>, but not to <see cref="PackingMode.SizeByte"/>, etc.
        [Obsolete("WIP - will be provided by NetCore v1, or removed if not needed.", error: true)]
        public void Trim(bool remap = false)
        {
            throw new NotImplementedException();
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private void EnsureFlagsCapacity(int entries)
        {
            int size = mode switch
            {
                PackingMode.Size0 => 0,
                PackingMode.SizeByte => flags.Length << 2,
                PackingMode.SizeUShort => flags.Length << 1,
                PackingMode.SizeUInt => flags.Length,
                _ => throw new SwitchExpressionException(mode),
            };

            if (entries > size)
            {
                switch (mode = GetPackingMode(entries))
                {
                    case PackingMode.Size0: break;
                    case PackingMode.SizeByte: entries = (entries + 3) >> 2; break;
                    case PackingMode.SizeUShort: entries = (entries + 1) >> 1; break;
                    case PackingMode.SizeUInt: break;

                    default: throw new SwitchExpressionException(mode);
                }

                Array.Resize(ref flags, entries);
            }
        }

        private void EnsureItemsCapacity(int amount)
        {
            if (amount > items.Length)
            {
                Array.Resize(ref items, amount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetModeCapacity(PackingMode mode) => mode switch
        {
            PackingMode.Size0 => 0,
            PackingMode.SizeByte => PackingByte.Capacity,
            PackingMode.SizeUShort => PackingUShort.Capacity,
            PackingMode.SizeUInt => PackingUInt.Capacity,
            _ => throw new SwitchExpressionException(mode),
        };

        static PackingMode GetPackingMode(int capacity) => capacity switch
        {
            0 => PackingMode.Size0,
            <= PackingByte.Capacity => PackingMode.SizeByte,
            <= PackingUShort.Capacity => PackingMode.SizeUShort,
            _ => PackingMode.SizeUInt,
        };





        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Creates a new lookup data structure, attached to this <see cref="CRTPList{TBase}"/> parent.
        /// </summary>
        /// <exception cref="InvalidLookupTypeException">
        /// <typeparamref name="TFilter"/> equals to <typeparamref name="TBase"/>.
        /// This is not allowed, because <see cref="Lookup{TFilter}"/> will describe all values of <see cref="CRTPList{TBase}"/>
        /// which doesn't make sense - it's better to expose the original <see cref="CRTPList{TBase}"/> instead.
        /// </exception>
        public Lookup<TFilter> GetLookup<TFilter>() where TFilter : TBase
        {
            if (typeof(TFilter) == typeof(TBase))
            {
                throw new InvalidLookupTypeException(typeof(TFilter));
            }

            return new Lookup<TFilter>(this);
        }
    }

    /// <summary>
    /// Useful extension methods when working with <see cref="CRTPList{T}"/>.
    /// </summary>
    public static class CRTPListExtensions
    {
        /// <summary>
        /// Attempts to add given <paramref name="item"/> to the list.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <param name="list"><see cref="CRTPList{TBase}"/> to work with.</param>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <returns>
        /// <c>true</c> if added successfully.
        /// <c>false</c> if item already exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TBase"/> does not match a real type of the <paramref name="item"/>.</exception>
        /// <seealso cref="CRTPList{TBase}.Add{TItem}(TItem)"/>
        /// Note (for maintainers): Implemented as extension, so it highlights with different color in IDEs.
        ///  This is needed because this method is slower than strongly-typed <see cref="CRTPList{TBase}.Insert{TItem}(TItem, int)"/>,
        ///  But this method is needed for convenience.
        public static bool Add<TBase>(this CRTPList<TBase> list, TBase item) where TBase : class
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TBase) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TBase), item.GetType());

            if (CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
            {
                return list.AddInternal(order, item);
            }

            return false;
        }

        /// <summary>
        /// Attempts to add given <paramref name="item"/> to the list, without throwing exceptions.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <param name="list"><see cref="CRTPList{TBase}"/> to work with.</param>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <returns>
        /// <c>true</c> if added successfully.
        /// <c>false</c> if item already exist.
        /// </returns>
        /// <seealso cref="CRTPList{TBase}.Add{TItem}(TItem)"/>
        /// Note (for maintainers): Implemented as extension, so it highlights with different color in IDEs.
        ///  This is needed because this method is slower than strongly-typed <see cref="CRTPList{TBase}.Insert{TItem}(TItem, int)"/>,
        ///  But this method is needed for convenience.
        public static bool SafeAdd<TBase>(this CRTPList<TBase> list, TBase item) where TBase : class
        {
            if (list is null || item is null || typeof(TBase) != item.GetType())
                return false;

            if (CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
            {
                return list.AddInternal(order, item);
            }

            return false;
        }

        /// <summary>
        /// Attempts to insert given <paramref name="item"/> into the list at given <paramref name="index"/>.
        /// </summary>
        /// <remarks>
        /// <para>Throws if item with the same type is already defined in the list.</para>
        /// <para>Throws if <paramref name="index"/> is beyond a current set of items (i.e. larger or equal to <see cref="CRTPList{TBase}.Count"/>).</para>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// <para>O(n) branchless - if <paramref name="index"/> == 0.</para>
        /// <para>O(1) - if <paramref name="index"/> == <see cref="CRTPList{TBase}.Count"/>.</para>
        /// </remarks>
        /// <param name="list"><see cref="CRTPList{TBase}"/> to work with.</param>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <param name="index">Index at which to insert given <paramref name="item"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is larger than <see cref="CRTPList{TBase}.Count"/>.</exception>
        /// <exception cref="InvalidIndexingTypeException"><paramref name="item"/> does not inherit <typeparamref name="TBase"/>.</exception>
        /// <exception cref="ItemAlreadyExistException">An item with the same type is already listed.</exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TBase"/> does not match a real type of the <paramref name="item"/>.</exception>
        /// <seealso cref="CRTPList{TBase}.Insert{TItem}(TItem, int)"/>
        /// Note (for maintainers): Implemented as extension, so it highlights with different color in IDEs.
        ///  This is needed because this method is slower than strongly-typed <see cref="CRTPList{TBase}.Add{TItem}(TItem)"/>,
        ///  But this method is needed for convenience.
        public static void Insert<TBase>(this CRTPList<TBase> list, TBase item, int index) where TBase : class
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (index > list.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (typeof(TBase) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TBase), item.GetType());

            if (!CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
                throw new InvalidIndexingTypeException(item.GetType(), typeof(TBase));

            if (list.HasInternal(order))
                throw new ItemAlreadyExistException(item.GetType());

            list.InsertInternalUnchecked(order, item, index);
        }

        /// <summary>
        /// Attempts to insert given <paramref name="item"/> into the list at given <paramref name="index"/>, without throwing exceptions.
        /// </summary>
        /// <remarks>
        /// <para>Complexity: O(n) (In large lists can be very expensive)</para>
        /// <para>O(n) branchless - if <paramref name="index"/> == 0.</para>
        /// <para>O(1) - if <paramref name="index"/> == <see cref="CRTPList{TBase}.Count"/>.</para>
        /// </remarks>
        /// <param name="list"><see cref="CRTPList{TBase}"/> to work with.</param>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <param name="index">Index at which to insert given <paramref name="item"/>.</param>
        /// <returns>
        /// <see langword="true"/> if item was successfully inserted.
        /// <see langword="false"/> if item under the same type is already on the list.
        /// </returns>
        /// <seealso cref="CRTPList{TBase}.SafeInsert{TItem}(TItem, int)"/>
        public static bool SafeInsert<TBase>(this CRTPList<TBase> list, TBase item, int index) where TBase : class
        {
            if (list is null || item is null || index > list.Count || typeof(TBase) != item.GetType())
                return false;

            if (!CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
                return false;

            if (list.HasInternal(order))
                return false;

            list.InsertInternalUnchecked(order, item, index);
            return true;
        }

        /// <summary>
        /// Removes item under a type <typeparamref name="TBase"/> from the list, without throwing exceptions.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="InvalidIndexingTypeException"><paramref name="item"/> does not inherit <typeparamref name="TBase"/>.</exception>
        /// <exception cref="ItemAlreadyExistException">An item with the same type is already listed.</exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TBase"/> does not match a real type of the <paramref name="item"/>.</exception>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public static bool Remove<TBase>(this CRTPList<TBase> list, TBase item) where TBase : class
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TBase) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TBase), item.GetType());

            if (!CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
                throw new InvalidIndexingTypeException(item.GetType(), typeof(TBase));

            return list.RemoveInternal(order, item, out var _);
        }

        /// <summary>
        /// Removes item under a type <typeparamref name="TBase"/> from the list.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public static bool SafeRemove<TBase>(this CRTPList<TBase> list, TBase item) where TBase : class
        {
            if (list is null || item is null || typeof(TBase) != item.GetType())
                return false;

            if (!CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
                return false;

            return list.RemoveInternal(order, item, out var _);
        }
    }
}
