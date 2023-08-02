using D3DLab.ECS;
using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace D3DLab.ECS {
    public abstract class BaseEntitySystem : IComponentSystemIncrementId, IDisposable {
        public int ID { get; set; }
        public TimeSpan ExecutionTime { get; private set; }

        readonly Stopwatch stopwatch;

        protected BaseEntitySystem() {
            stopwatch = new Stopwatch();
            //var ddd = Stopwatch.IsHighResolution;
        }

        public void Execute(ISceneSnapshot snapshot) {
            try {
                stopwatch.Restart();
                Executing(snapshot);
            } finally {
                stopwatch.Stop();
                ExecutionTime = stopwatch.Elapsed;
            }
        }

        protected abstract void Executing(ISceneSnapshot snapshot);
        public virtual void Dispose() {

        }
    }
    public interface INestedGraphicSystem {
        int OrderId { get; set; }
    }
    public abstract class ContainerSystem<TNestedSystem> : BaseEntitySystem, IGraphicSystemContextDependent
        where TNestedSystem : INestedGraphicSystem {

        public IContextState ContextState { set; protected get; }

        readonly SynchronizationContext<ContainerSystem<TNestedSystem>, TNestedSystem> synchronization;
        protected readonly List<TNestedSystem> nested;

        public ContainerSystem() {
            synchronization = new SynchronizationContext<ContainerSystem<TNestedSystem>, TNestedSystem>(this);
            nested = new List<TNestedSystem>();
        }

        public ContainerSystem<TNestedSystem> CreateNested<T>() where T : TNestedSystem {
            synchronization.Add((owner, tech) => {
                tech.OrderId = owner.nested.Count;
                if (tech is IGraphicSystemContextDependent dependent) {
                    dependent.ContextState = ContextState;
                }
                owner.nested.Add(tech);
            }, Activator.CreateInstance<T>());
            return this;
        }

        protected void Synchronize() {
            synchronization.Synchronize(-1);
        }
    }
}
