using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace D3DLab.Toolkit.D3Objects {

    public class LightIndexOccupiedException : Exception {
        public LightIndexOccupiedException(int index) : base($"Index '{index}' is occupied.") { }
    }

    public class LightObject : SingleVisualObject {
        static float lightpower = 1;
        static int lights = 0;
        static HashSet<int> occupiedIndex;
        static LightObject() {
            occupiedIndex = new HashSet<int>();
        }

        public LightObject(ElementTag tag, string desc) : base(tag, desc) { }


        #region Creators

        public static LightObject CreateFollowCameraDirectLight(IEntityManager manager, Vector3 direction, float intensity = 0.2f) {// ,
            var index = 2;
            if (!occupiedIndex.Add(index)) {
                throw new LightIndexOccupiedException(index);
            }

            var tag = new ElementTag("DirectionLight_" + Interlocked.Increment(ref lights));
            manager.CreateEntity(tag)
                   .AddComponent(LightComponent.CreateDirectional(intensity, index, direction))
                   .AddComponent(ColorComponent.CreateDiffuse(new Vector4(1, 1, 1, 1)))
                   .AddComponent(FollowCameraDirectLightComponent.Create());

            return new LightObject(tag, "DirectionLight");
        }

        public static LightObject CreatePointLight(IEntityManager manager, Vector3 position) {// 
            var index = 1;
            if (!occupiedIndex.Add(index)) {
                throw new LightIndexOccupiedException(index);
            }
            var tag = new ElementTag("PointLight_" + Interlocked.Increment(ref lights));

            manager.CreateEntity(tag)
                 .AddComponent(LightComponent.CreatePoint(0.4f, index, position))
                 .AddComponent(ColorComponent.CreateDiffuse(new Vector4(1, 1, 1, 1)));

            return new LightObject(tag, "PointLight");
        }

        public static LightObject CreateAmbientLight(IEntityManager manager, float intensity = 0.4f) {
            var index = 0;
            if (!occupiedIndex.Add(index)) {
                throw new LightIndexOccupiedException(index);
            }

            var tag = new ElementTag("AmbientLight_" + Interlocked.Increment(ref lights));
            var sv4 = SharpDX.Color.White.ToVector4();

            manager.CreateEntity(tag)
                   .AddComponent(LightComponent.CreateAmbient(intensity, index))
                   .AddComponent(ColorComponent.CreateDiffuse(new Vector4(sv4.X, sv4.Y, sv4.Z, sv4.W)));

            return new LightObject(tag, "AmbientLight");
        }

        public static LightObject CreateDirectionLight(IEntityManager manager, Vector3 direction, float intensity) {// ,
            var index = 2;
            if (!occupiedIndex.Add(index)) {
                throw new LightIndexOccupiedException(index);
            }

            var tag = new ElementTag("DirectionLight_" + Interlocked.Increment(ref lights));
            manager.CreateEntity(tag)
                   .AddComponent(LightComponent.CreateDirectional(intensity, index, direction))
                   .AddComponent(ColorComponent.CreateDiffuse(new Vector4(1, 1, 1, 1)));

            return new LightObject(tag, "DirectionLight");
        }

        #endregion        
    }
}
