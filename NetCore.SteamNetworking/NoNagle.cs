namespace NetCore.SteamNetworking
{
    /// <summary>
    /// Steam Networking specific.
    /// Indicates that message should be sent without using Nagle's algorithm whenever possible.
    /// </summary>
    /// <remarks>
    /// Nagle's algorithm holds some messages together for a bit, to send them in bulk.
    /// Optimizes networking when sending a lot of messages frequently.
    /// But introduces a few milliseconds of delay.
    /// </remarks>
    public readonly struct NoNagle : INoContentFlag { }
}
