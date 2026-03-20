using NetCore.Common;
using NetCore.Identity;
using System;

namespace NetCore
{
    /// <summary>
    /// Controls where <see cref="CustomHeader{T}"/> data is located.
    /// </summary>
    /// TODO: Provide custom pooling solution for byte arrays, for better GC compatibility.
    public static class CustomHeaders
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Static Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Max amount of headers system should expect.
        /// </summary>
        //public static int MaxHeaderAmount
        //{
        //    get => m_MaxHeaderAmount;
        //    set => m_MaxHeaderAmount = Math.Max(1024, value);
        //}

        /// <summary>
        /// Amount of currently registered custom headers.
        /// </summary>
        public static int Amount
        {
            get
            {
                lock (_lock) return m_SizeBytes.Count;
            }
        }

        /// <summary>
        /// Max size (in bytes) seen amongst all currently registered headers. Can be 0.
        /// </summary>
        public static int MaxHeaderSizeInBytes
        {
            get
            {
                lock (_lock) return m_MaxHeaderSizeInBytes;
            }
        }

        /// <summary>
        /// Max size (in bits) seen amongst all currently registered headers. Can be 0.
        /// </summary>
        public static int MaxHeaderSizeInBits
        {
            get
            {
                lock (_lock) return m_MaxHeaderSizeInBits;
            }
        }

        /// <summary>
        /// Max total size of unpacked headers, if all registered headers are defined.
        /// </summary>
        public static int MaxContentSizeInBytes
        {
            get
            {
                lock (_lock) return m_MaxContentSizeInBytes;
            }
        }

        /// <summary>
        /// Max total size of packed headers, if all registered headers are defined.
        /// </summary>
        public static int MaxContentSizeInBits
        {
            get
            {
                lock (_lock) return m_MaxContentSizeInBits;
            }
        }

        /// <summary>
        /// Map, enlisting <see cref="CustomHeader{T}.SizeInBits"/> of all currently registered <see cref="CustomHeader{T}"/> types.
        /// </summary>
        public static int[] SizeMap
        {
            get
            {
                lock (_lock) return [.. m_SizeBytes];
            }
        }

        /// <summary>
        /// Map, enlisting how much bits <see cref="CustomHeader{T}.SizeInBits"/> actually take.
        /// </summary>
        public static int[] BitMap
        {
            get
            {
                lock (_lock) return [.. m_SizeBits];
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static readonly LazyArray<CustomHeaderRegionSupplier> m_SensitiveRegions = [];
        private static readonly LazyArray<int> m_SizeBytes = [];
        private static readonly LazyArray<int> m_SizeBits = [];
        private static readonly object _lock = new();
        private static Action? m_OnReset;

        //private static int m_MaxHeaderAmount = 2048;
        private static int m_MaxContentSizeInBytes;
        private static int m_MaxContentSizeInBits;
        private static int m_MaxHeaderSizeInBytes;
        private static int m_MaxHeaderSizeInBits;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        static CustomHeaders() => RegisterBuiltInHeaders();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Removes all custom header allocations, and re-registers built-in ones.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                m_OnReset?.Invoke();
                m_OnReset = null;

                m_SizeBytes.Clear();
                m_SizeBits.Clear();
                m_MaxContentSizeInBytes = 0;
                m_MaxContentSizeInBits = 0;
                m_MaxHeaderSizeInBytes = 0;
                m_MaxHeaderSizeInBits = 0;

                // Re-registers built-in ones.
                RegisterBuiltInHeaders();
            }
        }

        /// <summary>
        /// Initializes a custom header system by adding a built-in headers.
        /// </summary>
        internal static void RegisterBuiltInHeaders()
        {
            lock (_lock)
            {
                TemporaryIdentifierHeader.Register();
            }
        }

        /// <summary>
        /// Stores info about which item was registered.
        /// </summary>
        internal static class Registrar<T>
        {
            public static bool IsRegistered = false;
        }

        /// <remarks>
        /// If you call this method multiple times - it will create multiple allocations.
        /// Make sure to call it only once after app startup or after <see cref="Reset"/>!
        /// </remarks>
        internal static void Register<T>() where T : CustomHeader<T>, new()
        {
            if (NetworkMembers.IsAnyActive)
            {
                throw new Exception($"Cannot define new headers after any {nameof(NetworkMember)} were started! HeaderHelpers: {typeof(T).Name}");
            }

            lock (_lock)
            {
                if (Registrar<T>.IsRegistered)
                {
                    throw new Exception($"Cannot register a header twice! HeaderHelpers: {typeof(T).Name}");
                }

                // Fetches info about header position in flags and content.
                int order = m_SizeBytes.Count;
                int contentPosition = m_MaxContentSizeInBytes;
                m_OnReset += static () =>
                {
                    Registrar<T>.IsRegistered = false;
                    CustomHeader<T>.Descriptor.Provider(default, default);
                };

                // Registers information:
                m_SizeBits.Add(CustomHeader<T>.SizeInBits);
                m_MaxContentSizeInBits += CustomHeader<T>.SizeInBits;
                m_MaxHeaderSizeInBits = Math.Max(m_MaxHeaderSizeInBits, CustomHeader<T>.SizeInBits);
                m_SizeBytes.Add(CustomHeader<T>.SizeInBytes);
                m_MaxContentSizeInBytes += CustomHeader<T>.SizeInBytes;
                m_MaxHeaderSizeInBytes = Math.Max(m_MaxHeaderSizeInBytes, CustomHeader<T>.SizeInBytes);

                // Fires registration callbacks:
                Registrar<T>.IsRegistered = true;
                CustomHeader<T>.Descriptor.Provider(order, contentPosition);
            }
        }

        /// <summary>
        /// Gets enumerator for iterating over all sensitive regions.
        /// </summary>
        /// <remarks>
        /// Returns a *copy* of the <see cref="LazyArray{T}"/>.
        /// </remarks>
        internal static LazyArray<CustomHeaderRegionSupplier> GetSensitiveRegions() => m_SensitiveRegions;
    }
}
