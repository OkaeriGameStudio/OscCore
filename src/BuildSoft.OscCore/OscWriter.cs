using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlobHandles;
using BuildSoft.OscCore.UnityObjects;
using MiniNtp;

namespace BuildSoft.OscCore;

public sealed unsafe class OscWriter : IDisposable
{
    public readonly byte[] Buffer;
    readonly byte* _bufferPtr;
    readonly GCHandle _bufferHandle;
    readonly MidiMessage* _bufferMidiPtr;

    readonly float[] _floatSwap = new float[1];
    readonly byte* _floatSwapPtr;
    readonly GCHandle _floatSwapHandle;

    readonly double[] _doubleSwap = new double[1];
    readonly byte* _doubleSwapPtr;
    readonly GCHandle _doubleSwapHandle;

    readonly Color32[] _color32Swap = new Color32[1];
    readonly byte* _color32SwapPtr;
    readonly GCHandle _color32SwapHandle;

    int _length;

    /// <summary>The number of bytes currently written to the buffer</summary>
    public int Length => _length;

    public OscWriter(int capacity = 4096)
    {
        Buffer = new byte[capacity];

        // Even though Unity's GC does not move objects around, pin them to be safe.
        _bufferPtr = Utils.PinPtr<byte, byte>(Buffer, out _bufferHandle);
        _bufferMidiPtr = (MidiMessage*)_bufferPtr;

        _floatSwapPtr = Utils.PinPtr<float, byte>(_floatSwap, out _floatSwapHandle);
        _doubleSwapPtr = Utils.PinPtr<double, byte>(_doubleSwap, out _doubleSwapHandle);
        _color32SwapPtr = Utils.PinPtr<Color32, byte>(_color32Swap, out _color32SwapHandle);
    }

    ~OscWriter() { Dispose(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() { _length = 0; }

    /// <summary>Write a 32-bit integer element</summary>
    public void Write(int data)
    {
        Buffer[_length++] = (byte)(data >> 24);
        Buffer[_length++] = (byte)(data >> 16);
        Buffer[_length++] = (byte)(data >> 8);
        Buffer[_length++] = (byte)(data);
    }

    /// <summary>Write a 32-bit floating point element</summary>
    public void Write(float data)
    {
        _floatSwap[0] = data;
        Buffer[_length++] = _floatSwapPtr[3];
        Buffer[_length++] = _floatSwapPtr[2];
        Buffer[_length++] = _floatSwapPtr[1];
        Buffer[_length++] = _floatSwapPtr[0];
    }

    /// <summary>Write a 2D vector as two float elements</summary>
    public void Write(Vector2 data)
    {
        Write(data.x);
        Write(data.y);
    }

    /// <summary>Write a 3D vector as three float elements</summary>
    public void Write(Vector3 data)
    {
        Write(data.x);
        Write(data.y);
        Write(data.z);
    }

    /// <summary>Write an ASCII string element. The string MUST be ASCII-encoded!</summary>
    public void Write(string data)
    {
        foreach (var chr in data)
            Buffer[_length++] = (byte)chr;

        var alignedLength = (data.Length + 3) & ~3;
        // if our length was already aligned to 4 bytes, that means we don't have a string terminator yet,
        // so we need to write one, which requires aligning to the next 4-byte mark.
        if (alignedLength == data.Length)
            alignedLength += 4;

        for (int i = data.Length; i < alignedLength; i++)
            Buffer[_length++] = 0;
    }

    /// <summary>Write an ASCII string element. The string MUST be ASCII-encoded!</summary>
    public void Write(BlobString data)
    {
        var strLength = data.Length;
        System.Buffer.MemoryCopy(data.Handle.Pointer, _bufferPtr + _length, strLength, strLength);
        _length += strLength;

        var alignedLength = (data.Length + 3) & ~3;
        if (alignedLength == data.Length)
            alignedLength += 4;

        for (int i = data.Length; i < alignedLength; i++)
            Buffer[_length++] = 0;
    }

    /// <summary>Write a blob element</summary>
    /// <param name="bytes">The bytes to copy from</param>
    /// <param name="length">The number of bytes in the blob element</param>
    /// <param name="start">The index in the bytes array to start copying from</param>
    public void Write(byte[] bytes, int length, int start = 0)
    {
        if (start + length > bytes.Length)
            return;

        Write(length);
        System.Buffer.BlockCopy(bytes, start, Buffer, _length, length);
        _length += length;

        // write any trailing zeros necessary
        var remainder = ((length + 3) & ~3) - length;
        for (int i = 0; i < remainder; i++)
        {
            Buffer[_length++] = 0;
        }
    }

    /// <summary>Write a 64-bit integer element</summary>
    public void Write(long data)
    {
        var buffer = Buffer;
        buffer[_length++] = (byte)(data >> 56);
        buffer[_length++] = (byte)(data >> 48);
        buffer[_length++] = (byte)(data >> 40);
        buffer[_length++] = (byte)(data >> 32);
        buffer[_length++] = (byte)(data >> 24);
        buffer[_length++] = (byte)(data >> 16);
        buffer[_length++] = (byte)(data >> 8);
        buffer[_length++] = (byte)(data);
    }

    /// <summary>Write a 64-bit floating point element</summary>
    public void Write(double data)
    {
        var buffer = Buffer;
        _doubleSwap[0] = data;
        var dsPtr = _doubleSwapPtr;
        buffer[_length++] = dsPtr[7];
        buffer[_length++] = dsPtr[6];
        buffer[_length++] = dsPtr[5];
        buffer[_length++] = dsPtr[4];
        buffer[_length++] = dsPtr[3];
        buffer[_length++] = dsPtr[2];
        buffer[_length++] = dsPtr[1];
        buffer[_length++] = dsPtr[0];
    }

    /// <summary>Write a 32-bit RGBA color element</summary>
    public void Write(Color32 data)
    {
        _color32Swap[0] = data;
        Buffer[_length++] = _color32SwapPtr[3];
        Buffer[_length++] = _color32SwapPtr[2];
        Buffer[_length++] = _color32SwapPtr[1];
        Buffer[_length++] = _color32SwapPtr[0];
    }

    /// <summary>Write a MIDI message element</summary>
    public void Write(MidiMessage data)
    {
        var midiWritePtr = (MidiMessage*)(_bufferPtr + _length);
        *midiWritePtr = data;
        _length += 4;
    }

    /// <summary>Write a 64-bit NTP timestamp element</summary>
    public void Write(NtpTimestamp time)
    {
        time.ToBigEndianBytes((uint*)(_bufferPtr + _length));
        _length += 8;
    }

    /// <summary>Write a single ascii character element</summary>
    public void Write(char data)
    {
        // char is written in the last byte of the 4-byte block;
        Buffer[_length + 3] = (byte)data;
        _length += 4;
    }

    /// <summary>Write '#bundle ' at the start of a bundled message</summary>
    public void WriteBundlePrefix()
    {
        const int size = 8;
        // TODO replace with dereferencing the long  version ?
        System.Buffer.BlockCopy(Constant.BundlePrefixBytes, 0, Buffer, _length, size);
        _length += size;
    }

    /// <summary>
    /// Combines Reset(), Write(address), and Write(tags) in a single function to reduce call overhead
    /// </summary>
    /// <param name="address">The OSC address to send to</param>
    /// <param name="tags">4 bytes that represent up to 3 type tags</param>
    public void WriteAddressAndTags(string address, uint tags)
    {
        _length = 0;
        foreach (var chr in address)
            Buffer[_length++] = (byte)chr;

        var alignedLength = (address.Length + 3) & ~3;
        // if our length was already aligned to 4 bytes, that means we don't have a string terminator yet,
        // so we need to write one, which requires aligning to the next 4-byte mark.
        if (alignedLength == address.Length)
            alignedLength += 4;

        for (int i = address.Length; i < alignedLength; i++)
            Buffer[_length++] = 0;

        // write the 4 bytes for the type tags
        ((uint*)(_bufferPtr + _length))[0] = tags;
        _length += 4;
    }

    public void CopyBuffer(byte[] copyTo, int copyOffset = 0)
    {
        System.Buffer.BlockCopy(Buffer, 0, copyTo, copyOffset, Length);
    }

    public void Dispose()
    {
        _bufferHandle.SafeFree();
        _color32SwapHandle.SafeFree();
        _floatSwapHandle.SafeFree();
        _doubleSwapHandle.SafeFree();
    }
}
