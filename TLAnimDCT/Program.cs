using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

using static TLAnimDCT.CModelDataEnum;

namespace TLAnimDCT;

internal class Program
{
    static void Main(string[] args)
    {
        byte[] filee = File.ReadAllBytes(@"MTN_CHR_ALV_000_BTL_000.TOANMB");
        EndianUtils.IsBigEndian = true;

        Vector4 outt = new Vector4();
        var curve = new CAnimationData.SCurve();
        curve.mCurveFormatFlag = 0x3309;
        CAnimationAlgorithmBase.CalcDCT(ref outt, ref curve, filee, 0x3644, 35, 0.0f, 66); 
    }
}

public class CAnimationData
{
    public class SCurve
    {
        public uint mCurveFormatFlag;
        public List<byte> data;
        public byte[] byteData;
        public bool bByteDataValid;
    }
}

public class CModelDataEnum
{
    public enum ECurveFormatFlag
    {
        CURVE_FORMAT_LINEAR = 1,
        CURVE_FORMAT_FLAT = 2,
        CURVE_FORMAT_CONSTANT = 3,
        CURVE_FORMAT_DCT4_SHORT = 8,
        CURVE_FORMAT_DCT4_LONG = 9,

        CURVE_FORMAT_LEAP_MASK = 0b1111, // 0x0F, 15

        CURVE_FORMAT_VALUE_F32 = 0,
        CURVE_FORMAT_VALUE_S16 = 16,
        CURVE_FORMAT_VALUE_MASK = 48,

        CURVE_FORMAT_KEY_NO = 0,
        CURVE_FORMAT_KEY_F32 = 256,
        CURVE_FORMAT_KEY_U16 = 512,
        CURVE_FORMAT_KEY_U8 = 768,
        CURVE_FORMAT_KEY_INDEX = 1024,
        CURVE_FORMAT_KEY_MASK = 3840,

        CURVE_FORMAT_DIM_FLAG = 61440,
        CURVE_FORMAT_DIM_SHIFT = 12
    }
}
