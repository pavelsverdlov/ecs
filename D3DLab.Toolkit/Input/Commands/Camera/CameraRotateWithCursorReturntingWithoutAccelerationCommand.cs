using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Input;

namespace D3DLab.Toolkit.Input.Commands.Camera {
    public class CameraRotateWithCursorReturntingWithoutAccelerationCommand : IInputCommand {
        readonly float sensitivity;

        public InputStateData InputState { get; }

        public CameraRotateWithCursorReturntingWithoutAccelerationCommand(InputStateData state, float sensitivity) {
            this.InputState = state;
            this.sensitivity = sensitivity;
        }


        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var entity = context.GetEntityManager().GetEntity(snapshot.CurrentCameraTag);

            if (!entity.TryGetComponent(out OrthographicCameraComponent ccom)) { return false; }

            var p11 = InputState.ButtonsStates[GeneralMouseButtons.Right].PointV2;
            var p2 = InputState.CurrentPosition;
            var data = new MovementData { Begin = p11, End = p2 };

            var state = ccom.GetState();
            entity.UpdateComponent(CameraMovementComponent.CreateRotate(state, data, sensitivity));

            return true;
        }
    }
}
