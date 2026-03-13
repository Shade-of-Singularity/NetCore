using NetCore.Common;
using System;

namespace NetCore
{
    /// <summary>
    /// Controls where <see cref="CustomHeader{T}"/> data is located.
    /// </summary>
    public static class CustomHeaders
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Static Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Amount of currently registered custom headers.
        /// </summary>
        public static int Amount
        {
            get
            {
                lock (_lock) return m_SizeMap.Count;
            }
        }

        /// <summary>
        /// Map, enlisting <see cref="CustomHeader{T}.SizeInBits"/> of all currently registered <see cref="CustomHeader{T}"/> types.
        /// </summary>
        public static int[] SizeMap
        {
            get
            {
                lock (_lock) return [.. m_SizeMap];
            }
        }

        /// <summary>
        /// Map, enlisting how much bits <see cref="CustomHeader{T}.SizeInBits"/> actually take.
        /// </summary>
        public static int[] BitMap
        { 
            get
            {
                lock (_lock) return [.. m_SizeMap];
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static readonly LazyArray<int> m_SizeMap = [];
        private static readonly LazyArray<int> m_BitMap = [];
        private static readonly object _lock = new();
        private static Action? m_OnReset;




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
        /// Removes all custom header allocations, without touching any built-in headers.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                try
                {
                    m_OnReset?.Invoke();
                }
                catch (Exception ex)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    Console.ForegroundColor = color;
                }

                m_SizeMap.Clear();
                m_BitMap.Clear();

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
                // Note: You can add here built-in headers.
            }
        }


        /// <remarks>
        /// If you call this method multiple times - it will create multiple allocations.
        /// Make sure to call it only once after app startup or after <see cref="Reset"/>!
        /// </remarks>
        internal static void Register<T>(Action onReset, out int index) where T : CustomHeader<T>, new()
        {
            lock (_lock)
            {
                index = m_SizeMap.Count;
                m_SizeMap.Add(CustomHeader<T>.SizeInBits);
                int lastBit = 1 << BitScanner.BitScanForward((ulong)CustomHeader<T>.SizeInBits);
                m_BitMap.Add(lastBit | (lastBit - 1));
                m_OnReset += onReset;
            }
        }
    }
}
