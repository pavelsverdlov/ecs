using D3DLab.ECS;

using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public readonly struct HittableComponent : IGraphicComponent {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority">0 - max priority;uint.MaxValue - min priority</param>
        /// <returns></returns>
        public static HittableComponent Create(uint priority) {
            return new HittableComponent(priority);
        }


        HittableComponent(uint priorityIndex) : this() {
            PriorityIndex = priorityIndex;
            IsValid = true;
            Tag = ElementTag.New();
        }

        public uint PriorityIndex { get; }
        public ElementTag Tag { get; }
        public bool IsModified { get; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() {
        }
    }
}
