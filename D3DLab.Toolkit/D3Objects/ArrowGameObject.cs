using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public class ArrowGameObject : SingleVisualObject {

        ArrowGameObject(ElementTag tag) :base(tag, "arrow"){
        }

        public static ArrowGameObject Create(IContextState context, ElementTag tag, ArrowData data, bool visible = true) {
            var geo = GeometryBuilder.BuildArrow(data);

            var geoId = context.GetGeometryPool()
               .AddGeometry(geo);

            var en = context.GetEntityManager()
              .CreateEntity(tag)
              .AddComponent(visible ? 
                RenderableComponent.AsTriangleColored(SharpDX.Direct3D.PrimitiveTopology.TriangleStrip)
                : RenderableComponent.AsTriangleColored(SharpDX.Direct3D.PrimitiveTopology.TriangleStrip).Disable())
              .AddComponent(TransformComponent.Identity())
              .AddComponent(MaterialColorComponent.Create(data.color))
              .AddComponent(geoId)
              ;

            return new ArrowGameObject(en.Tag);
        }
    }
}
