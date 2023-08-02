using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Input;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Systems {
    public class DefaultOrthographicCameraSystem :
        BaseEntitySystem, IGraphicSystem, IGraphicSystemContextDependent {
        public IContextState ContextState { protected get; set; }

        protected override void Executing(ISceneSnapshot snapshot) {
            var window = snapshot.Surface;
            var emanager = ContextState.GetEntityManager();

            var entity = emanager.GetEntity(snapshot.CurrentCameraTag);

            if (!entity.TryGetComponent(out OrthographicCameraComponent camera)) {
                Debug.WriteLine("Camera System: no camera in context");
                return;
            }

            if (entity.TryGetComponent(out CameraMovementComponent movement) && movement.IsValid) {
                bool needUpdate;
                bool needRemove;
                camera = OrthographicCameraComponent.Clone(camera);
                switch (movement.MovementType) {
                    case CameraMovementComponent.MovementTypes.Rotate:
                        camera = HandleRotatingWithCursorReturningPosition(snapshot, camera, movement);
                        needUpdate = true;
                        needRemove = true;
                        break;
                    case CameraMovementComponent.MovementTypes.Zoom:
                        camera = HandleZooming(snapshot, camera, movement);
                        needUpdate = true;
                        needRemove = true;
                        break;
                    case CameraMovementComponent.MovementTypes.Pan:
                        camera = HandlePanning(snapshot, camera, movement);
                        needUpdate = true;
                        needRemove = false;
                        break;
                    case CameraMovementComponent.MovementTypes.ChangeRotationCenter:
                        camera = HandleChangingRotateCenter(snapshot, camera, ref movement);
                        needUpdate = true;
                        needRemove = true;
                        break;
                    default:
                        throw new Exception($"Type {movement.MovementType} is not supported.");
                }
                //entity.RemoveComponent(movement);
                if (needUpdate) {
                    entity.UpdateComponent(camera);
                }
                if (needRemove) {
                    //need to remove zoom com. manually because WinForm pushs many mouse wheel commands for one user scroll 
                    //also remove for all, there is no garantee to get mouse up
                    entity.RemoveComponent<CameraMovementComponent>();
                }
            }

            camera.UpdateViewMatrix();
            camera.UpdateProjectionMatrix(window.Size);

            snapshot.UpdateCamera(entity.Tag, camera.GetState());
        }
        protected virtual OrthographicCameraComponent HandleZooming(ISceneSnapshot snapshot, OrthographicCameraComponent camera, CameraMovementComponent component) {
            var changed = OrthographicCameraComponent.Clone(camera);
            float delta = component.Delta;
            //var nscale = camera.Scale - (delta * component.SpeedValue);
            //move always by half of scale 
            var nscale = changed.Scale - (camera.Scale * 0.5f * MathF.Sign(delta));
            if (nscale > 0) {
                changed.Scale = nscale;
            }

            return changed;
        }
        protected virtual OrthographicCameraComponent HandleRotatingWithCursorReturningPosition(ISceneSnapshot snapshot, OrthographicCameraComponent camera, CameraMovementComponent component) {
            var state = component.State;
            var data = component.MovementData;

            var cursorPosition = snapshot.InputSnapshot.CurrentInputState.ButtonsStates[GeneralMouseButtons.Right]
                .CursorPoint.ToDrawingPoint();

            var rotateAround = camera.RotatePoint;
            var delta = data.End - data.Begin;
            var relativeTarget = rotateAround - state.Target;
            var relativePosition = rotateAround - state.Position;

            var cUp = Vector3.Normalize(state.UpDirection);
            var up = state.UpDirection;
            var dir = Vector3.Normalize(state.LookDirection);
            var right = Vector3.Cross(dir, cUp);

            float d = -0.5f;
            d *= component.SpeedValue;

            var xangle = d * 1 * delta.X / 180 * (float)Math.PI;
            var yangle = d * delta.Y / 180 * (float)Math.PI;

            //System.Diagnostics.Trace.WriteLine($"up: {up}/{xangle}, right: {right}/{yangle}");

            var q1 = Quaternion.CreateFromAxisAngle(up, xangle);
            var q2 = Quaternion.CreateFromAxisAngle(right, yangle);
            Quaternion q = q1 * q2;

            var m = Matrix4x4.CreateFromQuaternion(q);

            var newRelativeTarget = Vector3.Transform(relativeTarget, m);
            var newRelativePosition = Vector3.Transform(relativePosition, m);

            var newTarget = rotateAround - newRelativeTarget;
            var newPosition = rotateAround - newRelativePosition;

            var changed = OrthographicCameraComponent.Clone(camera);

            changed.UpDirection = Vector3.TransformNormal(cUp, m);
            changed.LookDirection = (newTarget - newPosition);
            changed.Position = newPosition;

            return changed;
        }
        protected virtual OrthographicCameraComponent HandlePanning(ISceneSnapshot snapshot, OrthographicCameraComponent camera, CameraMovementComponent component) {
            var state = component.State;
            var data = component.MovementData;

            var changed = OrthographicCameraComponent.Clone(camera);

            var forward = state.LookDirection;
            var left = Vector3.Cross(state.UpDirection, forward).Normalized();
            var up = Vector3.Cross(forward, left).Normalized();

            var move = new Vector2(data.End.X - data.Begin.X, data.End.Y - data.Begin.Y);
            var PanK = (changed.Width * changed.Scale) / snapshot.Surface.Size.Width;
            var projectionMove = new Vector2(move.X * PanK, move.Y * PanK);

            var panVector = left * projectionMove.X + up * projectionMove.Y;

            changed.Position = state.Position + panVector;

            return changed;
        }
        protected virtual OrthographicCameraComponent HandleChangingRotateCenter(ISceneSnapshot snapshot,
            OrthographicCameraComponent camera, ref CameraMovementComponent component) {
            var world = ContextState.GetEntityManager().GetEntity(snapshot.WorldTag);

            if (world.TryGetComponents(
                    out CaptureTargetUnderMouseComponent capture,
                    out CollidedWithEntityByRayComponent collided)) {

                var changed = OrthographicCameraComponent.Clone(camera);
                changed.RotatePoint = collided.IntersectionPositionWorld;

                var begin = snapshot.Viewport.Vector3ToScreen(changed.Position, camera.GetState(), snapshot.Surface);
                var move = new Vector2(capture.ScreenPosition.X - begin.X, capture.ScreenPosition.Y - begin.Y);
                var PanK = (changed.Width * changed.Scale) / snapshot.Surface.Size.Width;
                var projectionMove = new Vector2(move.X * PanK, move.Y * PanK);
                var left = Vector3.Cross(changed.UpDirection, changed.LookDirection).Normalized();
                var up = Vector3.Cross(changed.LookDirection, left).Normalized();
                var panVector = left * projectionMove.X + up * projectionMove.Y;
                changed.Position += -panVector;

                world.RemoveComponents<CaptureTargetUnderMouseComponent, CollidedWithEntityByRayComponent>();

                camera = changed;
            }

            return camera;
        }
    }
}
