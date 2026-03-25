using System;

namespace NetCore
{
    /// <summary>
    /// Thrown when you try to use an <see cref="MessageHandler"/> without running <see cref="MessageHandler.Initialize"/> on it first.
    /// </summary>
    /// <param name="type">Type of the <see cref="MessageHandler"/>.</param>
    public sealed class HandlerUninitializedException(Type type) : Exception($"{type.Name} is was not initialized before use.");
    /// <summary>
    /// Message handler class for processing arrived messages.
    /// </summary>
    public class MessageHandler
    {
        /// <summary>
        /// Global handler for all general type of messages.
        /// Caches message handler targets prematurely.
        /// </summary>
        public static MessageHandler Global => AttributeMessageHandler.Global;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Whether <see cref="MessageHandler"/> is initialized or not.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                lock (_lock) return m_IsInitialized;
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Lock used for asynchronous handling of the messages.
        /// </summary>
        protected readonly object _lock = new();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private volatile bool m_IsInitialized;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Initializes this <see cref="MessageHandler"/>.
        /// </summary>
        /// <returns>Itself.</returns>
        public MessageHandler Initialize()
        {
            lock (_lock)
            {
                if (!m_IsInitialized)
                {
                    m_IsInitialized = false;
                    InitializeOperation();
                }

                return this;
            }
        }

        /// <summary>
        /// Handles and routes a <paramref name="datagram"/> from a remote connection.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram"></param>
        /// <param name="fromConnection"></param>
        public virtual void HandleMessage(in Header header, in ReadOnlySpan<byte> datagram, ConnectionID fromConnection)
        {

        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        protected virtual void InitializeOperation()
        {

        }
    }
}
