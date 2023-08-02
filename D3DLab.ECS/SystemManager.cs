using System;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.ECS {
    public sealed class SystemManager : ISystemManager {
        readonly List<IGraphicSystem> systems = new List<IGraphicSystem>();
        public TSystem CreateSystem<TSystem>() where TSystem : class, IGraphicSystem, new() {
            var sys = new TSystem();

            if (sys is IComponentSystemIncrementId incrementId) {
                incrementId.ID = systems.Count;
            }            

            if(sys is IGraphicSystemContextDependent dependent) {
                dependent.ContextState = context;
            }

            systems.Add(sys);            
            notify.NotifyAdd<IGraphicSystem>(sys);
            return sys;
        }
        public IEnumerable<IGraphicSystem> GetSystems() {
            return systems;
        }

        public IEnumerable<T> GetSystems<T>() where T : IGraphicSystem {
            return systems.OfType<T>();
        }

        public void Dispose() {
            foreach(var sys in systems) {
                sys.Dispose();
            }
            systems.Clear();
        }

        readonly IManagerChangeNotify notify;
        readonly IContextState context;

        public SystemManager(IManagerChangeNotify notify, IContextState context) {
            this.notify = notify;
            this.context = context;
        }
    }
}
