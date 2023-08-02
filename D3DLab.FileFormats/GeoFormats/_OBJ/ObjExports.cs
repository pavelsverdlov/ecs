using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeoFormats._OBJ {
    public static class ObjExports {
        public static void ExportCurve(Vector3[] curve, string path) {
            using (var file = new FileStream(path, FileMode.Create)) {
                using (var writer = new StreamWriter(file, Encoding.UTF8)) {
                    for (var i = 0; i < curve.Length; i++) {
                        var point = curve[i];
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2}", point.X, point.Y, point.Z));
                    }
                    for (var i = 0; i < curve.Length - 2; i++) {
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "l {0} {1}", i + 1, i + 2));
                    }
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "l {0} {1}", curve.Length - 1, 1));
                }
            }
        }

        public static void ExportMesh(Vector3[] curve, int[] indx, string path) {
            using (var file = new FileStream(path, FileMode.Create)) {
                using (var writer = new StreamWriter(file, Encoding.UTF8)) {
                    for (var i = 0; i < curve.Length; i++) {
                        var point = curve[i];
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2}", point.X, point.Y, point.Z));
                    }
                    for (var i = 0; i < indx.Length; i+=3) {
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "f {0} {1} {2}",
                            indx[i] + 1, indx[i+1] + 1, indx[i + 2] + 1));
                    }
                }
            }
        }
    }
}
