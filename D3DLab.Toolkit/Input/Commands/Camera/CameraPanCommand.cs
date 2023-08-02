using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Input.Commands.Camera {
    public class CameraPanCommand :  IInputCommand {
        public InputStateData InputState { get; }
        public CameraPanCommand(InputStateData state) {
            this.InputState = state;
        }

        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var entity = context.GetEntityManager().GetEntity(snapshot.CurrentCameraTag);

            var p1 = InputState.ButtonsStates[GeneralMouseButtons.Right].PointV2;// InputState.PrevPosition;
            var p2 = InputState.CurrentPosition;

            var data = new MovementData { Begin = p1, End = p2 };

            var state = entity.GetComponent<OrthographicCameraComponent>().GetState();

            if (entity.TryGetComponent<CameraMovementComponent>(out var movement)) {
                //get prev state... means manipulate is continuing 
                state = movement.State;
            }

            entity.UpdateComponent(CameraMovementComponent.CreatePan(state, data));
            return true;
        }
    }
}
