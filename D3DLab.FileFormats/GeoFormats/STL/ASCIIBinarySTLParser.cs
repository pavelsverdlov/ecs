using D3DLab.FileFormats.GeoFormats._OBJ;

using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeoFormats.STL {
    public sealed class ASCIIBinarySTLParser : IUtf8SpanReader {
        class Geo {
            private readonly Vector3 color;
            public List<Vector3> positions;
            public List<Vector3> normals;
            public List<Vector3> colors;
            public List<Vector2> tex;
            public List<int> indices;
            public Dictionary<Vector3, int> map = new Dictionary<Vector3, int>();

            public Geo(Vector3 color) {
                positions = new List<Vector3>();
                normals = new List<Vector3>();
                colors = new List<Vector3>();
                tex = new List<Vector2>();
                indices = new List<int>();
                this.color = color;
            }

            internal IFileGeometry3D GetMesh() {
                return new GeometryData(color.ToString(), positions, normals, colors, indices, tex, ECS.GeometryPrimitiveTopologies.TriangleList);
            }
        }

        public List<IFileGeometry3D> Geometry => map.Values.Select(x => x.GetMesh()).ToList();

        Dictionary<Vector3, Geo> map = new Dictionary<Vector3, Geo>();

        public void Read(Stream stream) {
            var asc = Encoding.ASCII;
            using (var binary = new BinaryReader(stream, asc)) {
                var head = new byte[80];
                for (var i = 0; i < head.Length; i++) {
                    head[i] = (byte)binary.ReadSByte();
                }

                var header = asc.GetString(head);

                var triangles = binary.ReadInt32();

                var lastColor = Vector3.Zero;

                for (var i = 0; i < triangles; i++) {
                    var normal = ReadV3(binary);
                    var v0 = ReadV3(binary);
                    var v1 = ReadV3(binary);
                    var v2 = ReadV3(binary);

                    //var bits = new BitArray(new[] {
                    //    binary.ReadByte(), binary.ReadByte()
                    //});

                    var color = ReadColor(binary);
                    if (!map.TryGetValue(color, out var geo)) {
                        geo = new Geo(color);
                        map.Add(color, geo);
                    }

                    if (!geo.map.ContainsKey(v0)) {
                        geo.map.Add(v0, geo.positions.Count);
                        geo.positions.Add(v0);
                        geo.normals.Add(normal);
                        geo.colors.Add(color);
                    }
                    if (!geo.map.ContainsKey(v1)) {
                        geo.map.Add(v1, geo.positions.Count);
                        geo.positions.Add(v1);
                        geo.colors.Add(color);
                        geo.normals.Add(normal);
                    }
                    if (!geo.map.ContainsKey(v2)) {
                        geo.map.Add(v2, geo.positions.Count);
                        geo.positions.Add(v2);
                        geo.colors.Add(color);
                        geo.normals.Add(normal);
                    }

                    geo.indices.Add(geo.map[v0]);
                    geo.indices.Add(geo.map[v1]);
                    geo.indices.Add(geo.map[v2]);

                    lastColor = color;

                    //var blue = binary.ReadByte();
                    //var green = binary.ReadByte();
                    //var red = binary.ReadByte();
                    //var isValid = binary.ReadByte();
                    //var attr = binary.ReadUInt16();

                }
            }
        }
        static Vector3 ReadColor(BinaryReader binary) {//BitArray bits
            //var blue = Convert4BitsToInt(bits, 0);
            //var green = Convert4BitsToInt(bits, 5);
            //var red = Convert4BitsToInt(bits, 10);

            //return new Vector3(red, green, blue);//, 1) ;

            var attrib = Convert.ToString(binary.ReadUInt16(), 2).PadLeft(16, '0').ToCharArray();
            var hasColor = attrib[0].Equals('1');

            int blue = attrib[15].Equals('1') ? 1 : 0;
            blue = attrib[14].Equals('1') ? blue + 2 : blue;
            blue = attrib[13].Equals('1') ? blue + 4 : blue;
            blue = attrib[12].Equals('1') ? blue + 8 : blue;
            blue = attrib[11].Equals('1') ? blue + 16 : blue;
            int b = blue * 8;

            int green = attrib[10].Equals('1') ? 1 : 0;
            green = attrib[9].Equals('1') ? green + 2 : green;
            green = attrib[8].Equals('1') ? green + 4 : green;
            green = attrib[7].Equals('1') ? green + 8 : green;
            green = attrib[6].Equals('1') ? green + 16 : green;
            int g = green * 8;

            int red = attrib[5].Equals('1') ? 1 : 0;
            red = attrib[4].Equals('1') ? red + 2 : red;
            red = attrib[3].Equals('1') ? red + 4 : red;
            red = attrib[2].Equals('1') ? red + 8 : red;
            red = attrib[1].Equals('1') ? red + 16 : red;
            int r = red * 8;

            return new Vector3(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }

        static float Convert4BitsToInt(BitArray bits, int start) {
            return Convert.ToInt32($"{Convert.ToByte(bits[start])}{Convert.ToByte(bits[start + 1])}{Convert.ToByte(bits[start + 2])}{Convert.ToByte(bits[start + 3])}", 2)
                / 31f;
        }



        static Vector3 ReadV3(BinaryReader binary) {
            return new Vector3(binary.ReadSingle(), binary.ReadSingle(), binary.ReadSingle());
        }
        unsafe void UnsafeRead(ReadOnlySpan<byte> all) {
            var _uint32 = sizeof(UInt32);

            var header = all.Slice(0, sizeof(sbyte) * 80);

            var triangles = all.Slice(sizeof(sbyte) * 80, sizeof(uint));

            var text = Utf8ReadOnlySpanHelper.GetString(header);
            if (!Utf8Parser.TryParse(triangles, out UInt32 value, out var _)) {
                throw new Exception("Can't read float");
            }

        }

        public void Read(ReadOnlySpan<byte> bytes) {
            UnsafeRead(bytes);
        }
    }
}
