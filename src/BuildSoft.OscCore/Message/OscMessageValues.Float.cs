using System.Runtime.CompilerServices;

namespace BuildSoft.OscCore;

public sealed unsafe partial class OscMessageValues
{
    /// <summary>
    /// Read a single 32-bit float message element.
    /// Checks the element type before reading and throw <see cref="InvalidOperationException"/> if it's not interpretable as a float.
    /// </summary>
    /// <param name="index">The element index</param>
    /// <returns>The value of the element</returns>
    public float ReadFloatElement(int index)
    {
#if OSCCORE_SAFETY_CHECKS
        if (OutOfBounds(index)) return default;
#endif
        var offset = _offsets[index];
        switch (_tags[index])
        {
            case TypeTag.Float32:
                _swapBuffer32[0] = _sharedBufferPtr[offset + 3];
                _swapBuffer32[1] = _sharedBufferPtr[offset + 2];
                _swapBuffer32[2] = _sharedBufferPtr[offset + 1];
                _swapBuffer32[3] = _sharedBufferPtr[offset];
                return *_swapBuffer32Ptr;
            case TypeTag.Int32:
                return _sharedBufferPtr[index] << 24 |
                       _sharedBufferPtr[index + 1] << 16 |
                       _sharedBufferPtr[index + 2] << 8 |
                       _sharedBufferPtr[index + 3];
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Read a single 32-bit float message element, without checking the type tag of the element.
    /// Only call this if you are really sure that the element at the given index is a valid float,
    /// as the performance difference is small.
    /// </summary>
    /// <param name="index">The element index</param>
    /// <returns>The value of the element</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloatElementUnchecked(int index)
    {
#if OSCCORE_SAFETY_CHECKS
        if (OutOfBounds(index)) return default;
#endif
        var offset = _offsets[index];
        _swapBuffer32[0] = _sharedBufferPtr[offset + 3];
        _swapBuffer32[1] = _sharedBufferPtr[offset + 2];
        _swapBuffer32[2] = _sharedBufferPtr[offset + 1];
        _swapBuffer32[3] = _sharedBufferPtr[offset];
        return *_swapBuffer32Ptr;
    }
}
