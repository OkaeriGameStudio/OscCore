using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BuildSoft.OscCore.UnityObjects;
using MiniNtp;
using NUnit.Framework;

namespace BuildSoft.OscCore.Tests;

public class MessageReadPerformanceTests
{
    private const int Count = 4096;
    private static readonly Stopwatch _stopwatch = new();
    private int[] _intSourceData = new int[Count];
    private float[] _floatSourceData = new float[Count];
    private byte[] _bigEndianIntSourceBytes = new byte[Count * 4];
    private byte[] _bigEndianFloatSourceBytes = new byte[Count * 4];
    private byte[] _midiSourceBytes = null!;
    private byte[] _timeSourceBytes = null!;
    private List<GCHandle> _handles = new();

    [OneTimeSetUp]
    public void BeforeAll()
    {
        _handles.Clear();

        _handles.Add(GCHandle.Alloc(_bigEndianIntSourceBytes, GCHandleType.Pinned));
        _handles.Add(GCHandle.Alloc(_bigEndianFloatSourceBytes, GCHandleType.Pinned));

        _midiSourceBytes = TestUtil.RandomMidiBytes(Count * 4);
        _timeSourceBytes = TestUtil.RandomTimestampBytes(Count * 4);

        for (int i = 0; i < _intSourceData.Length; i++)
            _intSourceData[i] = TestUtil.SharedRandom.Next(-10000, 10000);

        for (int i = 0; i < _floatSourceData.Length; i++)
            _floatSourceData[i] = (float)TestUtil.SharedRandom.NextDouble() * 200f - 100f;
    }

    [SetUp]
    public void BeforeEach()
    {
        for (int i = 0; i < _intSourceData.Length; i++)
        {
            var lBytes = BitConverter.GetBytes(_intSourceData[i]);
            var bBytes = TestUtil.ReversedCopy(lBytes);

            var elementStart = i * 4;
            for (int j = 0; j < bBytes.Length; j++)
                _bigEndianIntSourceBytes[elementStart + j] = bBytes[j];
        }

        for (int i = 0; i < _floatSourceData.Length; i++)
        {
            var lBytes = BitConverter.GetBytes(_floatSourceData[i]);
            var bBytes = TestUtil.ReversedCopy(lBytes);

            var elementStart = i * 4;
            for (int j = 0; j < bBytes.Length; j++)
                _bigEndianFloatSourceBytes[elementStart + j] = bBytes[j];
        }
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

    [Test]
    public void ReadFloatElement_CheckedVsUnchecked()
    {
        const int count = 2048;
        var values = FromBytes(_bigEndianFloatSourceBytes, count, TypeTag.Float32);

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
    public void ReadIntElement_CheckedVsUnchecked()
    {
        const int count = 2048;
        var values = FromBytes(_bigEndianIntSourceBytes, count, TypeTag.Int32);

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
