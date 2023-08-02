using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit._CommonShaders {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TransforStructBuffer {
        public static TransforStructBuffer ToTranspose(Matrix4x4 world) {
            Matrix4x4.Invert(world, out var inverted);
            return new TransforStructBuffer(Matrix4x4.Transpose(world), Matrix4x4.Transpose(inverted));
        }

        public const int RegisterResourceSlot = 2;
        public readonly Matrix4x4 World;
        public readonly Matrix4x4 WorldInverse;

        TransforStructBuffer(Matrix4x4 world, Matrix4x4 inverse) {
            World = world;
            WorldInverse = inverse;
        }
    }
}
