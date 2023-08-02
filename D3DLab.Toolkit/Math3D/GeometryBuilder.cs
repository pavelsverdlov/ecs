using D3DLab.ECS;
using D3DLab.ECS.Ext;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    public struct ArrowData {
        public Vector3 axis;
        public Vector3 orthogonal;
        public Vector3 center;
        public Vector4 color;
        public float lenght;
        public float radius;
    }

    public struct SkyPlaneData {
        public static SkyPlaneData Default {
            get {
                return new SkyPlaneData {
                    PlaneResolution = 10,
                    PlaneWidth = 10.0f,
                    PlaneTop = 0.5f,
                    PlaneBottom = 0.0f,
                    TextureRepeat = 4
                };
            }
        }

        public int PlaneResolution;
        public float PlaneWidth;
        public float PlaneTop;
        public float PlaneBottom;
        public int TextureRepeat;
    }

    public class GeometryBuilder {
        public static ImmutableGeometryData BuildRotationOrbits(float radius, Vector3 center) {
            var step = 10f;

            var axises = new[] {
                new { asix = Vector3.UnitX, color =V4Colors.Green},
                new { asix = Vector3.UnitY, color = V4Colors.Red },
                new { asix = Vector3.UnitZ, color = V4Colors.Blue }
            };
            var lines = new List<Vector3>();
            var color = new List<Vector4>();
            foreach (var a in axises) {
                var axis = a.asix;
                var start = center + axis.FindAnyPerpendicular() * radius;
                var rotate = Matrix4x4.CreateFromAxisAngle(axis, step.ToRad());
                lines.Add(start);
                color.Add(a.color);
                for (var angle = step; angle < 360; angle += step) {
                    start = Vector3.Transform(start, rotate);
                    lines.Add(start);
                    color.Add(a.color);
                }
            }
            return new ImmutableGeometryData(lines.AsReadOnly(),
                Array.Empty<int>().AsReadOnly(), color.AsReadOnly());
        }

        public static ImmutableGeometryData BuildArrow(ArrowData data) {
            var axis = data.axis;
            var zero = Vector3.Zero;
            var lenght = data.lenght;
            var radius = data.radius;

            var rotate = Matrix4x4.CreateFromAxisAngle(axis, 10f.ToRad()); // Matrix4x4.CreateFromQuaternion(new Quaternion(axis,10));
            var move = Matrix4x4.CreateTranslation(data.center - zero);

            var points = new List<Vector3>();
            var normals = new List<Vector3>();
            var index = new List<int>();

            points.Add(zero + axis * lenght);
            normals.Add(axis);

            var orto = data.orthogonal;
            var corner = zero + orto * radius;
            normals.Add((corner - zero).Normalized());

            points.Add(corner);
            for (var i = 10; i < 360; i += 10) {
                corner = Vector3.Transform(corner, rotate);
                points.Add(corner);
                normals.Add((corner - zero).Normalized());
            }
            points.Add(zero);
            normals.Add(-axis);

            for (var i = 0; i < points.Count - 2; ++i) {
                index.AddRange(new[] { 0, i, i + 1 });
                index.AddRange(new[] { points.Count - 1, i, i + 1 });
            }
            index.AddRange(new[] { 0, points.Count - 2, 1 });
            index.AddRange(new[] { points.Count - 1, points.Count - 2, 1 });

            var pp = points.ToArray();
            points.Clear();
            pp.ForEach(x => points.Add(Vector3.Transform(x, move)));

            return new ImmutableGeometryData(
                points.AsReadOnly(),
                normals.AsReadOnly(),
                index.AsReadOnly()
            );
        }

        public static ImmutableGeometryData BuildSkyPlane(SkyPlaneData data) {
            var count = (data.PlaneResolution + 1) * (data.PlaneResolution + 1);
            var points = new Vector3[count];
            var tex = new Vector2[count];

            // Determine the size of each quad on the sky plane.
            float quadSize = data.PlaneWidth / (float)data.PlaneResolution;
            // Calculate the radius of the sky plane based on the width.
            float radius = data.PlaneWidth / 2.0f;
            // Calculate the height constant to increment by.
            float constant = (data.PlaneTop - data.PlaneBottom) / (radius * radius);
            // Calculate the texture coordinate increment value.
            float textureDelta = (float)data.TextureRepeat / (float)data.PlaneResolution;

            // Loop through the sky plane and build the coordinates based on the increment values given.
            for (int j = 0; j <= data.PlaneResolution; j++) {
                for (int i = 0; i <= data.PlaneResolution; i++) {
                    // Calculate the vertex coordinates.
                    float positionX = (-0.5f * data.PlaneWidth) + ((float)i * quadSize);
                    float positionZ = (-0.5f * data.PlaneWidth) + ((float)j * quadSize);
                    float positionY = data.PlaneTop - (constant * ((positionX * positionX) + (positionZ * positionZ)));

                    // Calculate the texture coordinates.
                    float tu = (float)i * textureDelta;
                    float tv = (float)j * textureDelta;

                    // Calculate the index into the sky plane array to add this coordinate.
                    int index = j * (data.PlaneResolution + 1) + i;

                    // Add the coordinates to the sky plane array.
                    points[index] = new Vector3(positionX, positionY, positionZ);
                    tex[index] = new Vector2(tu, tv);
                }
            }
            var vertexCount = (data.PlaneResolution + 1) * (data.PlaneResolution + 1) * 6;

            var indices = new int[vertexCount];
            var positions = new Vector3[vertexCount];
            var texture = new Vector2[vertexCount];

            // Initialize the index into the vertex array.
            int indx = 0;
            // Load the vertex and index array with the sky plane array data.
            for (int j = 0; j < data.PlaneResolution; j++) {
                for (int i = 0; i < data.PlaneResolution; i++) {
                    int index1 = j * (data.PlaneResolution + 1) + i;
                    int index2 = j * (data.PlaneResolution + 1) + (i + 1);
                    int index3 = (j + 1) * (data.PlaneResolution + 1) + i;
                    int index4 = (j + 1) * (data.PlaneResolution + 1) + (i + 1);

                    // Triangle 1 - Upper Left
                    positions[indx] = points[index1];
                    texture[indx] = tex[index1];
                    indices[indx] = indx;
                    indx++;

                    // Triangle 1 - Upper Right
                    positions[indx] = points[index2];
                    texture[indx] = tex[index2];
                    indices[indx] = indx;
                    indx++;

                    // Triangle 1 - Bottom Left
                    positions[indx] = points[index3];
                    texture[indx] = tex[index3];
                    indices[indx] = indx;
                    indx++;

                    // Triangle 2 - Bottom Left
                    positions[indx] = points[index3];
                    texture[indx] = tex[index3];
                    indices[indx] = indx;
                    indx++;

                    // Triangle 2 - Upper Right
                    positions[indx] = points[index2];
                    texture[indx] = tex[index2];
                    indices[indx] = indx;
                    indx++;

                    // Triangle 2 - Bottom Right
                    positions[indx] = points[index4];
                    texture[indx] = tex[index4];
                    indices[indx] = indx;
                    indx++;
                }
            }

            return new ImmutableGeometryData(
                positions.AsReadOnly(),
                positions.CalculateNormals(indices).AsReadOnly(),
                indices.AsReadOnly(),
                texture.AsReadOnly()
            );
        }

        public static ImmutableGeometryData BuildGeoBox(AxisAlignedBox box) {
            var indx = new List<int>();
            var dic = new Dictionary<Vector3, int>();

            var corners = box.GetCornersBox();
            var index = 0;

            dic.Add(corners.FarBottomLeft, index);
            dic.Add(corners.FarBottomRight, ++index);
            dic.Add(corners.FarTopRight, ++index);
            dic.Add(corners.FarTopLeft, ++index);

            dic.Add(corners.NearBottomLeft, ++index);
            dic.Add(corners.NearBottomRight, ++index);
            dic.Add(corners.NearTopRight, ++index);
            dic.Add(corners.NearTopLeft, ++index);

            indx.AddRange(new[] {
                //top
                dic[corners.FarTopRight], dic[corners.FarTopLeft], dic[corners.NearTopRight],
                dic[corners.NearTopLeft],dic[corners.NearTopRight],  dic[corners.FarTopLeft],

                //Bottom
                dic[corners.FarBottomRight],  dic[corners.NearBottomRight], dic[corners.FarBottomLeft],
                dic[corners.NearBottomLeft], dic[corners.FarBottomLeft],dic[corners.NearBottomRight], 

                //left
                dic[corners.NearTopLeft], dic[corners.FarTopLeft], dic[corners.FarBottomLeft],
                dic[corners.NearBottomLeft],  dic[corners.NearTopLeft], dic[corners.FarBottomLeft],

                //right
                dic[corners.NearTopRight],  dic[corners.NearBottomRight],dic[corners.FarTopRight],
                dic[corners.NearBottomRight], dic[corners.FarBottomRight], dic[corners.FarTopRight],

                //near
                dic[corners.NearBottomLeft],  dic[corners.NearBottomRight], dic[corners.NearTopRight],
                dic[corners.NearBottomLeft],  dic[corners.NearTopRight], dic[corners.NearTopLeft],

                //far
                dic[corners.FarBottomLeft], dic[corners.FarTopRight], dic[corners.FarBottomRight],
                dic[corners.FarBottomLeft], dic[corners.FarTopLeft], dic[corners.FarTopRight],
            });


            return new ImmutableGeometryData(dic.Keys.ToList().AsReadOnly(), indx.AsReadOnly());
        }

        public static Vector3[] BuildBox(AxisAlignedBox box) {
            var pos = new List<Vector3>();
            var corners = box.GetCornersBox();

            pos.Add(corners.FarBottomLeft);
            pos.Add(corners.FarBottomRight);

            pos.Add(corners.FarBottomRight);
            pos.Add(corners.FarTopRight);

            pos.Add(corners.FarTopRight);
            pos.Add(corners.FarTopLeft);

            pos.Add(corners.FarTopLeft);
            pos.Add(corners.FarBottomLeft);


            pos.Add(corners.NearBottomLeft);
            pos.Add(corners.NearBottomRight);

            pos.Add(corners.NearBottomRight);
            pos.Add(corners.NearTopRight);

            pos.Add(corners.NearTopRight);
            pos.Add(corners.NearTopLeft);

            pos.Add(corners.NearTopLeft);
            pos.Add(corners.NearBottomLeft);


            pos.Add(corners.NearBottomLeft);
            pos.Add(corners.FarBottomLeft);

            pos.Add(corners.NearBottomRight);
            pos.Add(corners.FarBottomRight);

            pos.Add(corners.NearTopRight);
            pos.Add(corners.FarTopRight);

            pos.Add(corners.NearTopLeft);
            pos.Add(corners.FarTopLeft);

            return pos.ToArray();
        }

        public static ImmutableGeometryData BuildSphere(Vector3 center, float radius) {
            int thetaDiv = 32;
            int phiDiv = 32;
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var texCoor = new List<Vector2>();

            var dt = 2f * MathF.PI / thetaDiv;
            var dp = MathF.PI / phiDiv;

            for (var pi = 0; pi <= phiDiv; pi++) {
                var phi = pi * dp;

                for (var ti = 0; ti <= thetaDiv; ti++) {
                    // start the mesh on the x axis
                    var theta = ti * dt;

                    // Spherical coordinates
                    // http://mathworld.wolfram.com/SphericalCoordinates.html
                    float x = MathF.Cos(theta) * MathF.Sin(phi);
                    float y = MathF.Sin(theta) * MathF.Sin(phi);
                    float z = MathF.Cos(phi);

                    var p = new Vector3(
                        center.X + (radius * x),
                        center.Y + (radius * y),
                        center.Z + (radius * z));
                    positions.Add(p);
                    normals.Add(new Vector3(x, y, z));
                    var uv = new Vector2(theta / (2 * MathF.PI), phi / MathF.PI);
                    texCoor.Add(uv);
                }
            }


            int rows = phiDiv + 1;
            int columns = thetaDiv;
            var indices = new List<int>();

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) {
                    int ij = (i * columns) + j;
                    if (i > 0) {//ignore first slice because there only one tringle
                        indices.Add(ij);
                        indices.Add(ij + 1 + columns);
                        indices.Add(ij + 1);
                    }
                    if (i < rows - 1) {//ignore last slice because there only one tringle
                        indices.Add(ij + 1 + columns);
                        indices.Add(ij);
                        indices.Add(ij + columns);
                    }
                }
            }

            return new ImmutableGeometryData(positions, normals, indices, texCoor);
        }

        public static ImmutableGeometryData BuildCylinder(Vector3 start, Vector3 axis, float radius, float lenght) {
            var positions = new Dictionary<Vector3, int>();
            var normals = new List<Vector3>();
            var texCoor = new List<Vector2>();
            var indices = new List<int>();

            var circles = new List<List<Vector3>>();

            for (var z = 0; z < lenght; z += 10) {
                var circle = new List<Vector3>();
                var st = (circles.Count % 2) == 0 ? 0f : 5f;
                for (var angle = st; angle < 360; angle += 10f) {
                    var rad = angle.ToRad();
                    var x = radius * MathF.Cos(rad);
                    var y = radius * MathF.Sin(rad);

                    circle.Add(new Vector3(x, y, z));
                }
                circles.Add(circle);
            }

            var prevCircle = circles[0];
            for (var i = 1; i < circles.Count; i++) {
                var circle = circles[i];

                var prevVb = prevCircle[0];
                var prevVt = circle[0];
                var cc0 = prevCircle.GetCenter();
                var cc1 = circle.GetCenter();

                int indx0, indx1, indx2, indx3;
                indx0 = indx1 = indx2 = indx3 = -1;
                for (var vI = 1; vI < prevCircle.Count; vI++) {
                    var vb = prevCircle[vI];
                    var vt = circle[vI];

                    var v0 = prevVb;
                    var v1 = prevVt;
                    var v2 = vb;
                    var v3 = vt;

                    if (!positions.TryGetValue(v0, out indx0)) {
                        indx0 = positions.Count;
                        positions.Add(v0, indx0);
                        normals.Add((v0 - cc0).Normalized());
                    }
                    if (!positions.TryGetValue(v1, out indx1)) {
                        indx1 = positions.Count;
                        positions.Add(v1, indx1);
                        normals.Add((v1 - cc1).Normalized());
                    }
                    if (!positions.TryGetValue(v2, out indx2)) {
                        indx2 = positions.Count;
                        positions.Add(v2, indx2);
                        normals.Add((v2 - cc0).Normalized());
                    }
                    if (!positions.TryGetValue(v3, out indx3)) {
                        indx3 = positions.Count;
                        positions.Add(v3, indx3);
                        normals.Add((v3 - cc0).Normalized());
                    }
                    indices.Add(indx1);
                    indices.Add(indx2);
                    indices.Add(indx0);

                    indices.Add(indx2);
                    indices.Add(indx1);
                    indices.Add(indx3);

                    prevVb = vb;
                    prevVt = vt;
                }
                indices.Add(positions[prevCircle[0]]);
                indices.Add(indx2);
                indices.Add(indx3);

                indices.Add(positions[circle[0]]);
                indices.Add(positions[prevCircle[0]]);
                indices.Add(indx3);

                prevCircle = circle;
            }

            var geo = new ImmutableGeometryData(positions.Keys.ToArray(), normals, indices);

            var box = AxisAlignedBox.CreateFrom(geo.Positions);

            var cross = Vector3.Cross(axis, Vector3.UnitZ);
            if (cross == Vector3.Zero) {
                cross = Vector3.UnitX;
            }
            cross.Normalize();
            var angleRad = Vector3.UnitZ.AngleRad(axis);
            var rotate = Matrix4x4.CreateFromAxisAngle(cross, -angleRad);
            var moveToStart = Matrix4x4.CreateTranslation(start);

            geo = geo.Transform(rotate * moveToStart);

            return geo;
        }
    }
}
