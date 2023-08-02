using D3DLab.ECS.Camera;
using D3DLab.ECS.Common;
using D3DLab.ECS.Ext;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    [Obsolete("Better to turn it into struct")]
    public abstract class GeneralCameraComponent : GraphicComponent {
        protected bool COORDINATE_SYSTEM_LH = false;

        // A unit Vector3 designating forward in a left-handed coordinate system
        public static readonly Vector3 ForwardLH = new Vector3(0, 0, 1);
        //A unit Vector3 designating forward in a right-handed coordinate system
        public static readonly Vector3 ForwardRH = new Vector3(0, 0, -1);

        public Vector3 RotatePoint { get; set; }


        public Vector3 Position { get; set; }
        public float NearPlaneDistance { get; set; }
        public Vector3 LookDirection { get; set; }
        public Vector3 UpDirection { get; set; }
        public float FarPlaneDistance { get; set; }

        public Matrix4x4 ViewMatrix { get; protected set; }
        public Matrix4x4 ProjectionMatrix { get; protected set; }

        public Vector3 Target => Position + LookDirection;

        protected GeneralCameraComponent() {

        }

        public Matrix4x4 UpdateViewMatrix() {
            ViewMatrix = Matrix4x4.CreateLookAt(Position, Target, UpDirection);
            return ViewMatrix;
        }
        public abstract Matrix4x4 UpdateProjectionMatrix(SurfaceSize size);

        public CameraState GetState() {
            CreateState(out var state);

            state.LookDirection = LookDirection;
            state.UpDirection = UpDirection;
            state.Position = Position;
            state.Target = Target;

            state.ViewMatrix = ViewMatrix;
            state.ProjectionMatrix = ProjectionMatrix;
            state.NearPlaneDistance = NearPlaneDistance;
            state.FarPlaneDistance = FarPlaneDistance;

            return state;
        }

        protected abstract void CreateState(out CameraState state);

        public abstract void ResetToDefault();

    }

    [Obsolete("Remake")]
    public class OrthographicCameraComponent : GeneralCameraComponent {
        
        public static OrthographicCameraComponent Clone(OrthographicCameraComponent com) {
            var copied = new OrthographicCameraComponent(new SurfaceSize(com.Width, com.prevScreenHeight));
            copied.Copy(com);
            return copied;
        }

        public float Width { get; set; }
        public float Scale { get; set; }

        protected float prevScreenWidth;
        protected float prevScreenHeight;

        public OrthographicCameraComponent(SurfaceSize surface) {
            this.prevScreenWidth = surface.Width;
            this.prevScreenHeight = surface.Height;
            Scale = 1;
            ResetToDefault();
        }

        public override Matrix4x4 UpdateProjectionMatrix(SurfaceSize size) {
            this.prevScreenWidth = size.Width;
            this.prevScreenHeight = size.Height;
            float aspectRatio = size.Width / size.Height;

            var frameWidth = Width * Scale;
            ProjectionMatrix = Matrix4x4.CreateOrthographic(
                        frameWidth,
                        frameWidth / aspectRatio,
                        NearPlaneDistance,
                        FarPlaneDistance);
            return ProjectionMatrix;
        }

        public override void ResetToDefault() {
            UpDirection = Vector3.UnitY;
            Width = 35f;
            //FieldOfViewRadians = 1.05f;
            NearPlaneDistance = 0.01f;
            LookDirection = ForwardRH;
            Position = Vector3.UnitZ * Width * 10f;

            FarPlaneDistance = Position.Length() * 50;

            Scale = 1;
        }

        protected override void CreateState(out CameraState state) {
            state = CameraState.OrthographicState();
        }

        public void Copy(OrthographicCameraComponent com) {
            FarPlaneDistance = com.FarPlaneDistance;
            NearPlaneDistance = com.NearPlaneDistance;
            
            RotatePoint = com.RotatePoint;
            Position = com.Position;
            UpDirection = com.UpDirection;
            LookDirection = com.LookDirection;
            
            ProjectionMatrix = com.ProjectionMatrix;
            ViewMatrix = com.ViewMatrix;

            Width = com.Width;
            Scale = com.Scale;
            prevScreenHeight = com.prevScreenHeight;
            prevScreenWidth = com.prevScreenWidth;
        }
    }
}
