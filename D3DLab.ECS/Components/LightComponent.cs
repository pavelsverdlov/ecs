using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    public readonly struct LightComponent : IGraphicComponent {
       

        public static LightComponent Create(float intensity, int index, Vector3 direction, Vector3 position, LightTypes type) {
            return new LightComponent(intensity, index, position, direction, type);
        }
        public static LightComponent CreateDirectional(float intensity, int index, Vector3 direction) {
            return new LightComponent(intensity, index, Vector3.Zero, direction, LightTypes.Directional);
        }
        public static LightComponent CreatePoint(float intensity, int index, Vector3 position) {
            return new LightComponent(intensity, index, position, Vector3.Zero, LightTypes.Point);
        }
        public static LightComponent CreateAmbient(float intensity, int index) {
            return new LightComponent(intensity, index, Vector3.Zero, Vector3.Zero, LightTypes.Ambient);
        }

        /// <summary>
        /// 0 - 1 range
        /// </summary>
        public float Intensity { get;  }
        public int Index { get;}
        public Vector3 Position { get;}
        public Vector3 Direction { get;   }
        public LightTypes Type { get;  }

        public ElementTag Tag { get; }
        public bool IsModified { get;}
        public bool IsValid { get;  }
        public bool IsDisposed { get;  }
        
        LightComponent(float intensity, int index, Vector3 position, Vector3 direction, LightTypes type) : this() {
            Tag = ElementTag.New();
            IsValid = true;
            Intensity = intensity;
            Index = index;
            Position = position;
            Direction = direction;
            Type = type;
        }

        public LightComponent ApplyDirection(Vector3 direction)
            => new LightComponent(Intensity, Index, Position, direction, Type);

        public void Dispose() {
            
        }
    }
}
