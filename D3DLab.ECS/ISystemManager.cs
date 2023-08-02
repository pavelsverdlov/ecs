using System.Collections.Generic;

namespace D3DLab.ECS {
    public interface ISystemManager {
        TSystem CreateSystem<TSystem>() where TSystem : class, IGraphicSystem, new();
        IEnumerable<IGraphicSystem> GetSystems();
        IEnumerable<T> GetSystems<T>() where T : IGraphicSystem;
        //void AddSystem(IComponentSystem system);
        void Dispose();
    }

}
