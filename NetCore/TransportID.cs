using System;
using System.Collections.Generic;

namespace NetCore
{
    /// <summary>
    /// Stores special ID for all transports.
    /// </summary>
    /// <remarks>
    /// ID is never used in networking (there is no need).
    /// It is only used to optimize dictionary by ~3.5 times.
    /// </remarks>
    public static class TransportID<T> where T : ITransport
    {
        public static readonly ulong ID = ;



        static ulong GetID();
    }

    public static class TransportID
    {
        const int TransportLimit = 14; // Described by the fundamental design of the library.
        static readonly Dictionary<Type, ulong> m_IDs = new(TransportLimit); // Supports up to 13 IDs.
        static int m_TransportsRegistered = 0;

        public static ulong GetTransportID()
        {
            if (m_TransportsRegistered >= TransportLimit)
            {
                throw new Exception("Exhausted transport count.");
            }

            switch (m_TransportsRegistered++)
            {
                case 0: return 
            }
        }
    }
}
