using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BuildSoft.OscCore.UnityObjects;


// allow tests to modify things as if in the same assembly
[assembly: InternalsVisibleTo("OscCore.Tests.Editor")]

namespace BuildSoft.OscCore;

/// <summary>
/// Represents the tags and values associated with a received OSC message
/// </summary>
public sealed unsafe partial class OscMessageValues
{
    // the buffer where we read messages from - usually provided + filled by a socket reader
    readonly byte[] _sharedBuffer;
    readonly byte* _sharedBufferPtr;
    // used to swap bytes for 32-bit numbers when reading
    readonly byte[] _swapBuffer32 = new byte[4];
    readonly float* _swapBuffer32Ptr;
    readonly uint* _swapBuffer32UintPtr;
    readonly Color32* _swapBufferColor32Ptr;
    readonly GCHandle _swap32Handle;
    // used to swap bytes for 64-bit numbers when reading
    readonly byte[] _swapBuffer64 = new byte[8];
    readonly double* _swapBuffer64Ptr;
    readonly GCHandle _swap64Handle;

    /// <summary>
    /// All type tags in the message.
    /// All values past index >= ElementCount are junk data and should NEVER BE USED!
    /// </summary>
    internal readonly TypeTag[] _tags;

    /// <summary>
    /// Indexes into the shared buffer associated with each message element
    /// All values at index >= ElementCount are junk data and should NEVER BE USED!
    /// </summary>
    internal readonly int[] _offsets;

    /// <summary>The number of elements in the OSC Message</summary>
    public int ElementCount { get; internal set; }

    internal OscMessageValues(byte[] buffer, int elementCapacity = 8)
    {
        ElementCount = 0;
        _tags = new TypeTag[elementCapacity];
        _offsets = new int[elementCapacity];
        _sharedBuffer = buffer;

        fixed (byte* bufferPtr = buffer) { _sharedBufferPtr = bufferPtr; }

        // pin byte swap buffers in place, so that we can count on the pointers never changing
        _swap32Handle = GCHandle.Alloc(_swapBuffer32, GCHandleType.Pinned);
        var swap32Ptr = _swap32Handle.AddrOfPinnedObject();
        _swapBuffer32Ptr = (float*)swap32Ptr;
        _swapBuffer32UintPtr = (uint*)swap32Ptr;
        _swapBufferColor32Ptr = (Color32*)(byte*)swap32Ptr;

        _swapBuffer64Ptr = Utils.PinPtr<byte, double>(_swapBuffer64, out _swap64Handle);
    }

    ~OscMessageValues()
    {
        _swap32Handle.Free();
        _swap64Handle.Free();
    }

    /// <summary>Execute a method for every element in the message</summary>
    /// <param name="elementAction">A method that takes in the index and type tag for an element</param>
    public void ForEachElement(Action<int, TypeTag> elementAction)
    {
        for (int i = 0; i < ElementCount; i++)
            elementAction(i, _tags[i]);
    }

    /// <summary>
    /// Get a <see cref="TypeTag"/> corresponding to <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Index of <see cref="TypeTag"/> you want to get</param>
    /// <returns></returns>
    public TypeTag GetTypeTag(int index)
    {
#if OSCCORE_SAFETY_CHECKS
        if (OutOfBounds(index)) return default;
#endif
        return _tags[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool OutOfBounds(int index)
    {
        if (index >= ElementCount)
        {
            Debug.Fail($"Tried to read message element index {index}, but there are only {ElementCount} elements");
            return true;
        }

        return false;
    }
}
