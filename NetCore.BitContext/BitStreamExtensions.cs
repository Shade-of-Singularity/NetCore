using ComputerysBitStream;
using System;
using System.Runtime.CompilerServices;

namespace NetCore.BitContext
{
    /// <summary>
    /// Useful extensions for working with <see cref="ComputerysBitStream"/>.
    /// </summary>
    /// Note: some of the methods can be packed in a PR and merged with the original repo.
    public static class BitStreamExtensions
    {
        /// <summary>
        /// Gets a span of bytes representing the written data in the buffer.
        /// </summary>
        /// <inheritdoc cref="WriteContext.ToByte"/>
        public static Span<byte> ToBytesClean(this scoped in WriteContext context)
        {
            // This might contain garbage bits in the last byte of the span.
            var span = context.ToByte();

            const uint ByteSizeMask = 0b111; // Mask, covering values from 0 to 7, representing a max offset a bit can have.
            switch (context.Position & ByteSizeMask)
            {
                // Note (premature optimization): we sacrifice one branch (~15 cycles) to make sure
                //  to not touch the array at all, as array access should be a lot more expensive than a branch check.
                // Note #2: jump-table optimized switch statement will still have at least one branch for an upper bound check.
                case 0: break; // No need for cleaning.

                case 1: span[^1] &= 0b0000_0001; break;
                case 2: span[^1] &= 0b0000_0011; break;
                case 3: span[^1] &= 0b0000_0111; break;
                case 4: span[^1] &= 0b0000_1111; break;
                case 5: span[^1] &= 0b0001_1111; break;
                case 6: span[^1] &= 0b0011_1111; break;
                case 7: span[^1] &= 0b0111_1111; break;

                default: throw new SwitchExpressionException(context.Position & ByteSizeMask);
            }

            return span;
        }
    }
}
