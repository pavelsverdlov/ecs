using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Systems {
    public class CollidingSystem : BaseEntitySystem, IGraphicSystem, IGraphicSystemContextDependent {
        public IContextState ContextState { private get; set; }

        protected override void Executing(ISceneSnapshot snapshot) {
            var emanager = ContextState.GetEntityManager();
            var cmanager = ContextState.GetComponentManager();

            var world = emanager.GetEntity(snapshot.WorldTag);
            if (!world.TryGetComponent<CaptureTargetUnderMouseComponent>(out var capture) || !capture.IsValid) {
                return;
            }

            if (!cmanager.TryGetComponent<OrthographicCameraComponent>(snapshot.CurrentCameraTag, out var camera)
                || !capture.IsValid) {
                return;
            }
            var prevCameraState = camera.GetState();

            var geoPool = ContextState.GetGeometryPool();
            var rayWorld = snapshot.Viewport.UnProject(capture.ScreenPosition, prevCameraState, snapshot.Surface);

            var octree = ContextState.GetOctreeManager();

            var priority = uint.MaxValue;
            var closest = float.MaxValue;
            var hit = Vector3.Zero;

            var result = octree.GetColliding(ref rayWorld, enTag => {
                var entity = emanager.GetEntity(enTag);

                if (!entity.GetComponent<RenderableComponent>().IsRenderable) {
                    return false;
                }

                if (!entity.TryGetComponents<GeometryPoolComponent, TransformComponent, HittableComponent>(
                    out var geoId, out var transform, out var hittable) && geoId.IsValid && transform.IsValid) {
                    return false;
                }

                if (priority < hittable.PriorityIndex) {
                    return false; //ignore less priority objcts
                }

                if (priority > hittable.PriorityIndex) {
                    closest = float.MaxValue; //not interesting distance because the priority is highest 
                    priority = hittable.PriorityIndex;
                }

                var geo = geoPool.GetGeometry<GeometryStructures>(geoId);

                if (!geo.IsBuilt) {
                    return false;
                }

                var toLocal = Matrix4x4.Identity;
                var toWorld = Matrix4x4.Identity;
                var rayLocal = rayWorld;
                if (!transform.MatrixWorld.IsIdentity) {
                    toWorld = transform.MatrixWorld;
                    toLocal = toWorld.Inverted();
                    rayLocal = rayLocal.Transformed(toLocal);//to local
                }

                var hits = geo.HitByLocal(rayLocal);

                if (!hits.Any()) {
                    return false;
                }

                var min = hits.Min(x => x.Distance);

                if (closest < min) {
                    return false;
                }

                closest = min;
                hit = hits.First(x => x.Distance == min).Point.TransformedCoordinate(toWorld);
                return true;
            });

            if (result.Any()) {//important to update not add, to rewrite existed component
                world.UpdateComponent(CollidedWithEntityByRayComponent.Create(result.Last(), hit));
            } else {
                //can't find any hitable object under mouse
                world.RemoveComponent<CaptureTargetUnderMouseComponent>();
            }
        }
    }
}
