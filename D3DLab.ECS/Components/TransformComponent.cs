using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    public readonly struct TransformComponent : IGraphicComponent {
       

        public static TransformComponent Create(in Matrix4x4 matrixWorld) {
            return new TransformComponent(matrixWorld);
        }

        public static TransformComponent Identity() {
            return new TransformComponent(Matrix4x4.Identity);
        }

        public void Dispose() {
            //throw new Exception("Imposible to remove TransformComponent, use MovingComponent to update it.");
        }

        public Matrix4x4 MatrixWorld { get; }
        public ElementTag Tag { get;  }
        public bool IsValid { get; }

        TransformComponent(Matrix4x4 matrixWorld) : this() {
            IsValid = true;
            MatrixWorld = matrixWorld;
            Tag = ElementTag.New();
        }

        public TransformComponent Multiply(in Matrix4x4 matrix) => Create(MatrixWorld * matrix);
    }
}
