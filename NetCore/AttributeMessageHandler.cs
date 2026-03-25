namespace NetCore
{
    /// <summary>
    /// Storage for all attribute-based message handlers.
    /// </summary>
    public sealed class AttributeMessageHandler : MessageHandler
    {
        /// <summary>
        /// Global handler for all general type of messages.
        /// Caches message handler targets prematurely.
        /// </summary>
        public static new readonly AttributeMessageHandler Global = (AttributeMessageHandler)new AttributeMessageHandler().Initialize();
    }
}
