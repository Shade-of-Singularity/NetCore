using System;

namespace NetCore
{
    /// <summary>
    /// Indicates that there was not enough buffer space to contain some kind of data.
    /// </summary>
    public sealed class InsufficientBufferException(string argument, int has, int need)
        : Exception($"{typeof(InsufficientBufferException).FullName}: Buffer ({argument}) is too small. Provided: ({has}). Need: ({need})")
    { }
}
