using NetCore.Common;
using System;
using System.Collections.Generic;

namespace NetCore
{
    /// <summary>
    /// Controls where <see cref="CustomHeader{T}"/> data is located.
    /// </summary>
    public static class CustomHeaders
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Amount of bits used to encode all built-in <see cref="MessageType"/>s.
        /// You cannot set <see cref="TypeBits"/> to a value smaller than this.
        /// </summary>
        public const int BuiltInTypeBits = 2;
        /// <summary>
        /// Max amount of bits that can be used to encode <see cref="MessageType"/>.
        /// Limited to <see cref="byte"/> for easier reading on arrival.
        /// </summary>
        public const int MaxTypeBits = sizeof(byte) * 8;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Static Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Amount of *bits* used to encode <see cref="MessageType"/>.
        /// Since <see cref="MessageType"/> is a <see cref="byte"/> - value will be clamped to a range [2:8].
        /// </summary>
        public static int TypeBits
        {
            get => m_TypeBits;
            set => m_TypeBits = Math.Clamp(value, BuiltInTypeBits, MaxTypeBits);
        }

        /// <summary>
        /// Amount of currently registered custom headers.
        /// </summary>
        public static int Amount
        {
            get
            {
                lock (_lock) return m_Amount;
            }
            set
            {
                lock (_lock) m_Amount = value;
            }
        }

        /// <summary>
        /// Map with sizes of all 
        /// </summary>
        public static int SizeMap
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// Map
        /// </summary>
        public static int TotalHeadedSize
        {
            get => throw new NotImplementedException();
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static readonly Dictionary<MessageType, LazyArray<int>> m_SizeMaps = [];
        private static readonly object _lock = new();
        private static bool m_IsInitialized;
        private static int m_Amount;
        private static int m_TypeBits = BuiltInTypeBits;





        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Retrieves header size map for a specific <see cref="MessageType"/>,
        /// describing all headers this <see cref="MessageType"/> can contain.
        /// </summary>
        /// <remarks>
        /// Returned size map can be empty, but never null.
        /// </remarks>
        public static int[] GetSizeMap(MessageType type)
        {
            lock (m_SizeMaps)
            {
                if (!m_SizeMaps.TryGetValue(type, out var lazy))
                {
                    return [];
                }

                return [.. lazy];
            }
        }

        /// <summary>
        /// Removes all custom header allocations, without touching any built-in headers.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {

            }
        }

        /// <summary>
        /// Initializes a custom header system by adding a built-in headers.
        /// </summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (m_IsInitialized)
                    return;

                m_IsInitialized = true;
            }
        }

    }
}
