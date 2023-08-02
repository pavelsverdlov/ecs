using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Components {
    public readonly struct PerfomanceComponent : IGraphicComponent {
        

        public static PerfomanceComponent Create(double millisec, double total, double frameTimeMillisec) {
            int fps = frameTimeMillisec == 0 ? (int)total : (int)(total / frameTimeMillisec);

            return new PerfomanceComponent(millisec, fps) {
              
            };
        }

        public ElementTag Tag { get;  }
        public bool IsValid { get; }

        public double ElapsedMilliseconds { get; }
        public double FPS { get; }
        PerfomanceComponent(double elapsedMilliseconds, double fPS) : this() {
            Tag = ElementTag.New();
            ElapsedMilliseconds = elapsedMilliseconds;
            IsValid = true;
            FPS = fPS;
        }
        public override string ToString() {
            return $"Perfomance[ElapsedMilliseconds:{ElapsedMilliseconds} FPS:{FPS}]";
        }

        public void Dispose() {
        }
    }
}
