namespace NetCore.SteamNetworking
{
    /// <summary>
    /// Used with <see cref="SteamTransport"/> on message sending.
    /// </summary>
    /// <remarks>
    /// Asks <see cref="Steamworks"/> to use current thread for sending the message.
    /// </remarks>
    public readonly struct UseCurrentThread : INoContentFlag { }
}
