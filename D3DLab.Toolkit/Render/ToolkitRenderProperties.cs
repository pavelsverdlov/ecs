using D3DLab.ECS.Camera;

namespace D3DLab.Toolkit.Render {
    public class ToolkitRenderProperties : IToolkitFrameProperties {
        public CameraState CameraState { get; set; }
        public SharpDX.Direct3D11.Buffer Game { get; }
        public SharpDX.Direct3D11.Buffer Lights { get; }
        public ToolkitRenderProperties(SharpDX.Direct3D11.Buffer game, SharpDX.Direct3D11.Buffer light, CameraState cameraState) {
            Game = game;
            Lights = light;
            CameraState = cameraState;
        }
    }
}
