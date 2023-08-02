using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.FileFormats {
    static class Utf8ReadOnlySpanHelper {
        static readonly Encoding utf8 = Encoding.UTF8;

        public static unsafe string GetString(in ReadOnlySpan<byte> span) {
            fixed (byte* buffer = &MemoryMarshal.GetReference(span)) {
                var charCount = utf8.GetCharCount(buffer, span.Length);
                fixed (char* chars = stackalloc char[charCount]) {
                    var count = utf8.GetChars(buffer, span.Length, chars, charCount);
                    return new string(chars, 0, count);
                }
            }
        }
    }
}
