using NetCore.Common;
using NetCore.Transports;
using System.Runtime.CompilerServices;

namespace NetCore
{
    public abstract partial class NetworkMember
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// CRTP-based list (similar to dictionary), which stores references to all transports this <see cref="NetworkMember"/> manages. 
        /// </summary>
        protected readonly CRTPList<ITransport> Transports;
        /// <summary>
        /// Lookup for all <see cref="IResilientTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        protected readonly CRTPList<ITransport>.Lookup<IResilientTransport> ResilientTransports;
        /// <summary>
        /// Lookup for all <see cref="IReliableTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        protected readonly CRTPList<ITransport>.Lookup<IReliableTransport> ReliableTransports;
        /// <summary>
        /// Lookup for all <see cref="ISequentialTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        protected readonly CRTPList<ITransport>.Lookup<ISequentialTransport> SequentialTransports;
        /// <summary>
        /// Lookup for all <see cref="IUnreliableTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        protected readonly CRTPList<ITransport>.Lookup<IUnreliableTransport> UnreliableTransports;
        /// <summary>
        /// Lookup for all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        protected readonly CRTPList<ITransport>.Lookup<IStreamTransport> StreamTransports;
        /// <summary>
        /// Lookup for all <see cref="IStreamTransport"/>s this <see cref="NetworkMember"/> manages.
        /// </summary>
        protected readonly CRTPList<ITransport>.Lookup<IFileTransport> FileTransports;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                            Transport Management
        /// .                     Note: registration methods are provided via auto-gen methods.
        /// .                  TODO: Only terminate transport if it was removed from ALL other storages.
        /// .       Note: this might be easier to resolve if we provide custom storage solution, allowing for filtering.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region Any transport management
        /// <summary>
        /// Check whether transport of a specified type is registered.
        /// </summary>
        public bool HasTransport<TTransport>() where TTransport : ITransport
        {
            lock (_lock) return Transports.Has<TTransport>();
        }

        /// <inheritdoc cref="HasTransport{TTransport}()"/>
        protected bool HasTransportCore<TTransport>() where TTransport : ITransport
        {
            return Transports.Has<TTransport>();
        }

        /// <summary>
        /// Checks whether transport of a specified type is registered and belongs to a specific sending mode group.
        /// </summary>
        public bool HasTransport<TTransport>(SendingMode mode) where TTransport : ITransport
        {
            lock (_lock) return mode switch
            {
                SendingMode.Unreliable => UnreliableTransports.Has<TTransport>(),
                SendingMode.Reliable => ReliableTransports.Has<TTransport>(),
                SendingMode.Sequential => SequentialTransports.Has<TTransport>(),
                SendingMode.Resilient => ResilientTransports.Has<TTransport>(),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <inheritdoc cref="HasTransport{TTransport}(SendingMode)"/>
        //protected bool HasTransportCore<TTransport>(SendingMode mode) where TTransport : ITransport => mode switch
        //{
        //    SendingMode.Unreliable => UnreliableTransports.Has<TTransport>(),
        //    SendingMode.Reliable => ReliableTransports.Has<TTransport>(),
        //    SendingMode.Sequential => SequentialTransports.Has<TTransport>(),
        //    SendingMode.Resilient => ResilientTransports.Has<TTransport>(),
        //    _ => throw new SwitchExpressionException(mode),
        //};

        /// <summary>
        /// Checks if there is any transport, at all, than will be able to handle a given <see cref="SendingMode"/>.
        /// Used in broadcasting TrySend methods.
        /// </summary>
        public bool HasAnyTransport(SendingMode mode)
        {
            lock (_lock) return mode switch
            {
                SendingMode.Unreliable => UnreliableTransports.Count > 0,
                SendingMode.Reliable => ReliableTransports.Count > 0,
                SendingMode.Sequential => SequentialTransports.Count > 0,
                SendingMode.Resilient => ResilientTransports.Count > 0,
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <inheritdoc cref="HasAnyTransport(SendingMode)"/>
        //public bool HasAnyTransportCore(SendingMode mode) => mode switch
        //{
        //    SendingMode.Unreliable => UnreliableTransports.Count > 0,
        //    SendingMode.Reliable => ReliableTransports.Count > 0,
        //    SendingMode.Sequential => SequentialTransports.Count > 0,
        //    SendingMode.Resilient => ResilientTransports.Count > 0,
        //    _ => throw new SwitchExpressionException(mode),
        //};
        #endregion
    }
}
