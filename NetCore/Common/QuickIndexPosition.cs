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
        Three = 2,
        Four = 4,
        Five = 6,
        Six = 9,
        Seven = 12,
        Eight = 16,
        Nine = 20,
        Ten = 25,
        Eleven = 30,
        Twelve = 36,
        Thirteen = 42,
        Fourteen = 49,
        Fifteen = 56,
    }
}