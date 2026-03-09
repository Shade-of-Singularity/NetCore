namespace NetCore.Common
{
    /// <summary>
    /// How much bits you need to offset an <see cref="QuickIndexMask"/> by to unpack your index.
    /// </summary>
    /// <remarks>
    /// Can be easily packed as one char in 'Base64'
    /// </remarks>
    public enum QuickIndexPosition : byte
    {
        One = 0,
        Two = 1,
        Three = 3,
        Four = 5,
        Five = 8,
        Six = 11,
        Seven = 15,
        Eight = 19,
        Nine = 24,
        Ten = 29,
        Eleven = 35,
        Twelve = 41,
        Thirteen = 48,
    }
}