using System;
using System.Linq;
using System.Numerics;

using D3DLab.FileFormats.GeoFormats._OBJ;
using D3DLab.Toolkit.Math3D;

using Xunit;

namespace D3DLab.Toolkit.UnitTests {
    public class GeometryBuilderTests {
        [Fact]
        public void BuildSphere_Test() {
            var center = Vector3.Zero;
            float radius = 2;
            var geo = GeometryBuilder.BuildSphere(center, radius);

            ObjExports.ExportMesh(geo.Positions.ToArray(), geo.Indices.ToArray(),
                @"D:\debug\sphere.obj");
        }
    }
}
