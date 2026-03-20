using Cysharp.Threading.Tasks;
using NetCore.Common;
using NetCore.Transports;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace NetCore
{
    public abstract partial class NetworkMember
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private uint m_ConnectedTransportsCount = 0;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        void INetworkMemberStatistics.IncrementConnectedTransports() => IncrementConnectedTransports();

        /// <summary>
        /// <inheritdoc cref="INetworkMemberStatistics.IncrementConnectedTransports"/>
        /// </summary>
        protected virtual void IncrementConnectedTransports()
        {
            checked { m_ConnectedTransportsCount++; }
        }

        void INetworkMemberStatistics.DecrementConnectedTransports() => DecrementConnectedTransports();

        /// <summary>
        /// <inheritdoc cref="INetworkMemberStatistics.DecrementConnectedTransports"/>
        /// </summary>
        protected virtual void DecrementConnectedTransports()
        {
            checked { m_ConnectedTransportsCount--; }
        }
    }
}
