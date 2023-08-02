using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.FileFormats.GeoFormats {
    public interface IUtf8SpanReader {
        void Read(ReadOnlySpan<byte> bytes);
    }
}
