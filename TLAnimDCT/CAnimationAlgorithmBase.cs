using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using static TLAnimDCT.CModelDataEnum;

namespace TLAnimDCT;

public class CAnimationAlgorithmBase
{
    private const uint DCT_PART_SIZE = 33U;

    private static float[][] sDCTTable; // Initialized at CAnimationBase::InitializeStatic

    static CAnimationAlgorithmBase()
    {
        InitializeStatic();
    }

    static void InitializeStatic()
    {
        sDCTTable = new float[8][];
        for (int i = 0; i < 8; i++)
        {
            int size = 4 * i + 4;
            float scale = MathF.Sqrt(2.0f / (float)size);

            int index = 0;
            float[] dctRow = new float[size * size];
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                    dctRow[index++] = MathF.Cos((row + 0.5f) * ((float)Math.PI / (float)size) * (col + 0.5f)) * scale;
            }

            sDCTTable[i] = dctRow;
        }
    }

    public static void CalcLinear(ref Vector4 retValue, ref CAnimationData.SCurve iCurve, byte[] iValueData, uint iValueIndex, uint iKeyIndex, float iAlpha, uint iNumKey)
    {
        uint iDim = iCurve.mCurveFormatFlag >> (int)ECurveFormatFlag.CURVE_FORMAT_DIM_SHIFT;
        ECurveFormatFlag format = (ECurveFormatFlag)(iCurve.mCurveFormatFlag & (int)ECurveFormatFlag.CURVE_FORMAT_VALUE_MASK);

        Vector4 min_; Vector4 max_;
        if (format == ECurveFormatFlag.CURVE_FORMAT_VALUE_S16)
        {
            TL_BRVec4C.BRVec4LoadS16(out min_, iValueData, (iValueIndex + (sizeof(ushort) * (iDim * iKeyIndex))));
            TL_BRVec4C.BRVec4LoadS16(out max_, iValueData, (iValueIndex + (sizeof(ushort) * (iDim * (iKeyIndex + 1)))));
        }
        else
        {
            TL_BRVec4C.BRVec4LoadS16(out min_, iValueData, (iValueIndex + (sizeof(float) * (iDim * iKeyIndex))));
            TL_BRVec4C.BRVec4LoadS16(out max_, iValueData, (iValueIndex + (sizeof(float) * (iDim * (iKeyIndex + 1)))));
        }

        retValue = Vector4.Lerp(min_, max_, iAlpha);
    }

    public static void CalcFlat(ref Vector4 retValue, ref CAnimationData.SCurve iCurve, byte[] iValueData, uint iValueIndex, uint iKeyIndex, float iAlpha, uint iNumKey)
    {
        uint iDim = iCurve.mCurveFormatFlag >> (int)ECurveFormatFlag.CURVE_FORMAT_DIM_SHIFT;
        ECurveFormatFlag format = (ECurveFormatFlag)(iCurve.mCurveFormatFlag & (int)ECurveFormatFlag.CURVE_FORMAT_VALUE_MASK);

        if (format == ECurveFormatFlag.CURVE_FORMAT_VALUE_S16)
            TL_BRVec4C.BRVec4LoadS16(out retValue, iValueData, (iValueIndex + (sizeof(ushort) * (iDim * (iKeyIndex + 1)))));
        else
            TL_BRVec4C.BRVec4LoadS16(out retValue, iValueData, (iValueIndex + (sizeof(float) * (iDim * iKeyIndex))));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oBRValue">Return value.</param>
    /// <param name="iCurve">Input curve.</param>
    /// <param name="iValueData">Curve data, after keys if any.</param>
    /// <param name="iValueIndex">Offset.</param>
    /// <param name="iKeyIndex">Key index.</param>
    /// <param name="iAlpha">Interpolation factor, the value normally between 0.0 and 1.0</param>
    /// <param name="iNumKey">Num key in the curve?</param>
    public static void CalcDCT(ref Vector4 oBRValue, ref CAnimationData.SCurve iCurve, byte[] iValueData, uint iValueIndex, uint iKeyIndex, float iAlpha, uint iNumKey)
    {
        uint iDim = iCurve.mCurveFormatFlag >> (int)ECurveFormatFlag.CURVE_FORMAT_DIM_SHIFT;
        ECurveFormatFlag leap = (ECurveFormatFlag)(iCurve.mCurveFormatFlag & (int)ECurveFormatFlag.CURVE_FORMAT_LEAP_MASK);
        ECurveFormatFlag format = (ECurveFormatFlag)(iCurve.mCurveFormatFlag & (int)ECurveFormatFlag.CURVE_FORMAT_VALUE_MASK);

        short numPartFieldSize = 0;
        ushort numParts = 1;

        float iBRBaseAll = BitConverter.ToSingle(iValueData, 0);
        uint currentOffset = sizeof(float);

        if (leap == ECurveFormatFlag.CURVE_FORMAT_DCT4_LONG)
        {
            numPartFieldSize = sizeof(ushort);
            numParts = BitConverter.ToUInt16(iValueData, (int)currentOffset);
        }

        int offsetToParts = (int)currentOffset + numPartFieldSize;
        int offsetToVectorBaseTable = CeilingWith(numPartFieldSize - 2 + (2 * numParts), 0x04) + (int)currentOffset;
        int vectorBaseTableSize;
        if (format == ECurveFormatFlag.CURVE_FORMAT_VALUE_S16)
            vectorBaseTableSize = CeilingWith((int)iDim * sizeof(ushort) * (numParts + 1), 0x04);
        else
            vectorBaseTableSize = (int)iDim * sizeof(float) * (numParts + 1); // F32

        currentOffset = (uint)(offsetToVectorBaseTable + vectorBaseTableSize);

        uint partIndex = iKeyIndex / DCT_PART_SIZE;
        uint iIndex = iKeyIndex - DCT_PART_SIZE * ((iKeyIndex / DCT_PART_SIZE) & 0x1FFFFFFF);

        if (partIndex > 0)
        {
            ushort count = BitConverter.ToUInt16(iValueData, (int)(offsetToParts + (partIndex * sizeof(ushort))));
            currentOffset += count;
        }

        int iNumPartSample = (int)DCT_PART_SIZE;
        if (partIndex == numParts - 1)
            iNumPartSample = (int)(iNumKey - (int)DCT_PART_SIZE * partIndex - 1);

        Vector4 result;
        if (iValueData?.Length > 0)
        {
            if (iAlpha == 0.0f)
            {
                Vector4 vec = Vector4.Zero;
                ExtractDCT(ref vec, iBRBaseAll, iValueData, currentOffset, iIndex, iDim, (uint)iNumPartSample);
                result = vec;
            }
            else
            {
                Vector4 min = Vector4.Zero;
                Vector4 max = Vector4.Zero;
                ExtractDCT(ref min, iBRBaseAll, iValueData, currentOffset, iIndex, iDim, (uint)iNumPartSample);
                ExtractDCT(ref max, iBRBaseAll, iValueData, currentOffset, iIndex + 1, iDim, (uint)iNumPartSample);
                result = Vector4.Lerp(min, max, iAlpha);
            }
        }
        else
        {
            result = Vector4.Zero;
        }

        Vector4 min_; Vector4 max_;
        if (format == ECurveFormatFlag.CURVE_FORMAT_VALUE_S16)
        {
            TL_BRVec4C.BRVec4LoadS16(out min_, iValueData, (uint)(offsetToVectorBaseTable + (sizeof(ushort) * (iDim * partIndex))));
            TL_BRVec4C.BRVec4LoadS16(out max_, iValueData, (uint)(offsetToVectorBaseTable + (sizeof(ushort) * (iDim * (partIndex + 1)))));
        }
        else
        {
            TL_BRVec4C.BRVec4LoadS16(out min_, iValueData, (uint)(offsetToVectorBaseTable + (sizeof(float) * (iDim * partIndex))));
            TL_BRVec4C.BRVec4LoadS16(out max_, iValueData, (uint)(offsetToVectorBaseTable + (sizeof(float) * (iDim * (partIndex + 1)))));
        }

        float factor = (float)(iIndex + iAlpha) / (float)iNumPartSample;
        oBRValue = Vector4.Lerp(min_, max_, factor);
    }


    /// <summary>
    /// Performs vector decompression using precomputed DCT table
    /// </summary>
    /// <param name="oBRRet">Output vector</param>
    /// <param name="iBRBaseAll"></param>
    /// <param name="iPartData"></param>
    /// <param name="iPartIndex"></param>
    /// <param name="iIndex"></param>
    /// <param name="iDim">Number of dimensions/axis</param>
    /// <param name="iNumPartSample"></param>
    public static void ExtractDCT(ref Vector4 oBRRet, float iBRBaseAll, byte[] iPartData, uint iPartIndex, uint iIndex, uint iDim, uint iNumPartSample)
    {
        if (iIndex == 0 || iIndex == iNumPartSample)
        {
            oBRRet = Vector4.Zero;
            return;
        }

        for (int axis = 0; axis < iDim; axis++)
        {
            // SCompDCTHeader (2 + 2 + 1 + 1 + 1 + 1) - 8 bytes
            float mBase1 = (float)(BitConverter.ToUInt16(iPartData, (int)iPartIndex + 0) / (float)ushort.MaxValue) * iBRBaseAll;
            float mBase2 = (float)(BitConverter.ToUInt16(iPartData, (int)iPartIndex + 2) / (float)ushort.MaxValue) * iBRBaseAll;
            float mBase3 = mBase1 * mBase2 * (iPartData[iPartIndex + 4] / (float)byte.MaxValue); // 255.0

            uint mNum16_8 = iPartData[iPartIndex + 5];
            uint mNum4_0 = iPartData[iPartIndex + 6];
            uint mNumBase1_2 = iPartData[iPartIndex + 7];

            uint mNumS16Vectors = mNum16_8 >> 4;
            uint mNumS8Vectors = mNum16_8 & 0b1111;
            uint mNumS4Vectors = mNum4_0 >> 4;
            uint mNum0 = mNum4_0 & 0b1111;

            uint mNumBase1 = mNumBase1_2 >> 4;
            uint mNumBase2 = mNumBase1_2 & 0b1111;

            var baseList = new BaseList();
            baseList.Init(mNumBase1, mNumBase2, mBase1, mBase2, mBase3);

            int dctIndex = (int)(mNumS16Vectors + mNumS8Vectors + mNumS4Vectors + mNum0);
            var dctSubTbl = sDCTTable[dctIndex - 1];
            int dctSubIndex = (int)(4 * (iIndex - 1) * dctIndex);

            long currentOffset = iPartIndex + 8; // size of header
            float axisValue = 0.0f;

            for (int i = 0; i < mNumS16Vectors; i++)
            {
                // Load normalized shorts
                TL_BRVec4C.BRVec4LoadS16(out Vector4 oResult, iPartData, (uint)currentOffset);

                axisValue += ((oResult.X * dctSubTbl[dctSubIndex]) + (oResult.Y * dctSubTbl[dctSubIndex + 1]) + (oResult.Z * dctSubTbl[dctSubIndex + 2]) + (oResult.W * dctSubTbl[dctSubIndex + 3])) * baseList.GetBase();

                baseList.Next();
                currentOffset += 4 * sizeof(ushort); // 8 bytes
                dctSubIndex += 4;
            }

            for (int i = 0; i < mNumS8Vectors; i++)
            {
                // Load bytes
                float x = ((sbyte)iPartData[currentOffset + 0]) / (float)sbyte.MaxValue; // 127 - signed cast is important
                float y = ((sbyte)iPartData[currentOffset + 1]) / (float)sbyte.MaxValue;
                float z = ((sbyte)iPartData[currentOffset + 2]) / (float)sbyte.MaxValue;
                float w = ((sbyte)iPartData[currentOffset + 3]) / (float)sbyte.MaxValue;

                axisValue += ((x * dctSubTbl[dctSubIndex]) + (y * dctSubTbl[dctSubIndex + 1]) + (z * dctSubTbl[dctSubIndex + 2]) + (w * dctSubTbl[dctSubIndex + 3])) * baseList.GetBase();

                baseList.Next();
                currentOffset += 4;
                dctSubIndex += 4;
            }

            for (int i = 0; i < mNumS4Vectors / 2; i++)
            {
                // Load 2 vec4 vectors from 2 nibble vectors
                TL_BRSpecialC.BRVec4x2LoadS4x2(out Vector4 oResultHi, out Vector4 oResultLo, iPartData, (int)currentOffset);

                // Part 1
                axisValue += ((oResultHi.X * dctSubTbl[dctSubIndex]) + (oResultHi.Y * dctSubTbl[dctSubIndex + 1]) + (oResultHi.Z * dctSubTbl[dctSubIndex + 2]) + (oResultHi.W * dctSubTbl[dctSubIndex + 3])) * baseList.GetBase();
                baseList.Next();
                dctSubIndex += 4;

                // Part 2
                axisValue += ((oResultLo.X * dctSubTbl[dctSubIndex]) + (oResultLo.Y * dctSubTbl[dctSubIndex + 1]) + (oResultLo.Z * dctSubTbl[dctSubIndex + 2]) + (oResultLo.W * dctSubTbl[dctSubIndex + 3])) * baseList.GetBase();
                baseList.Next();
                dctSubIndex += 4;

                currentOffset += 4;
            }

            // Set axis value to our decompressed vector
            // i.e axis = 0, oBRRet.X = axisValue
            oBRRet[axis] = axisValue;
        }
    }

    // CriWave.CriMana.Detail.RendererResource.CeilingWith
    static int CeilingWith(int x, int ceilingValue)
    {
        return -ceilingValue & (x + ceilingValue - 1);
    }

    public struct BaseList
    {
        public void Init(uint iNumBase1, uint iNumBase2, float iBase1, float iBase2, float iBase3)
        {
            mIndex = 0;
            mNumBase1 = iNumBase1;
            mNumBase2 = iNumBase2;
            mBase1 = iBase1;
            mBase2 = iBase2;
            mBase3 = iBase3;
        }

        public float GetBase()
        {
            if (mIndex < mNumBase1)
                return mBase1;
            if (mIndex < mNumBase2)
                return mBase2;

            return mBase3;
        }

        public void Next()
        {
            mIndex++;
        }

        public uint mIndex;
        public uint mNumBase1;
        public uint mNumBase2;
        public float mBase1;
        public float mBase2;
        public float mBase3;
    }
}
