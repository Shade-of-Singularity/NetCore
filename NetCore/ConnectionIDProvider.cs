using System;
using System.Collections.Generic;

namespace NetCore
{
    /// <summary>
    /// Provides <see cref="ConnectionID"/>s for new connections.
    /// </summary>
    public sealed class ConnectionIDProvider
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private readonly HashSet<ulong> IDs = [];
        private ulong nextCID = 0;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Resets internal state of <see cref="ConnectionIDProvider"/>: clears hash maps, and moves CID head back to '0'
        /// </summary>
        public void Reset()
        {
            IDs.Clear();
            nextCID = 0;
        }

        /// <summary>
        /// Retrieves
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ConnectionID NextCID()
        {
            ulong i = 0;
            while (true)
            {
                if (IDs.Add(nextCID))
                {
                    return (ConnectionID)nextCID++;
                }

                nextCID++;
                if (i >= ulong.MaxValue)
                {
                    throw new Exception($"(Somehow) exhausted all possible CIDs! How many clients do you even have???");
                }
            }
        }

        /// <summary>
        /// Forgets a specific ID, making it reusable again.
        /// </summary>
        /// <param name="connection">Connection to forget.</param>
        public void Forget(ConnectionID connection) => IDs.Remove((ulong)connection);
    }
}
