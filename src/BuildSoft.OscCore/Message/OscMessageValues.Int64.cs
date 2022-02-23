using System.Net;
using System.Runtime.CompilerServices;

namespace BuildSoft.OscCore;

public sealed unsafe partial class OscMessageValues
{
    /// <summary>
    /// Read a single 64-bit integer (long) message element.
    /// Checks the element type before reading and throw <see cref="InvalidOperationException"/> if it's not interpretable as a long.
    /// </summary>
    /// <param name="index">The element index</param>
    /// <returns>The value of the element</returns>
    public long ReadInt64Element(int index)
    {
#if OSCCORE_SAFETY_CHECKS
        if (OutOfBounds(index)) return default;
#endif
        var offset = _offsets[index];
        switch (_tags[index])
        {
            case TypeTag.Int64:
                // TODO - optimize
                long bigEndian = *(_sharedBufferPtr + offset);
                return IPAddress.NetworkToHostOrder(bigEndian);
            case TypeTag.Int32:
                return _sharedBuffer[offset] << 24 |
                       _sharedBuffer[offset + 1] << 16 |
                       _sharedBuffer[offset + 2] << 8 |
                       _sharedBuffer[offset + 3];
            case TypeTag.Float64:
                _swapBuffer64[7] = _sharedBuffer[offset];
                _swapBuffer64[6] = _sharedBuffer[offset + 1];
                _swapBuffer64[5] = _sharedBuffer[offset + 2];
                _swapBuffer64[4] = _sharedBuffer[offset + 3];
                _swapBuffer64[3] = _sharedBuffer[offset + 4];
                _swapBuffer64[2] = _sharedBuffer[offset + 5];
                _swapBuffer64[1] = _sharedBuffer[offset + 6];
                _swapBuffer64[0] = _sharedBuffer[offset + 7];
                double d = *_swapBuffer64Ptr;
                return (long)d;
            case TypeTag.Float32:
                _swapBuffer32[0] = _sharedBuffer[offset + 3];
                _swapBuffer32[1] = _sharedBuffer[offset + 2];
                _swapBuffer32[2] = _sharedBuffer[offset + 1];
                _swapBuffer32[3] = _sharedBuffer[offset];
                float f = *_swapBuffer32Ptr;
                return (long)f;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Read a single 64-bit integer (long) message element, without checking the type tag of the element.
    /// Only call this if you are really sure that the element at the given index is a valid long,
    /// as the performance difference is small.
    /// </summary>
    /// <param name="index">The element index</param>
    /// <returns>The value of the element</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64ElementUnchecked(int index)
    {
#if OSCCORE_SAFETY_CHECKS
        if (OutOfBounds(index)) return default;
#endif
        long bigEndian = *(_sharedBufferPtr + _offsets[index]);
        return IPAddress.NetworkToHostOrder(bigEndian);
    }
}
