using D3DLab.ECS.Camera;
using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS {    
    public interface IContextState {
        void BeginState();
        void EndState();
        ILabLogger Logger { get; }

        ISynchronizationContext GetSynchronizationContext();
        IComponentManager GetComponentManager();
        IEntityManager GetEntityManager();
        ISystemManager GetSystemManager();
        IGeometryMemoryPool GetGeometryPool();
        IOctreeManager GetOctreeManager();
        void SwitchTo(int stateTo);
        EntityOrderContainer EntityOrder { get; }

        void Dispose();
    }

    public interface IRenderState {
        IContextState ContextState { get; }
        float Ticks { get; }
        IRenderableWindow Window { get; }
        CameraState Camera { get; }
    }
}
