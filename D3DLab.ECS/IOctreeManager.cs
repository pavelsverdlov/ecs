using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS {
    public interface IOctreeManager : IDisposable {
        bool IsDrawingBoxesEnable { get; }
        IEnumerable<ElementTag> GetColliding(ref Ray ray, Func<ElementTag, bool> predicate);
        void EnableDrawingBoxes();
        void DisableDrawingBoxes();
    }   
}
