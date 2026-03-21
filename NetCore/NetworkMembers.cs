using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public static bool IsAnyActive => m_ActiveMembers.Count > 0;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static readonly Dictionary<NetworkMember, bool> m_ActiveMembers = [];




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Lists given <see cref="NetworkMember"/> in the active members hash set.
        /// </summary>
        public static void ListActiveMember(NetworkMember member) => m_ActiveMembers.TryAdd(member, default);
        /// <summary>
        /// Delists given <see cref="NetworkMember"/> from the active members hash set.
        /// </summary>
        /// <param name="member"></param>
        public static void DelistActiveMember(NetworkMember member) => m_ActiveMembers.Remove(member);
    }
}
