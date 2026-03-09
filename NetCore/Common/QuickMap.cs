using System;
using System.Collections;
using System.Collections.Generic;

namespace NetCore.Common
{
    /// <summary>
    /// Dictionary, optimized to quickly lookup specific items using CRTP internally.
    /// </summary>
    /// <remarks>
    /// Has a technical limit of items. See also: <see cref="QuickIndex.Limit"/>
    /// </remarks>
    /// <typeparam name="T">Item type to store.</typeparam>
    public struct QuickMap<T> : where T : class
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public readonly int Length => values.Length;

        /// <inheritdoc/>
        public T this[int index]
        {
            readonly get => values[index];
            set => throw new NotSupportedException($"Setter is not supported in {nameof(QuickMap<T>)}. Use {nameof(Add)} and {nameof(Remove)} instead.");
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private QuickIndexMask lookup; // We use enum instead of raw ulong to avoid a lot of casting in raw code.
        private T[] values;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>Default constructor. Initialized <see cref="QuickMap{T}"/> with an empty array.</summary>
        public QuickMap() => values = [];
        public QuickMap(int capacity)
        {
            if (capacity > QuickIndex.Limit)
            {
                throw new ArgumentOutOfRangeException($"Cannot create QuickMap with a {capacity}, larger than {QuickIndex.Limit} (technical limit)(seealso: {nameof(QuickIndex)}).{nameof(QuickIndex.Limit)}");
            }

            values = new T[capacity];
        }

        public void Add<TItem>(TItem item) where TItem : class, T
        {
            // Will throw if item cannot be stored. Some checks can be removed.
            QuickIndex index = QuickID<TItem, T>.Index;

        }

        public void Set<TItem>(TItem item) where TItem : class, T
        {
            // Will throw if item cannot be stored. Some checks can be removed.
            QuickIndex index = QuickID<TItem, T>.Index;

        }

        public bool Has<TItem>() where TItem : class, T
        {
            // Will throw if item cannot be stored. Some checks can be removed.
            QuickIndex index = QuickID<TItem, T>.Index;

        }

        public bool Remove<TItem>(TItem item) where TItem : class, T
        {
            // Will throw if item cannot be stored. Some checks can be removed.
            QuickIndex index = QuickID<TItem, T>.Index;
            Span<char>
        }

        /// <summary>
        /// Retrieves struct-based enumerator for enumerating over all value of this <see cref="QuickMap{T}"/>.
        /// </summary>
        /// <returns>Struct-based enumerator.</returns>
        public readonly QuickMapEnumerator GetEnumerator() => new(values);

        /// <summary>
        /// Struct-based enumerator used for zero allocation enumeration over the internal array of a <see cref="QuickMap{T}"/>.
        /// </summary>
        /// <param name="values"></param>
        public ref struct QuickMapEnumerator(T[] values)
        {
            private readonly T[] values = values;
            private int index = -1;

            /// <inheritdoc/>
            public readonly T Current => index < values.Length ? values[index] : default!;

            /// <summary>
            /// Moves pointer to the next index forward.
            /// </summary>
            /// <returns>Whether there are more items in a sequence.</returns>
            public bool MoveNext()
            {
                if (index < values.Length)
                {
                    index++;
                    return true;
                }

                return false;
            }
        }
    }
}
