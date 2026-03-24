namespace NetCore
{
    /// <summary>
    /// Describes whether message is a request or response.
    /// Both will also define a <see cref="RequestIDHeader"/> in <see cref="Header"/>.
    /// </summary>
    public enum RequestType : byte
    {
        /// <summary>
        /// Tells that this is a request message.
        /// </summary>
        Request = 0,
        /// <summary>
        /// Tells that this is a response message.
        /// </summary>
        Response = 1,
    }
}
