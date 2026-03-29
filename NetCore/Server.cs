using Cysharp.Threading.Tasks;
using NetCore.Transports;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    /// <inheritdoc cref="NetworkMember"/>
    /// TODO: Consider adding a check for 0 transports being present.
    public class Server(int transports) : NetworkMember<Server>(transports)
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default parameter-less .ctor.
        /// Pre-allocates some space for transports with <see cref="NetworkMember.DefaultInitialTransportCapacity"/>.
        /// </summary>
        public Server() : this(DefaultInitialTransportCapacity) { }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Starts a server and binds all registered transports to a provided <see cref="StartupArgs.LocalIPEndPoint"/>.
        /// </summary>
        /// <inheritdoc/>
        protected override async UniTask<OperationResult> StartOperation(StartupArgs args, CancellationToken token)
        {
            OperationResult result = await base.StartOperation(args, token);
            if (result == OperationResult.Success)
                Servers.Add(this);

            return result;
        }

        /// <summary>
        /// Disconnects all the players, stops the server, and unbinds all transports.
        /// </summary>
        /// <inheritdoc/>
        protected override UniTask<OperationResult> StopOperation()
        {
            Servers.Remove(this);
            return base.StopOperation();
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Send Methods
        /// .                                A.K.A.: Good luck translating documentation.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        
    }
}
