using System.Numerics;

namespace D3DLab.ECS.Components {
    public readonly struct MovingComponent : IGraphicComponent {
        public static MovingComponent CreateTranslation(in Vector3 position) {
            return new MovingComponent(Matrix4x4.CreateTranslation(position), true);
        }
        public static MovingComponent Create(in Matrix4x4 m) {
            return new MovingComponent(m, true);
        }

        //public static MovingComponent Identity() {
        //    return new MovingComponent(Matrix4x4.Identity, true);
        //}

        public void Dispose() {
        }

        public Matrix4x4 MovingMatrix { get; }
        public ElementTag Tag { get; }
        public bool IsValid { get; }

        MovingComponent(in Matrix4x4 matrixWorld, bool isvalid) : this() {
            MovingMatrix = matrixWorld;
            Tag = ElementTag.New();
            IsValid = isvalid;
        }
    }
}
