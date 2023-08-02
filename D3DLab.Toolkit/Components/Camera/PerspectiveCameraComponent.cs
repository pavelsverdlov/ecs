using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components.Camera {
    public class PerspectiveCameraComponent : GeneralCameraComponent {

        public float FieldOfViewRadians { get; set; }
        public float MinimumFieldOfView { get; set; }
        public float MaximumFieldOfView { get; set; }

        public PerspectiveCameraComponent() {
            ResetToDefault();
        }

        public override Matrix4x4 UpdateProjectionMatrix(SurfaceSize size) {
            float aspectRatio = size.Width / size.Height;

            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                        FieldOfViewRadians,
                        aspectRatio,
                        NearPlaneDistance,
                        FarPlaneDistance);

            return ProjectionMatrix;
        }

        public override void ResetToDefault() {
            UpDirection = Vector3.UnitY;
            FieldOfViewRadians = 1.05f;
            NearPlaneDistance = 1f;
            LookDirection = ForwardRH;
            Position = Vector3.UnitZ * 10f;

            FarPlaneDistance = Position.Length() * 70;
        }

        protected override void CreateState(out CameraState state) {
            state = CameraState.PerspectiveState();
        }
    }
}
