using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Common;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit._CommonShaders {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct GameStructBuffer {

        public readonly static int Size = Unsafe.SizeOf<GameStructBuffer>();

        public static GameStructBuffer FromCameraState(CameraState state, SurfaceSize size) {
            return new GameStructBuffer(
                      Matrix4x4.Transpose(state.ViewMatrix),
                      Matrix4x4.Transpose(state.ProjectionMatrix),
                      state.LookDirection,
                      state.UpDirection,
                      state.Position,
                      size);
        }

        public const int RegisterResourceSlot = 0;

        public readonly Vector4 CameraLook;
        public readonly Vector4 CameraUp;
        public readonly Vector4 CameraPosition;
        /// <summary>
        /// [w,h,1/w,1/h]
        /// </summary>
        public readonly Vector4 Viewport;

        public readonly Matrix4x4 View;
        public readonly Matrix4x4 Projection;

        //  

        public GameStructBuffer(Matrix4x4 view, Matrix4x4 proj, Vector3 lookDirection, Vector3 up, Vector3 pos, SurfaceSize size) {
            View = view;
            Projection = proj;
            CameraLook = new Vector4(Vector3.Normalize(lookDirection), 0);// Matrix4x4.Identity ;// lookDirection;
            CameraUp = new Vector4(Vector3.Normalize(up), 0);
            CameraPosition = new Vector4(pos, 1);
            Viewport = new Vector4(size.Width, size.Height, 1f / size.Width, 1f / size.Height);
        }
    }
}
