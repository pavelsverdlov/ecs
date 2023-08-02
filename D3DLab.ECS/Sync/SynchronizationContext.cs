using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace D3DLab.ECS.Sync {
    public class SynchronizationContext<TOwner, TInput> {
        class QueueItem {
            public readonly Action<TOwner, TInput> Action;
            public readonly TInput Input;
            public int Retries;

            public QueueItem(Action<TOwner, TInput> action, TInput input) {
                Action = action;
                Input = input;
                Retries = 5;
            }
        }

        Queue<QueueItem> queue;
        Queue<QueueItem> queueSnapshot;
        readonly TOwner owner;
        readonly object _loker;
        int theadId;

        public bool IsChanged { get; private set; }

        public SynchronizationContext(TOwner owner) : this(owner, new object()) {
            theadId = -1;
        }
        SynchronizationContext(TOwner owner, object _loker) {
            this.queue = new Queue<QueueItem>();
            this.owner = owner;
            this._loker = _loker;
        }

        public void BeginSynchronize() {
            Monitor.Enter(_loker);
            queueSnapshot = new Queue<QueueItem>(queue);
            queue = new Queue<QueueItem>();
            IsChanged = false;
        }

        public void EndSynchronize(int theadId) {
            this.theadId = theadId;

            var local = queueSnapshot;
            queueSnapshot = null;
            Monitor.Exit(_loker);

            while (local.Any()) {
                var item = local.Dequeue();
                try {
                    item.Action(owner, item.Input);
                } catch (Exception ex) {
                    System.Diagnostics.Trace.WriteLine($"retry, move action to next render iteration [{ex.Message}] Retries:{item.Retries}");
                    item.Retries--;
                    if (item.Retries > 0) {
                        Add(item.Action, item.Input);
                    }
                }
            }
        }

        public void Synchronize(int theadId) {
            BeginSynchronize();
            EndSynchronize(theadId);
        }

        public void Add(Action<TOwner, TInput> action, TInput input) {
            //if(Thread.CurrentThread.ManagedThreadId == theadId) {
            //    action(owner, input);
            //    return;
            //}
            lock (_loker) {
                IsChanged = true;
                queue.Enqueue(new QueueItem(action, input));
            }
        }
        public void AddRange(Action<TOwner, TInput> action, IEnumerable<TInput> inputs) {
            //if (Thread.CurrentThread.ManagedThreadId == theadId) {
            //    foreach (var input in inputs) {
            //        action(owner, input);
            //    }
            //    return;
            //}
            lock (_loker) {
                IsChanged = true;
                foreach (var input in inputs) {
                    queue.Enqueue(new QueueItem(action, input));
                }
            }
        }

        public void Dispose() {
            queue.Clear();
            queueSnapshot?.Clear();
        }
    }
}
