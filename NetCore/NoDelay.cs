namespace NetCore
{
    /// <summary>
    /// Steam Networking specific.
    /// Indicates that message should be sent without delays whenever possible.
    /// </summary>
    public readonly struct NoDelay : INoContentFlag { }
}
