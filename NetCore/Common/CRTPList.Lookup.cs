using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCore.Common
{

    public sealed partial class CRTPList<TBase>
    {
        /// <summary>
        /// (Not thread-safe!)
        /// Lookup struct for managing items of a specific type in a base <see cref="CRTPList{TBase}"/>.
        /// </summary>
        /// <typeparam name="TFilter">Type for which to filter.</typeparam>
        /// <param name="list"></param>
        public struct Lookup<TFilter>(CRTPList<TBase> list) where TFilter : TBase
        {
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                              Public Properties
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            public readonly int Count => stored;




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Private Fields
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            uint[] flags = [];
            int stored;




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Public Methods
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            /// <inheritdoc cref="CRTPList{TBase}.Has{TItem}()"/>
            public readonly bool Has<TItem>() where TItem : TFilter => list.Has<TItem>();

            /// <inheritdoc cref="CRTPList{TBase}.TryGet{TItem}(out TItem)"/>
            public readonly bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TFilter => list.TryGet(out item);

            /// <inheritdoc cref="CRTPList{TBase}.Get{TItem}()"/>
            public readonly TItem Get<TItem>() where TItem : TFilter => list.Get<TItem>();

            /// <inheritdoc cref="CRTPList{TBase}.GetSafe{TItem}()"/>
            public readonly TItem? GetSafe<TItem>() where TItem : class, TFilter => list.GetSafe<TItem>();

            /// <inheritdoc cref="CRTPList{TBase}.Add{TItem}(TItem)"/>
            public readonly bool Add<TItem>(TItem item) where TItem : TFilter => list.Add(item);

            /// <inheritdoc cref="CRTPList{TBase}.Insert{TItem}(TItem, int)"/>
            public readonly void Insert<TItem>(TItem item, int index) where TItem : TFilter => list.Insert(item, index);

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}()"/>
            public readonly bool Remove<TItem>() where TItem : TFilter
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}(out TItem)"/>
            public readonly bool Remove<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TFilter
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}(TItem)"/>
            public readonly bool Remove<TItem>(TItem item) where TItem : TFilter
            {
                throw new NotImplementedException();
            }

            // TODO: Add the rest of the methods, including an enumerator.
            /// <summary>
            /// Retrieves special struct-based enumerator for iterating over items, filtered with <typeparamref name="TFilter"/> type.
            /// </summary>
            /// <returns>Special struct-based enumerator for very fast type-safe iterations.</returns>
            public readonly Enumerator GetEnumerator()
            {
                PackingMode mode = list.mode;
                return new(list.items, mode,
                    bytes: mode == PackingMode.SizeByte ? MemoryMarshal.AsBytes(list.flags.AsSpan()) : default,
                    ushorts: mode == PackingMode.SizeUShort ? MemoryMarshal.Cast<uint, ushort>(list.flags.AsSpan()) : default,
                    uints: mode == PackingMode.SizeUInt ? list.flags.AsSpan() : default,
                    total: list.stored, flags, stored);
            }

            /// <summary>
            /// Struct-based enumerator for fast iterations over filtered items.
            /// </summary>
            public ref struct Enumerator
            {
                // CRTP List data:
                readonly TBase[] items;
                readonly PackingMode mode; // TODO: Replace with 4 enumerators instead, if possible.
                readonly Span<byte> bytes;
                readonly Span<ushort> ushorts;
                readonly Span<uint> uints;
                readonly int total;
                // Lookup data:
                readonly uint[] flags;
                readonly int stored;
                int iterator = -1;
                int index = -1;
                /// <param name="items">Reference to the internal array of items from <see cref="CRTPList{TBase}"/>.</param>
                /// <param name="mode">Current packing mode from a parent <see cref="CRTPList{TBase}"/>.</param>
                /// <param name="bytes">Bytes, used for encoding position data in a parent list or <see langword="default"/> if cannot be provided.</param>
                /// <param name="ushorts">UShorts, used for encoding position data in a parent list or <see langword="default"/> if cannot be provided.</param>
                /// <param name="uints">UInts, used for encoding position data in a parent list or <see langword="default"/> if cannot be provided.</param>
                /// <param name="total">Total amount of items, stored in the <paramref name="items"/> array.</param>
                /// <param name="flags">Flags, describing which <typeparamref name="TFilter"/> items are stored in <paramref name="items"/> array.</param>
                /// <param name="stored">Amount of filtered items. When all will be found - iteration will stop earier.</param>
                internal Enumerator(
                    TBase[] items, PackingMode mode, Span<byte> bytes, Span<ushort> ushorts, Span<uint> uints, int total, // CRTP List data.
                    uint[] flags, int stored) // Lookup data.
                {
                    this.items = items;
                    this.mode = mode;
                    this.bytes = bytes;
                    this.ushorts = ushorts;
                    this.uints = uints;
                    this.total = total;
                    this.flags = flags;
                    this.stored = stored;
                }
                /// <summary>
                /// Retrieves item under current iterator state.
                /// </summary>
                /// <exception cref="ArgumentOutOfRangeException">
                /// Enumerator is either not started or accessed when all possible items were already iterated over.
                /// </exception>
                public readonly TFilter Current => (TFilter)items[index];
                /// <summary>
                /// Moves enumerator forward.
                /// </summary>
                /// <returns>
                /// <see langword="true"/> if there was more items to iterate over, and <see cref="Current"/> was assigned.
                /// <see langword="false"/> if there was no more items, and <see cref="Current"/> was set to <see langword="default"/> value.
                /// </returns>
                public bool MoveNext()
                {
                    // TODO: Optimize lookups by processing own flags in bulk - in a set of 4 bits,
                    //  and mapping them to valid indexes using switch jump-table.
                    //  Or come up with even better approach using only bit masks.
                    return mode switch
                    {
                        PackingMode.Size0 => false,
                        PackingMode.SizeByte => false,// TODO: Finish.
                        PackingMode.SizeUShort => false,// TODO: Finish.
                        PackingMode.SizeUInt => false,// TODO: Finish.
                        _ => throw new SwitchExpressionException(mode),
                    };
                }
            }
        }
    }
}
