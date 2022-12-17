using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlobHandles;

/// <summary>
/// Wraps an arbitrary chunk of bytes in memory, so it can be used as a hash key
/// and compared against other instances of the same set of bytes 
/// </summary>
public readonly unsafe struct BlobHandle : IEquatable<BlobHandle>
{
    /// <summary>A pointer to the start of the blob</summary>
    public byte* Pointer
    {
        get
        {
            if (_pointer != null)
            {
                return _pointer;
            }
            fixed (byte* pointer = &_bytes[_offset])
            {
                return pointer;
            }
        }
    }

    private readonly byte* _pointer;
    private readonly byte[] _bytes;
    private readonly int _offset;

    /// <summary>The number of bytes in the blob</summary>
    public readonly int Length;

    public BlobHandle(byte* pointer, int length)
    {
        _pointer = pointer;
        Length = length;
    }

    public BlobHandle(IntPtr pointer, int length)
    {
        _pointer = (byte*)pointer;
        Length = length;
    }

    /// <summary>
    /// Get a blob handle for a byte array. The byte array should have its address pinned to work safely!
    /// </summary>
    /// <param name="bytes">The bytes to get a handle to</param>
    public BlobHandle(byte[] bytes)
    {
        _bytes = bytes;
        Length = bytes.Length;
    }

    /// <summary>
    /// Get a blob handle for part of a byte array. The byte array should have its address pinned to work safely!
    /// </summary>
    /// <param name="bytes">The bytes to get a handle to</param>
    /// <param name="length">The number of bytes to include. Not bounds checked</param>
    public BlobHandle(byte[] bytes, int length)
    {
        _bytes = bytes;
        Length = length;
    }

    /// <summary>
    /// Get a blob handle for a slice of a byte array. The byte array should have its address pinned to work safely!
    /// </summary>
    /// <param name="bytes">The bytes to get a handle to</param>
    /// <param name="length">The number of bytes to include. Not bounds checked</param>
    /// <param name="offset">The byte array index to start the blob at</param>
    public BlobHandle(byte[] bytes, int length, int offset)
    {
        _bytes = bytes;
        Length = length;
        _offset = offset;
    }

    public override string ToString()
    {
        return $"{Length} bytes @ {new IntPtr(Pointer)}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        unchecked
        {
            return Length * 397 ^ Pointer[Length - 1];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(BlobHandle other)
    {
        return Length == other.Length &&
               MemoryCompare(Pointer, other.Pointer, (UIntPtr)Length) == 0;
    }

    public override bool Equals(object obj)
    {
        return obj is BlobHandle other && Equals(other);
    }

    public static bool operator ==(BlobHandle left, BlobHandle right)
    {
        return left.Length == right.Length &&
               MemoryCompare(left.Pointer, right.Pointer, (UIntPtr)left.Length) == 0;
    }

    public static bool operator !=(BlobHandle left, BlobHandle right)
    {
        return left.Length != right.Length ||
               MemoryCompare(left.Pointer, right.Pointer, (UIntPtr)left.Length) != 0;
    }

    private static int MemoryCompare(void* ptr1, void* ptr2, UIntPtr count)
    {
        var p1 = (byte*)ptr1;
        var p2 = (byte*)ptr1;
        for (int i = 0; i < (int)count; i++)
        {
            if (p1[i] != p2[i])
            {
                return 1;
            }
        }
        return 0;
    }
}
