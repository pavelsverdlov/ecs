using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;
using D3DLab.Toolkit.Techniques.SpherePoint;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public class VisualSphereObject : SingleVisualObject {
        public struct Data {
            public Vector3 Center;
            public Vector4 Color;
            public float Radius;
        }

        public VisualSphereObject(ElementTag tag) : base(tag, "SphereByPoint") {
        }

        public static VisualSphereObject SphereGeo(IContextState context, ElementTag tag, Data data) {
            var geo = GeometryBuilder.BuildSphere(data.Center, data.Radius);
            
            var geoId = context.GetGeometryPool()
               .AddGeometry(geo);

            var en = context.GetEntityManager()
              .CreateEntity(tag)
              .AddComponent(RenderableComponent.AsTriangleColored(SharpDX.Direct3D.PrimitiveTopology.TriangleStrip))
              .AddComponent(TransformComponent.Identity())
              .AddComponent(MaterialColorComponent.Create(data.Color))
              .AddComponent(geoId)
              ;

            return new VisualSphereObject(en.Tag);
        }

        public static VisualSphereObject Create(IContextState context, ElementTag elet,  Data data) {

            var geo = GeometryBuilder.BuildSphere(data.Center, data.Radius);

            var geoId = context.GetGeometryPool()
               .AddGeometry(geo);

            var tag = context.GetEntityManager()
               .CreateEntity(elet)
               .AddComponent(SpherePointComponent.Create(data.Center, data.Radius))
               .AddComponent(MaterialColorComponent.Create(data.Color))
               .AddComponent(GeometryBoundsComponent.Create(AxisAlignedBox.FromSphere(data.Center, data.Radius)))
               .AddComponent(RenderableComponent.AsPoints())
               .AddComponent(TransformComponent.Identity())
               .Tag;

            return new VisualSphereObject(tag);
        }
    }

}
