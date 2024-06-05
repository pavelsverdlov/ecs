using D3DLab.ECS;
using D3DLab.ECS.Ext;
using D3DLab.FileFormats.GeoFormats;
using D3DLab.Toolkit.Math3D;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    internal static class EnumerableEx {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }
        public static IEnumerable<T> ToEnumerable<T>(this T source) { yield return source; }
    }
    internal static class ArrayEx {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] source) => Array.AsReadOnly(source);

        
    }

    public static class SystemWindowsMediaEx {
        public static Vector4 ToVector4(this System.Windows.Media.Color color) {
            color.Clamp();
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
        public static System.Windows.Media.Color ToColor(this string color) {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
        }
        public static System.Windows.Media.Color ToColor(this Vector4 color) {
            return System.Windows.Media.Color.FromArgb(
                (byte)(color.W * 255), (byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255));
        }
        public static string ToHexString(this System.Windows.Media.Color color) {
            return new System.Windows.Media.ColorConverter().ConvertToString(color);
        }
        //public static string ToHexString(this System.Windows.Media.Brush color) {
        //    return new System.Windows.Media.ColorConverter().ConvertToString(color);
        //}

        //
    }

    public static class FileFormatEx {
        public static ImmutableGeometryData ToGeometryData(this IFileGeometry3D fgeo) {// 
            return new ImmutableGeometryData(fgeo.Positions, fgeo.Normals, fgeo.Indices, 
                fgeo.TextureCoors, fgeo.Colors.ToList());
        }
    }

    public static class ExportEx {
        public static void ToObjFile(this IGeometryData geo, string path) {
            G3Writers.WriteObj(new System.IO.FileInfo(path), geo );
        }
    }


    public static class Vector3Ex {
        public static Vector4 ApplyAlpha(this Vector4 color, float alpha) {
            return new Vector4(color.X, color.Y, color.Z, alpha);
        }
    }
}
