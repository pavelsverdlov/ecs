using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public readonly struct FollowCameraDirectLightComponent : IGraphicComponent {
        public static FollowCameraDirectLightComponent Create() {
            return new FollowCameraDirectLightComponent(ElementTag.New());
        }

        public ElementTag Tag { get;  }
        public bool IsModified { get; }
        public bool IsValid => true;
        public bool IsDisposed { get; }
        FollowCameraDirectLightComponent(ElementTag tag) : this() {
            Tag = tag;
        }
        public void Dispose() {
        }
    }
}
