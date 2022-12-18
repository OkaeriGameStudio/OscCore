using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace BuildSoft.OscCore.Tests;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ConvertBuffer
{
    [FieldOffset(0)]
    public fixed byte Bytes[8];
    [FieldOffset(0)]
    public readonly int @int;
    [FieldOffset(0)]
    public readonly long @long;
    [FieldOffset(0)]
    public readonly float @float;
    [FieldOffset(0)]
    public readonly double @double;

    public byte[] GetReversedBytes(int size)
    {
        Debug.Assert(size > 0 && size <= 8);
        fixed (byte* p = Bytes)
        {
            return TestUtil.ReversedCopy(p, size);
        }
    }
}

public class MessageReadPerformanceTests
{
    private const int Count = 4096;
    private static readonly Stopwatch _stopwatch = new();
    private readonly ConvertBuffer[] _buffers = new ConvertBuffer[Count];
    private readonly byte[] _midiSourceBytes = TestUtil.RandomMidiBytes(Count * 4);
    private readonly byte[] _timeSourceBytes = TestUtil.RandomTimestampBytes(Count * 4);

    [OneTimeSetUp]
    public unsafe void BeforeAll()
    {
        for (int i = 0; i < _buffers.Length; i++)
        {
            fixed (byte* bytes = _buffers[i].Bytes)
            {
                for (int j = 0; j < 8; j++)
                {
                    bytes[j] = unchecked((byte)TestUtil.SharedRandom.Next());
                }
            }
        }
    }

    [SetUp]
    public void BeforeEach()
    {

    }

    private static OscMessageValues FromBytes(byte[] bytes, int count, TypeTag tag, int byteSize = 4)
    {
        var values = new OscMessageValues(bytes, count);
        for (int i = 0; i < count; i++)
        {
            values._offsets[i] = i * byteSize;
            values._tags[i] = tag;
        }

        values.ElementCount = count;
        return values;
    }

    private static OscMessageValues FromBytes(ConvertBuffer[] buffers, int count, TypeTag tag, int byteSize)
    {
        var values = new OscMessageValues(buffers.SelectMany(s => s.GetReversedBytes(byteSize)).ToArray(), count);
        for (int i = 0; i < count; i++)
        {
            values._offsets[i] = i * byteSize;
            values._tags[i] = tag;
        }

        values.ElementCount = count;
        return values;
    }

    [Test]
    public void ReadFloatElement_CheckedVsUnchecked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Float32, sizeof(float));

        float value = 0f;
        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            value = values.ReadFloatElement(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, checked float32 element read: {_stopwatch.ElapsedTicks} ticks, last value {value}");

        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            value = values.ReadFloatElementUnchecked(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, unchecked float32 element read: {_stopwatch.ElapsedTicks} ticks, last value {value}");
    }

    [Test]
    public void ReadFloatElement_Checked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Float32, sizeof(float));

        for (int i = 0; i < count; i++)
        {
            Assert.AreEqual(_buffers[i].@float, values.ReadFloatElement(i));
            Assert.AreEqual((double)_buffers[i].@float, values.ReadFloat64Element(i));
            Assert.AreEqual((int)_buffers[i].@float, values.ReadIntElement(i));
            Assert.AreEqual((long)_buffers[i].@float, values.ReadInt64Element(i));
            Assert.AreEqual(_buffers[i].@float.ToString(), values.ReadStringElement(i));
        }
    }

    [Test]
    public void ReadFloatElement_Unchecked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Float32, sizeof(float));

        for (int i = 0; i < count; i++)
        {
            Assert.AreEqual(_buffers[i].@float, values.ReadFloatElementUnchecked(i));
        }
    }

    [Test]
    public void ReadFloat64Element_Checked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Float64, sizeof(double));

        for (int i = 0; i < count; i++)
        {
            Assert.AreEqual(_buffers[i].@double, values.ReadFloat64Element(i));
            Assert.AreEqual((long)_buffers[i].@double, values.ReadInt64Element(i));
            Assert.AreEqual(_buffers[i].@double.ToString(), values.ReadStringElement(i));
        }
    }

    [Test]
    public void ReadFloat64Element_Unchecked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Float64, sizeof(double));

        for (int i = 0; i < count; i++)
        {
            Assert.AreEqual(_buffers[i].@double, values.ReadFloat64ElementUnchecked(i));
        }
    }


    [Test]
    public void ReadInt64Element_Checked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Int64, sizeof(double));

        for (int i = 0; i < count; i++)
        {
            Assert.AreEqual(_buffers[i].@long, values.ReadInt64Element(i));
            Assert.AreEqual((double)_buffers[i].@long, values.ReadFloat64Element(i));
            Assert.AreEqual(_buffers[i].@long.ToString(), values.ReadStringElement(i));
        }
    }

    [Test]
    public void ReadInt64Element_Unchecked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Int64, sizeof(double));

        for (int i = 0; i < count; i++)
        {
            Assert.AreEqual(_buffers[i].@long, values.ReadInt64ElementUnchecked(i));
        }
    }



    [Test]
    public void ReadIntElement_CheckedVsUnchecked()
    {
        const int count = 2048;
        var values = FromBytes(_buffers, count, TypeTag.Int32, sizeof(int));

        float value = 0f;
        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            value = values.ReadIntElement(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, checked int32 element read: {_stopwatch.ElapsedTicks} ticks, last value {value}");

        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            value = values.ReadIntElementUnchecked(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, unchecked int32 element read: {_stopwatch.ElapsedTicks} ticks, last value {value}");
    }

    [Test]
    public void ReadMidiMessageElement_CheckedVsUnchecked()
    {
        const int count = 2048;
        var values = FromBytes(_midiSourceBytes, count, TypeTag.MIDI);
        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            _ = values.ReadMidiElement(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, checked MIDI element read: {_stopwatch.ElapsedTicks} ticks");

        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            _ = values.ReadMidiElementUnchecked(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, unchecked MIDI element read: {_stopwatch.ElapsedTicks} ticks");
    }

    [Test]
    public void ReadColor32MessageElement_CheckedVsUnchecked()
    {
        const int count = 2048;
        var values = FromBytes(_midiSourceBytes, count, TypeTag.Color32);

        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            _ = values.ReadColor32Element(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, checked Color32 element read: {_stopwatch.ElapsedTicks} ticks");

        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            _ = values.ReadColor32ElementUnchecked(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 4} elements, unchecked Color32 element read: {_stopwatch.ElapsedTicks} ticks");
    }

    [Test]
    public void ReadTimestampElement_CheckedVsUnchecked()
    {
        const int count = 2048;
        var values = FromBytes(_timeSourceBytes, count, TypeTag.TimeTag);

        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            _ = values.ReadTimestampElement(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 8} elements, checked NTP timestamp element read: {_stopwatch.ElapsedTicks} ticks");

        _stopwatch.Restart();
        for (int i = 0; i < count; i++)
        {
            _ = values.ReadTimestampElementUnchecked(i);
        }
        _stopwatch.Stop();

        Debug.WriteLine($"{count / 8} elements, unchecked NTP timestamp element read: {_stopwatch.ElapsedTicks} ticks");
    }
}
