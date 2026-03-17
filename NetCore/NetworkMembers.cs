using System.Threading;

namespace NetCore
{
    /// <summary>
    /// Stores some trivia about <see cref="NetworkMember"/> instances.
    /// </summary>
    public static class NetworkMembers
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Static Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public static bool IsAnyActive => Interlocked.Read(ref m_ActiveNetworkMembers) > 0;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static long m_ActiveNetworkMembers = 0;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Increments the amount of currently active <see cref="NetworkMember"/>s.
        /// </summary>
        public static void IncrementActiveMembers() => Interlocked.Increment(ref m_ActiveNetworkMembers);

        /// <summary>
        /// Decrements the amount of currently active <see cref="NetworkMember"/>s.
        /// </summary>
        public static void DecrementActiveMembers() => Interlocked.Decrement(ref m_ActiveNetworkMembers);
    }
}
