using System.Net;
using System.Runtime.CompilerServices;

namespace BuildSoft.OscCore;

public sealed unsafe partial class OscMessageValues
{
    /// <summary>
    /// Read a single 64-bit float (double) message element.
    /// Checks the element type before reading & returns 0 if it's not interpretable as a double.
    /// </summary>
    /// <param name="index">The element index</param>
    /// <returns>The value of the element</returns>
    public double ReadFloat64Element(int index)
    {
#if OSCCORE_SAFETY_CHECKS
        if (OutOfBounds(index)) return default;
#endif
        var offset = _offsets[index];
        switch (_tags[index])
        {
            case TypeTag.Float64:
                _swapBuffer64[7] = _sharedBuffer[offset];
                _swapBuffer64[6] = _sharedBuffer[offset + 1];
                _swapBuffer64[5] = _sharedBuffer[offset + 2];
                _swapBuffer64[4] = _sharedBuffer[offset + 3];
                _swapBuffer64[3] = _sharedBuffer[offset + 4];
                _swapBuffer64[2] = _sharedBuffer[offset + 5];
                _swapBuffer64[1] = _sharedBuffer[offset + 6];
                _swapBuffer64[0] = _sharedBuffer[offset + 7];
                return *_swapBuffer64Ptr;
            case TypeTag.Float32:
                _swapBuffer32[0] = _sharedBuffer[offset + 3];
                _swapBuffer32[1] = _sharedBuffer[offset + 2];
                _swapBuffer32[2] = _sharedBuffer[offset + 1];
                _swapBuffer32[3] = _sharedBuffer[offset];
                return *_swapBuffer32Ptr;
            case TypeTag.Int64:
                long bigEndian = *(_sharedBufferPtr + offset);
                return IPAddress.NetworkToHostOrder(bigEndian);
            case TypeTag.Int32:
                return _sharedBuffer[index] << 24 |
                       _sharedBuffer[index + 1] << 16 |
                       _sharedBuffer[index + 2] << 8 |
                       _sharedBuffer[index + 3];
        }

        return default;
    }

    /// <summary>
    /// Read a single 64-bit float (double) message element, without checking the type tag of the element.
    /// Only call this if you are really sure that the element at the given index is a valid double,
    /// as the performance difference is small.
    /// </summary>
    /// <param name="index">The element index</param>
    /// <returns>The value of the element</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadFloat64ElementUnchecked(int index)
    {
#if OSCCORE_SAFETY_CHECKS
        if (OutOfBounds(index)) return default;
#endif
        var offset = _offsets[index];
        _swapBuffer64[7] = _sharedBuffer[offset];
        _swapBuffer64[6] = _sharedBuffer[offset + 1];
        _swapBuffer64[5] = _sharedBuffer[offset + 2];
        _swapBuffer64[4] = _sharedBuffer[offset + 3];
        _swapBuffer64[3] = _sharedBuffer[offset + 4];
        _swapBuffer64[2] = _sharedBuffer[offset + 5];
        _swapBuffer64[1] = _sharedBuffer[offset + 6];
        _swapBuffer64[0] = _sharedBuffer[offset + 7];
        return *_swapBuffer64Ptr;
    }
}
