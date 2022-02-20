using System.Runtime.CompilerServices;

namespace BuildSoft.OscCore;

/// <summary>
/// type tags from http://opensoundcontrol.org/spec-1_0 
/// </summary>
public enum TypeTag : byte
{
    False = 70,                     // F, non-standard
    Infinitum = 73,                 // I, non-standard
    Nil = 78,                       // N, non-standard
    AltTypeString = 84,             // S, non-standard
    True = 85,                      // T, non-standard
    ArrayStart = 91,                // [, non-standard
    ArrayEnd = 93,                  // ], non-standard
    Blob = 98,                      // b, STANDARD
    AsciiChar32 = 99,               // c, non-standard
    Float64 = 100,                  // d, non-standard
    Float32 = 102,                  // f, STANDARD
    Int64 = 104,                    // h, non-standard
    Int32 = 105,                    // i, STANDARD
    MIDI = 109,                     // m, non-standard
    Color32 = 114,                  // r, non-standard
    String = 115,                   // s, STANDARD
    TimeTag = 116                   // t, non-standard
}

public static class TypeTagMethods
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSupported(this TypeTag tag)
    {
        return tag switch
        {
            TypeTag.False or
            TypeTag.Infinitum or
            TypeTag.Nil or
            TypeTag.AltTypeString or
            TypeTag.True or
            TypeTag.Blob or
            TypeTag.AsciiChar32 or
            TypeTag.Float64 or
            TypeTag.Float32 or
            TypeTag.Int64 or
            TypeTag.Int32 or
            TypeTag.MIDI or
            TypeTag.Color32 or
            TypeTag.String or
            TypeTag.TimeTag or
            TypeTag.ArrayStart or
            TypeTag.ArrayEnd => true,
            _ => false,
        };
    }
}

