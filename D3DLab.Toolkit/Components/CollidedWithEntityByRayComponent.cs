using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public readonly struct CollidedWithEntityByRayComponent : IGraphicComponent {
        public static CollidedWithEntityByRayComponent Create(ElementTag with, Vector3 intersectionPositionWorld)
           => new CollidedWithEntityByRayComponent(with, intersectionPositionWorld);

        public ElementTag With { get; }
        public Vector3 IntersectionPositionWorld { get; }

        CollidedWithEntityByRayComponent(ElementTag with, Vector3 intersectionPositionWorld) : this() {
            Tag = ElementTag.New("CollidedWith");
            With = with;
            IsValid = true;
            IntersectionPositionWorld = intersectionPositionWorld;
        }

        public ElementTag Tag { get; }
        public bool IsModified { get;  }
        public bool IsValid { get;  }
        public bool IsDisposed { get;  }

        public void Dispose() {
        }
    }
}
