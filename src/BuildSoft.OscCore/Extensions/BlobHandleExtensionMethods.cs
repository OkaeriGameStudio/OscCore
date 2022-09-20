using System;
using System.Collections.Generic;
using System.Text;

namespace BlobHandles;

#if NETSTANDARD2_1_OR_GREATER
/// <summary>
/// Extension methods for <see cref="BlobHandle"/> or <see cref="BlobString"/>.
/// </summary>
public static class BlobHandleExtensionMethods
{
    public static unsafe ReadOnlySpan<byte> AsSpan(this BlobHandle blobHandle) => new(blobHandle.Pointer, blobHandle.Length);
    public static unsafe ReadOnlySpan<byte> AsSpan(this BlobString blobString) => blobString.Handle.AsSpan();
}
#endif
