using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    public readonly struct GeometryPoolComponent : IGraphicComponent {
        public static GeometryPoolComponent Create(Guid index) {
            return new GeometryPoolComponent(index);
        }
        public readonly Guid Key;
        public GeometryPoolComponent(Guid index) : this() {
            Key = index;
            Tag = ElementTag.New();
            IsValid = true;
        }


        public ElementTag Tag { get;  }
        public bool IsModified { get;  }
        public bool IsValid { get;  }
        public bool IsDisposed { get;  }

        public void Dispose() {
        }
    }
}
