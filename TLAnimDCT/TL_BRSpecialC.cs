using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TLAnimDCT;

public class TL_BRSpecialC
{
    /// <summary>
    /// Load nibble arrays into distict low and high vector4s
    /// </summary>
    /// <param name="oResultHi"></param>
    /// <param name="oResultLow"></param>
    /// <param name="iSource"></param>
    /// <param name="index"></param>
    public static void BRVec4x2LoadS4x2(out Vector4 oResultHi, out Vector4 oResultLow, byte[] iSource, int index)
    {
        oResultHi = default(Vector4);
        oResultLow = default(Vector4);

        oResultHi.X = ((iSource[index + 0] >> 4) - 8) / 7.0f;
        oResultHi.Y = ((iSource[index + 1] >> 4) - 8) / 7.0f;
        oResultHi.Z = ((iSource[index + 2] >> 4) - 8) / 7.0f;
        oResultHi.W = ((iSource[index + 3] >> 4) - 8) / 7.0f;
        oResultLow.X = ((iSource[index + 0] & 0b1111) - 8) / 7.0f;
        oResultLow.Y = ((iSource[index + 1] & 0b1111) - 8) / 7.0f;
        oResultLow.Z = ((iSource[index + 2] & 0b1111) - 8) / 7.0f;
        oResultLow.W = ((iSource[index + 3] & 0b1111) - 8) / 7.0f;
    }
}
