using D3DLab.ECS;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit._CommonShaders {
    [StructLayout(LayoutKind.Sequential)]
    public struct LightStructBuffer {

        public static LightStructBuffer From(LightState state) {
            return new LightStructBuffer(state.Type, state.Position, Vector4.Normalize(new Vector4(state.Direction, 0)), state.Color, state.Intensity);
        }

        public const int MaxCount = 3;
        public const int RegisterResourceSlot = 1;
        public readonly static int Size = Unsafe.SizeOf<LightStructBuffer>();


        public readonly Vector4 Color;
        public readonly Vector4 Direction;

        public readonly float Intensity;
        public readonly float Type;
        Vector2 offset;

        //public readonly Vector3 Position;

        public LightStructBuffer(LightTypes type, Vector3 pos, Vector4 dir, Vector4 color, float intensity):this() {
            Type = (uint)type;
            Intensity = intensity;
           // Position = pos;
            Direction = dir;
            Color = color;
            offset = Vector2.Zero;
        }
    }
}
