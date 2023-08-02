using D3DLab.ECS;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Components;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using D3DLab.Toolkit.Techniques.TriangleColored;
using D3DLab.Toolkit.Components;
using SharpDX.Direct3D11;
using D3DLab.Toolkit;
using System.IO;
using D3DLab.SDX.Engine.Components;
using D3DLab.Toolkit.Math3D;

namespace D3DLab.Toolkit {
    public static class EntityBuilders {
        public static GraphicEntity BuildColored(IContextState context, ElementTag tag, ImmutableGeometryData geometry, 
            Vector4 v4color, CullMode cullMode) {

            var material = MaterialColorComponent.Create(v4color);

            var manager = context.GetEntityManager();
            var geo = context.GetGeometryPool()
                .AddGeometry(geometry);

            return manager.CreateEntity(tag)
                .AddComponent(TransformComponent.Identity())
                .AddComponent(material)
                .AddComponent(geo)
                .AddComponent(RenderableComponent.AsTriangleColoredList(cullMode));
        }

        public static GraphicEntity BuildColored(IContextState context,
            IReadOnlyCollection<Vector3> pos, IReadOnlyCollection<int> indexes,
            IReadOnlyCollection<Vector3> norm, Vector4 v4color, CullMode cullMode) {

            var mormals = norm ?? pos.ToList().CalculateNormals(indexes.ToList()).AsReadOnly();

            return BuildColored(context, ElementTag.New("TriangleColored"), new ImmutableGeometryData(
                    pos,
                    mormals,
                    indexes), v4color,cullMode );
        }

        public static GraphicEntity BuildTextured(IContextState context,
            IReadOnlyCollection<Vector3> pos, IReadOnlyCollection<int> indexes, IReadOnlyCollection<Vector2> texCoor, FileInfo texture, CullMode cullMode) {
            if (texCoor == null) {
                throw new Exception("Geo must have TextCoor.");
            }
            var manager = context.GetEntityManager();
            var en = manager.CreateEntity(new ElementTag("TexturedGeometry" + Guid.NewGuid()));

            var material = MaterialColorComponent.CreateTransparent().ApplyAlpha(1);

            var geo = context.GetGeometryPool()
              .AddGeometry(new ImmutableGeometryData(
                  pos,
                  pos.ToList().CalculateNormals(indexes.ToList()).AsReadOnly(),
                  indexes,
                  texCoor));

            en.AddComponent(TransformComponent.Identity())
              .AddComponent(geo)
              .AddComponent(material)
              .AddComponent(new D3DTexturedMaterialSamplerComponent(texture))
              .AddComponent(RenderableComponent.AsTriangleTexturedList(cullMode));

            return en;
        }
    }
}
