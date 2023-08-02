using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Components {
    public readonly struct EmptyGraphicComponent : IGraphicComponent {
        EmptyGraphicComponent(ElementTag tag) : this() {
            Tag = tag;
        }

        public static EmptyGraphicComponent Create() {
            return new EmptyGraphicComponent(ElementTag.Empty);
        }

        public ElementTag Tag { get;}
        public bool IsModified { get; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() {

        }
    }
}
