using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Input;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Input.Commands.Camera {
    public class CameraSetRotationCenterUnderMouseCommand : IInputCommand {
        public InputStateData InputState { get; }

        public CameraSetRotationCenterUnderMouseCommand(InputStateData state) {
            InputState = state;
        }

        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var p1 = InputState.ButtonsStates[GeneralMouseButtons.Left].PointV2;

            var manager = context.GetEntityManager();
            var camera = manager.GetEntity(snapshot.CurrentCameraTag);
            var world = manager.GetEntity(snapshot.WorldTag);

            world.UpdateComponent(CaptureTargetUnderMouseComponent.Create(p1));
            camera.AddComponent(CameraMovementComponent.ChangeRotationCenter(
                 camera.GetComponent<OrthographicCameraComponent>().GetState(),
                 new MovementData { Begin = p1 }));

            return true;
        }
    }
}
