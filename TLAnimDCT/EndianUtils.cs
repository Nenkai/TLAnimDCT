using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TLAnimDCT;

public class EndianUtils
{
    public static bool IsBigEndian { get; set; }

    public unsafe static T ReadNumber<T>(byte[] bytes, int offset) where T : unmanaged
    {
        int valueSize = sizeof(T);
        Span<byte> span = bytes.AsSpan(offset, valueSize);
        if (bytes.Length < sizeof(T))
            throw new ArgumentException($"Not enough data to read {typeof(T).Name}");

        if (IsBigEndian)
        {
            Span<byte> copy = stackalloc byte[sizeof(T)];
            span.CopyTo(copy);
            copy.Reverse();
            return MemoryMarshal.Read<T>(copy);
        }

        return MemoryMarshal.Read<T>(span);
    }
}
