using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;

namespace D3DLab.ECS.Context {

    public sealed class ManagerContainer {
        public ISynchronizationContext SynchronizationContext { get; }
        public ISystemManager SystemManager { get; }
        public IComponentManager ComponentManager { get; }
        public IEntityManager EntityManager { get; }
        public IGeometryMemoryPool GeoMemoryPool { get; }
        public EntityOrderContainer EntityOrder { get; }
        public IOctreeManager OctreeManager { get; }
        public ILabLogger Logger { get; }

        public ManagerContainer(IManagerChangeNotify notify, 
            IOctreeManager octree, IContextState context, IGeometryMemoryPool geoPool,
            RenderLoopSynchronizationContext syncContext, ILabLogger logger) {
            SynchronizationContext = syncContext;
            EntityOrder = new EntityOrderContainer();
            this.SystemManager = new SystemManager(notify, context);
            var encom = new EntityComponentManager(notify, EntityOrder, syncContext);
            GeoMemoryPool = geoPool;
            this.ComponentManager = encom;
            this.EntityManager = encom;
            OctreeManager = octree;
            Logger = logger;
        }

        public void Dispose() {
            SystemManager.Dispose();
            ComponentManager.Dispose();
            EntityManager.Dispose();
            GeoMemoryPool.Dispose();
            OctreeManager.Dispose();
        }
    }

    public abstract class BaseContextState : IContextState {
        readonly ContextStateProcessor processor;
        readonly ManagerContainer managers;

        public BaseContextState(ContextStateProcessor processor, ManagerContainer managers) {
            this.processor = processor;
            this.managers = managers;
        }

        public virtual void SwitchTo(int stateTo) {
            processor.SwitchTo(stateTo);
        }
        public virtual void EndState() { }
        public virtual void BeginState() { }
        public ILabLogger Logger => managers.Logger;
        public virtual IComponentManager GetComponentManager() { return managers.ComponentManager; }
        public virtual IEntityManager GetEntityManager() { return managers.EntityManager; }
        public virtual ISystemManager GetSystemManager() { return managers.SystemManager; }
        public virtual IGeometryMemoryPool GetGeometryPool() => managers.GeoMemoryPool;
        public virtual ISynchronizationContext GetSynchronizationContext() => managers.SynchronizationContext;

        public EntityOrderContainer EntityOrder { get { return managers.EntityOrder; } }

    

        public IOctreeManager GetOctreeManager() => managers.OctreeManager;
        public void Dispose() {
            managers.Dispose();
        }

    }

    public sealed class ContextStateProcessor : IContextState {
        private sealed class EmptyContextState : IContextState {
            public EntityOrderContainer EntityOrder => throw new NotImplementedException();

            public ILabLogger Logger => throw new NotImplementedException();

            public void BeginState() { }
            public void EndState() { }
            public IComponentManager GetComponentManager() { throw new NotImplementedException(); }
            public IEntityManager GetEntityManager() { throw new NotImplementedException(); }
            public ISystemManager GetSystemManager() { throw new NotImplementedException(); }
            public void SwitchTo(int stateTo) { throw new NotImplementedException(); }
            public void Dispose() {}

            public IGeometryMemoryPool GetGeometryPool() {
                throw new NotImplementedException();
            }

            public IOctreeManager GetOctreeManager() {
                throw new NotImplementedException();
            }

            public ISynchronizationContext GetSynchronizationContext() {
                throw new NotImplementedException();
            }

        }

        IContextState currentState;
        private readonly Dictionary<int, Func<ContextStateProcessor, IContextState>> states;
        
        public ContextStateProcessor() {
            states = new Dictionary<int, Func<ContextStateProcessor, IContextState>>();
            states.Add(-1, x => new EmptyContextState());
            currentState = new EmptyContextState();
        }

        public void AddState(int stateTo, Func<ContextStateProcessor, IContextState> func) {
            states.Add(stateTo, func);
        }

        public void SwitchTo(int stateTo) {
            currentState.EndState();
            currentState = states[stateTo](this);
            currentState.BeginState();
        }
        public ILabLogger Logger => currentState.Logger;
        public ISystemManager GetSystemManager() {
            return currentState.GetSystemManager();
        }

        public IComponentManager GetComponentManager() {
            return currentState.GetComponentManager();
        }

        public IEntityManager GetEntityManager() {
            return currentState.GetEntityManager();
        }

        public IGeometryMemoryPool GetGeometryPool() => currentState.GetGeometryPool();
        public IOctreeManager GetOctreeManager() => currentState.GetOctreeManager();
        public ISynchronizationContext GetSynchronizationContext() => currentState.GetSynchronizationContext();

        public EntityOrderContainer EntityOrder {
            get { return currentState.EntityOrder; }
        }

      

        public void BeginState() {
            currentState.BeginState();
        }

        public void EndState() {
            currentState.EndState();
        }

        public void Dispose() {
            states.Clear();
            currentState.Dispose();
        }

       
    }
}
