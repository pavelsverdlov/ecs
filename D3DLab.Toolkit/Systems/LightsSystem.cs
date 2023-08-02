using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Systems;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Systems {
    public class LightsSystem : DefaultLightsSystem {
        protected override void OnExecuting(GraphicEntity entity, ref LightComponent light, ref ColorComponent color, ISceneSnapshot snapshot) {

            if (entity.Contains<FollowCameraDirectLightComponent>() && light.Direction != snapshot.Camera.LookDirection) {
                entity.UpdateComponent(light.ApplyDirection(snapshot.Camera.LookDirection));
            }

            base.OnExecuting(entity, ref light, ref color, snapshot);
        }
    }
}
