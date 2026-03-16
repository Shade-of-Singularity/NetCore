using System;

namespace NetCore
{
    /// <summary>
    /// Static identifier for a thing, which does not change no matter the environment.
    /// Elegantly encodes in a Base64/Base128/Base192 for transporting.
    /// </summary>
    public readonly struct StaticID
    {
        /// <summary>
        /// Size of a static identifier in a Base64 form.
        /// </summary>
        public const int Base64Length = 32;
        /// <summary>
        /// Size of a static identifier in a Base256 form.
        /// </summary>
        public const int Base256Length = 24;
        /// <summary>
        /// <see cref="StaticID"/> encoded in a Base64 format.
        /// </summary>
        public readonly byte[] Base64;
        /// <summary>
        /// <see cref="StaticID"/> encoded in a Base256 format.
        /// </summary>
        public readonly byte[] Base256;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private StaticID(byte[] base64, byte[] base256)
        {

        }

        /// <summary>
        /// Creates a static ID from a set of input values.
        /// </summary>
        public static StaticID From<T1>(T1 core)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="From{T1}(T1)"/>
        public static StaticID From<T1, T2>(T1 core, T2 arg2)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="From{T1}(T1)"/>
        public static StaticID From<T1, T2, T3>(T1 core, T2 arg2, T3 arg3)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="From{T1}(T1)"/>
        public static StaticID From<T1, T2, T3, T4>(T1 core, T2 arg2, T3 arg3, T4 arg4)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="From{T1}(T1)"/>
        public static StaticID From<T1, T2, T3, T4, T5>(T1 core, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="From{T1}(T1)"/>
        public static StaticID From<T1, T2, T3, T4, T5, T6>(T1 core, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            throw new NotImplementedException();
        }
    }
}
