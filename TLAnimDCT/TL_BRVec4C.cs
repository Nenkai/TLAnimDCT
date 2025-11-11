using System;
using System.Buffers.Binary;
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
        oResult.X = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 0)) / (float)short.MaxValue; // 32767
        oResult.Y = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 2)) / (float)short.MaxValue;
        oResult.Z = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 4)) / (float)short.MaxValue;
        oResult.W = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 6)) / (float)short.MaxValue;
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
        //Span<float> floats = MemoryMarshal.Cast<byte, float>(iSource.AsSpan()[(int)index..]);

        oResult.X = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 0x00));
        oResult.Y = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 0x04));
        oResult.Z = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 0x08));
        oResult.W = EndianUtils.ReadNumber<ushort>(iSource, (int)(index + 0x0C));
    }
}
