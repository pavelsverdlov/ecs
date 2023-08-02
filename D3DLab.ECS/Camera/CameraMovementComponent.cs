using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Camera {
    public struct MovementData {
        public Vector2 Begin;
        public Vector2 End;
    }



    public readonly struct CameraMovementComponent : IGraphicComponent {       

        public enum MovementTypes {
            Undefined,
            Zoom,
            Rotate,
            Pan,
            ChangeRotationCenter
        }

        public static CameraMovementComponent CreateZoom(CameraState state, MovementData movementData, int delta, float speedValue = 1) {
            return new CameraMovementComponent(state, movementData, MovementTypes.Zoom, delta, speedValue);
        }

        public static CameraMovementComponent CreatePan(CameraState state, MovementData movementData, float speedValue = 1) {
            return new CameraMovementComponent(state, movementData, MovementTypes.Pan, 0, speedValue);
        }

        public static CameraMovementComponent CreateRotate(CameraState state, MovementData movementData, float speedValue = 1) {
            return new CameraMovementComponent(state, movementData, MovementTypes.Rotate, 0, speedValue);

        }

        public static CameraMovementComponent ChangeRotationCenter(CameraState state, MovementData movementData) {
            return new CameraMovementComponent(state, movementData, MovementTypes.ChangeRotationCenter, 0, 0);
        }

        CameraMovementComponent(CameraState state, MovementData movementData, MovementTypes movementType, 
            int delta, float speedValue) : this() {
            Tag = ElementTag.New();
            IsValid = true;
            State = state;
            MovementData = movementData;
            MovementType = movementType;
            Delta = delta;
            SpeedValue = speedValue;
        }


        public ElementTag Tag { get; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }
        public bool IsModified { get; }
        //genneral
        public CameraState State { get; }
        public MovementData MovementData { get; }
        public MovementTypes MovementType { get; }

        //zooming
        public int Delta { get; }
        public float SpeedValue { get; }

        public void Dispose() {
           
        }

    }
}
