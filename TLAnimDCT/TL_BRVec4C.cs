using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TLAnimDCT;

public class TL_BRVec4C
{
    /// <summary>
    /// Load normalized shorts from array into vector4
    /// </summary>
    /// <param name="oResult"></param>
    /// <param name="iSource"></param>
    /// <param name="index"></param>
    public static void BRVec4LoadS16(out Vector4 oResult, byte[] iSource, uint index = 0U)
    {
        // (short*)&iSource[index]
        Span<short> ushorts = MemoryMarshal.Cast<byte, short>(iSource.AsSpan()[(int)index..]);

        oResult.X = BitConverter.ToInt16(iSource, ushorts[0]) / (float)short.MaxValue; // 32767
        oResult.Y = BitConverter.ToInt16(iSource, ushorts[1]) / (float)short.MaxValue;
        oResult.Z = BitConverter.ToInt16(iSource, ushorts[2]) / (float)short.MaxValue;
        oResult.W = BitConverter.ToInt16(iSource, ushorts[3]) / (float)short.MaxValue;
    }

    /// <summary>
    /// Load float array into vector4
    /// </summary>
    /// <param name="oResult"></param>
    /// <param name="iSource"></param>
    /// <param name="index"></param>
    public static void BRVec4Load(out Vector4 oResult, byte[] iSource, uint index = 0U)
    {
        // (float*)&iSource[index]
        Span<float> floats = MemoryMarshal.Cast<byte, float>(iSource.AsSpan()[(int)index..]);

        oResult.X = floats[0];
        oResult.Y = floats[1];
        oResult.Z = floats[2];
        oResult.W = floats[3];
    }
}
