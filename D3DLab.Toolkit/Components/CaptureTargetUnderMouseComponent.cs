using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public readonly struct CaptureTargetUnderMouseComponent : IGraphicComponent {
        internal static CaptureTargetUnderMouseComponent Create(Vector2 v2)
            => new CaptureTargetUnderMouseComponent(v2);

        public Vector2 ScreenPosition { get; }
        public ElementTag Tag { get; }
        public bool IsModified { get;}
        public bool IsValid { get;  }
        public bool IsDisposed { get; }
        public void Dispose() {
           
        }

        CaptureTargetUnderMouseComponent(Vector2 screenPosition) : this() {
            ScreenPosition = screenPosition;
            Tag = ElementTag.New();
            IsValid = true;
        }
    }

}
