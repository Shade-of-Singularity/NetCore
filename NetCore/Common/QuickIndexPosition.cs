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
        One = 1,
        Two = 2,
        Three = 4,
        Four = 6,
        Five = 9,
        Six = 12,
        Seven = 16,
        Eight = 20,
        Nine = 25,
        Ten = 30,
        Eleven = 36,
        Twelve = 42,
        Thirteen = 49,
    }
}