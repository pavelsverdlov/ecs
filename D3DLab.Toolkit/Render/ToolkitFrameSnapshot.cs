using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Input;
using D3DLab.Toolkit.Math3D;
using System;

namespace D3DLab.Toolkit.Render {
    class ToolkitFrameSnapshot : ISceneSnapshot {
        public IManagerChangeNotify Notifier { get; set; }
        public IRenderableSurface Surface { get; set; }
        public InputSnapshot InputSnapshot { get; set; }
        public TimeSpan FrameRateTime { get; set; }


        public CameraState Camera { get; private set; }
        public LightState[] Lights { get; set; }
        public IViewport Viewport { get; }

        public ElementTag CurrentCameraTag { get; }
        public ElementTag WorldTag { get; }

        public ToolkitFrameSnapshot(ElementTag worldTag, ElementTag camera) {
            Viewport = new Viewport();
            WorldTag = worldTag;
            CurrentCameraTag = camera;
        }

        public void UpdateCamera(ElementTag tag, CameraState state) {
            Camera = state;
        }

        public void UpdateLight(int index, LightState state) {
            Lights[index] = state;
        }
    }

}
