using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Input.Commands.Camera {
    class CameraZoomCommand : IInputCommand {
        const float scrollSpeed = 0.5f;
        public InputStateData InputState { get; }

        public CameraZoomCommand(InputStateData state) {
            this.InputState = state;
        }


        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var entity = context.GetEntityManager().GetEntity(snapshot.CurrentCameraTag);
            var state = entity.GetComponent<OrthographicCameraComponent>().GetState();

            var delta = InputState.Delta;

            var data = new MovementData { End = InputState.CurrentPosition };

            entity.UpdateComponent(CameraMovementComponent.CreateZoom(state, data, delta, 0.001f));

            return true;
        }
    }
}
