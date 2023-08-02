using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public class VisualPolylineObject : SingleVisualObject {
        public VisualPolylineObject(ElementTag tag1) : base(tag1, "poly") {
        }

        public static VisualPolylineObject Create(IContextState context, ElementTag tag,
            Vector3[] points, Vector4 color, bool isVisible = true) {
            var manager = context.GetEntityManager();

            var indeces = new List<int>();
            var pos = new List<Vector3>();
            var prev = points[0];
            for (var i = 0; i < points.Length; i++) {
                pos.Add(prev);
                pos.Add(points[i]);
                prev = points[i];

            }
            for (var i = 0; i < pos.Count; i++) {
                indeces.AddRange(new[] { i, i });
            }
            var geo = context.GetGeometryPool().AddGeometry(new ImmutableGeometryData(pos, indeces));
            manager
               .CreateEntity(tag)
               .AddComponent(geo)
               .AddComponent(GeometryBoundsComponent.Create(pos))
               .AddComponent(TransformComponent.Identity())
               .AddComponent(ColorComponent.CreateDiffuse(color))
               .AddComponent(isVisible ? RenderableComponent.AsLineList() : RenderableComponent.AsLineList().Disable());

            return new VisualPolylineObject(tag);
        }
        public static VisualPolylineObject Create(IContextState context, ElementTag tag,
            Vector3[] points, Vector4[] colors, bool isVisible = true) {
            var manager = context.GetEntityManager();
            var indeces = new List<int>();
            for (var i = 0; i < points.Length; i++) {
                indeces.AddRange(new[] { i, i });
            }
            var geo = context.GetGeometryPool().AddGeometry(new ImmutableGeometryData(points, indeces, colors));
            manager
               .CreateEntity(tag)
               .AddComponent(geo)
               .AddComponent(GeometryBoundsComponent.Create(points))
               .AddComponent(TransformComponent.Identity())
               .AddComponent(isVisible ? RenderableComponent.AsLineList() : RenderableComponent.AsLineList().Disable());

            return new VisualPolylineObject(tag);
        }
        public static VisualPolylineObject CreateBox(IContextState context, ElementTag tag, BoundingBoxCorners xbox, Vector4 color) {
            var indeces = new List<int>();
            var pos = new List<Vector3>();

            var NearBottomLeft = xbox.NearBottomLeft;
            var NearBottomRight = xbox.NearBottomRight;
            var NearTopLeft = xbox.NearTopLeft;
            var NearTopRight = xbox.NearTopRight;

            var FarBottomLeft = xbox.FarBottomLeft;
            var FarBottomRight = xbox.FarBottomRight;
            var FarTopLeft = xbox.FarTopLeft;
            var FarTopRight = xbox.FarTopRight;
            //top
            pos.Add(NearBottomLeft);
            pos.Add(NearBottomRight);

            pos.Add(NearBottomRight);
            pos.Add(NearTopRight);

            pos.Add(NearTopRight);
            pos.Add(NearTopLeft);

            pos.Add(NearTopLeft);
            pos.Add(NearBottomLeft);
            //side
            pos.Add(NearBottomLeft);
            pos.Add(FarBottomLeft);

            pos.Add(NearBottomRight);
            pos.Add(FarBottomRight);

            pos.Add(NearTopRight);
            pos.Add(FarTopRight);

            pos.Add(NearTopLeft);
            pos.Add(FarTopLeft);
            //bottom

            pos.Add(FarBottomLeft);
            pos.Add(FarBottomRight);

            pos.Add(FarBottomRight);
            pos.Add(FarTopRight);

            pos.Add(FarTopRight);
            pos.Add(FarTopLeft);

            pos.Add(FarTopLeft);
            pos.Add(FarBottomLeft);

            for (var i = 0; i < pos.Count; i++) {
                indeces.AddRange(new[] { i, i });
            }

            var manager = context.GetEntityManager();

            var geo = context.GetGeometryPool()
                .AddGeometry(new ImmutableGeometryData(pos.ToArray(), indeces.ToArray()));
            manager
               .CreateEntity(tag)
               .AddComponent(geo)
               .AddComponent(GeometryBoundsComponent.Create(pos))
               .AddComponent(TransformComponent.Identity())
               .AddComponent(ColorComponent.CreateDiffuse(color))
               .AddComponent(RenderableComponent.AsLineList());

            return new VisualPolylineObject(tag);
        }
    }
}
