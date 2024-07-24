using D3DLab.ECS;
using D3DLab.ECS.Ext;
using D3DLab.FileFormats.GeoFormats;
using D3DLab.FileFormats.GeoFormats.STL;

using g3;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    public static class G3Readers {
        class MeshData : IFileGeometry3D {
            public MeshData(string name,
                ReadOnlyCollection<Vector3> positions, ReadOnlyCollection<Vector3> normals,
                ReadOnlyCollection<int> indices, ReadOnlyCollection<Vector2> textureCoors,
                ReadOnlyCollection<Vector4> colors) {
                Name = name;
                Positions = positions;
                Normals = normals;
                Indices = indices;
                TextureCoors = textureCoors;
                Colors = colors;
                Topology = GeometryPrimitiveTopologies.TriangleList;
            }

            public string Name { get; }
            public ReadOnlyCollection<Vector3> Positions { get; }
            public ReadOnlyCollection<Vector3> Normals { get; }
            public ReadOnlyCollection<int> Indices { get; }
            public ReadOnlyCollection<Vector2> TextureCoors { get; }
            public ReadOnlyCollection<Vector4> Colors { get; }
            public GeometryPrimitiveTopologies Topology { get; }

            public IFileGeometry3D ApplyMatrix(ref Matrix4x4 matrix) {
                throw new NotImplementedException();
            }
        }

        public static ASCIIBinarySTLGeometry ReadStl(FileInfo file) {
            //var builder = new SimpleMeshBuilder();
            //var stl = new STLReader();
            IOReadResult res;
            var reader = new ASCIIBinarySTLParser();
            using (var fs = file.OpenRead()) {
                reader.Read(fs);
                //using (var br = new BinaryReader(fs)) {
                //    res = stl.Read(br, new ReadOptions { }, builder);
                //}
            }

            return reader.Result;

            //    if (res.code == IOCode.Ok && builder.Meshes.Any()) {
            //        var mesh = builder.Meshes.Single();
            //        return new[] {
            //            new MeshData("STL",
            //            mesh.GetVertexArrayFloat().ToVector3List().AsReadOnly(),
            //            mesh.HasVertexNormals ?
            //                mesh.GetVertexNormalArray().ToVector3List().AsReadOnly()
            //                : new ReadOnlyCollection<Vector3>(new List<Vector3>()),
            //            mesh.GetTriangleArray().AsReadOnly(),
            //            mesh.HasVertexUVs ?
            //                mesh.GetVertexUVArray().ToVector2List().AsReadOnly()
            //                : new ReadOnlyCollection<Vector2>(new List<Vector2>()),
            //            mesh.HasVertexColors ?
            //                mesh.GetVertexColorArray().ToVector3List().AsReadOnly()
            //                :  new ReadOnlyCollection<Vector3>(new List<Vector3>())
            //            )
            //        };
            //    }
            //    throw new Exception("Can't read STL.");
        }


    }

    public static class G3Writers {
        public static void WriteObj(FileInfo file, IEnumerable<IFileGeometry3D> geometries) {
            var meshes = new List<WriteMesh>();
            var mesh = new SimpleMesh();

            var map = new Dictionary<Vector3, int>();
            var indeces = new List<int>();
            foreach (var geo in geometries) {
                //var pcount = geo.Positions.Count * 3;
                //var pp = new double[pcount];
                //var pindex = 0;
                //for(var index =0; index < geo.Positions.Count; index++) {
                //    var v = geo.Positions[index];
                //    pp[pindex++] = v.X;
                //    pp[pindex++] = v.Y;
                //    pp[pindex++] = v.Z;
                //}
                //mesh.Initialize(new VectorArray3d(pp), new VectorArray3i(geo.Indices.ToArray()));
                for (var index = 0; index < geo.Positions.Count; index++) {
                    var v = geo.Positions[index];
                    if (!map.ContainsKey(v)) {
                        map.Add(v, map.Count);
                    }
                }
                for (var index = 0; index < geo.Indices.Count; index++) {
                    var i = geo.Indices[index];
                    var v = geo.Positions[i];
                    indeces.Add(map[v]);
                }
            }

            var points = map.Keys.ToArray();
            var pcount = points.Length * 3;
            var pp = new double[pcount];
            var pindex = 0;
            for (var index = 0; index < points.Length; index++) {
                var v = points[index];
                pp[pindex++] = v.X;
                pp[pindex++] = v.Y;
                pp[pindex++] = v.Z;
            }
            mesh.Initialize(new VectorArray3d(pp), new VectorArray3i(indeces.ToArray()));

            meshes.Add(new WriteMesh(mesh, $"d3dlab export"));

            StandardMeshWriter.WriteFile(file.FullName, meshes, WriteOptions.Defaults);

            //var obj = new OBJWriter();
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //using (var stream = File.Open(file.FullName, FileMode.Create)) {
            //    using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
            //        obj.Write(writer, meshes, WriteOptions.Defaults);
            //    }
            //}
        }

        public static void WriteObj(FileInfo file, IGeometryData geo) {
            var meshes = new List<WriteMesh>();
            var mesh = new SimpleMesh();

            var map = new Dictionary<Vector3, int>();
            var indeces = new List<int>();

            for (var index = 0; index < geo.Positions.Length; index++) {
                var v = geo.Positions[index];
                if (!map.ContainsKey(v)) {
                    map.Add(v, map.Count);
                }
            }
            for (var index = 0; index < geo.Indices.Length; index++) {
                var i = geo.Indices[index];
                var v = geo.Positions[i];
                indeces.Add(map[v]);
            }


            var points = map.Keys.ToArray();
            var pcount = points.Length * 3;
            var pp = new double[pcount];
            var pindex = 0;
            for (var index = 0; index < points.Length; index++) {
                var v = points[index];
                pp[pindex++] = v.X;
                pp[pindex++] = v.Y;
                pp[pindex++] = v.Z;
            }
            mesh.Initialize(new VectorArray3d(pp), new VectorArray3i(indeces.ToArray()));

            meshes.Add(new WriteMesh(mesh, $"d3dlab export"));

            StandardMeshWriter.WriteFile(file.FullName, meshes, WriteOptions.Defaults);
        }

    }
}
