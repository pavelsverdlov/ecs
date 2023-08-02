using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace D3DLab.ECS {
    public interface IEngineSubscriber { }
    public interface IManagerChangeSubscriber<T> : IEngineSubscriber {
        void Add(in T obj);
        void Remove(in T obj);
    }

    public interface IEntityRenderSubscriber : IEngineSubscriber {
        void Render(IEnumerable<GraphicEntity> entities);
    }

    public interface IEngineSubscribe {
        void Subscribe(IEngineSubscriber s);
    }

    public interface IManagerChangeNotify {
        void NotifyAdd<T>(in T _object);
        void NotifyRemove<T>(in T _object);

    }
    public interface IEntityRenderNotify {
        void NotifyRender(IEnumerable<GraphicEntity> entities);
    }

    public sealed class EngineNotificator : IEngineSubscribe, IManagerChangeNotify, IEntityRenderNotify {
        private readonly List<IEngineSubscriber> subscribers;
        readonly Task runner;
        public EngineNotificator() {
            this.subscribers = new List<IEngineSubscriber>();
            runner = Task.CompletedTask;
        }

        public void Subscribe(IEngineSubscriber s) {
            subscribers.Add(s);
        }

        public void NotifyRender(IEnumerable<GraphicEntity> entities) {
            var handlers = subscribers.OfType<IEntityRenderSubscriber>();
            foreach (var handler in handlers) {
                try {
                    handler.Render(entities);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw ex;
#endif
                }
            }
        }

        public void NotifyAdd<T>(in T _object) {
            var local = _object;
            //runner.ContinueWith(x => {
            var handlers = subscribers.OfType<IManagerChangeSubscriber<T>>();
            foreach (var handler in handlers) {
                try {
                    handler.Add(local);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw ex;
#endif
                }
            }
            //});
        }

        public void NotifyRemove<T>(in T _object) {
            var local = _object;
            //runner.ContinueWith(x => {
            var handlers = subscribers.OfType<IManagerChangeSubscriber<T>>();
            foreach (var handler in handlers) {
                try {
                    handler.Remove(local);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw ex;
#endif
                }
            }
            //});
        }

        public void Clear() {
            subscribers.Clear();
        }
    }
}
