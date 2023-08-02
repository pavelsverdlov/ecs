using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.SDX.Engine.Components;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Systems {
    public class MovingSystem : BaseEntitySystem, IGraphicSystem, IGraphicSystemContextDependent {
        public IContextState ContextState { set; private get; }

        protected override void Executing(ISceneSnapshot snapshot) {
            var emanager = ContextState.GetEntityManager();

            foreach (var entity in emanager.GetEntities()) {
                if (entity.TryGetComponent<MovingComponent>(out var moving)) {
                    if (moving.MovingMatrix.IsIdentity) {
                        entity.RemoveComponent<MovingComponent>();
                        continue;
                    }

                    var transform = entity.GetComponent<TransformComponent>();
                    //dispose transform buffer to force update it in render technique
                    if (entity.TryGetComponent<D3DRenderComponent>(out var d3d)) {
                        d3d.TransformWorldBuffer.Dispose();
                    }                    
                    //
                    entity.UpdateComponent(transform.Multiply(moving.MovingMatrix));

                    entity.RemoveComponent<MovingComponent>();
                }
            }
        }
    }
}
