using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DLab.ECS.Common {
    public class DisposeObserver : IDisposable {
        readonly List<IDisposable> observables;
        bool disposed;

        ~DisposeObserver() {
            Dispose(false);
        }

        public DisposeObserver() {
            observables = new List<IDisposable>();
        }

        public void Dispose() {
            Dispose(true);
        }

        public void Observe(IDisposable observable) {
            if (disposed) {
                throw new Exception("Observer was disposed.");
            }
            observables.Add(observable);
        }
        protected virtual void Dispose(bool disposing) {
            if (disposed) {
                return;
            }
          //  if (disposing) {
                Disposer.DisposeAll(observables);
                observables.Clear();
            //}
            disposed = true;
        }
        /// <summary>
        /// Dispose only observe items, not disposing itself.
        /// Can be stil used after invoked
        /// </summary>
        public void DisposeObservables() {
            if (disposed) {
                throw new Exception("Observer was disposed.");
            }
            Disposer.DisposeAll(observables);
        }
    }

    public class DisposableSetter<T> : IDisposable where T : class, IDisposable {
        public bool HasValue => disposable != null;

        T disposable;
        bool disposed;

        ~DisposableSetter() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        public T Get() => disposable;
        public void Set(T b) {
            disposable?.Dispose();
            disposable = b;
            disposed = false;
        }

        public DisposableSetter(DisposeObserver watcher) {
            watcher.Observe(this);
            disposed = false;
        }
        protected virtual void Dispose(bool disposing) {
            if (disposed) {
                return;
            }
           // if (disposing) {
                disposable?.Dispose();
                disposable = null;
           // }
            disposed = true;
        }
    }

    public class EnumerableDisposableSetter<T> : IDisposable where T : class, IEnumerable<IDisposable>{
        T disposable;
        bool disposed;

        ~EnumerableDisposableSetter() {
            Dispose(false);
        }
        public EnumerableDisposableSetter(DisposeObserver watcher) {
            watcher.Observe(this);
        }
        public void Dispose() {
            Dispose(true);
        }

        public T Get() => disposable;
        public bool HasValue => disposable != null && disposable.Any();

        public void Set(T b) {
            DisposeEnumerable();
            disposable = b;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed) {
                return;
            }
           // if (disposing) {
                DisposeEnumerable();
           // }
            disposed = true;
        }
        void DisposeEnumerable() {
            if (HasValue) {
                foreach (var d in disposable) {
                    d?.Dispose();
                }
                disposable = null;
            }
        }
    }


}
