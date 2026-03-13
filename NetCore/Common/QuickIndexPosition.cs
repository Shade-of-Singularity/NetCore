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
        Two = 0,
        Three = 1,
        Four = 3,
        Five = 5,
        Six = 8,
        Seven = 11,
        Eight = 14,
        Nine = 17,
        Ten = 21,
        Eleven = 25,
        Twelve = 29,
        Thirteen = 33,
        Fourteen = 37,
        Fifteen = 41,
        Sixteen = 45,
        Seventeen = 49,
        Eightteen = 54,
        Nineteen = 59,
    }
}