using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace D3DLab.ECS {
   
    public static class SynchronizationContextBuilder {
        public static ISynchronizationQueue<TOwner, TInput> Create<TOwner, TInput>(TOwner owner, RenderLoopSynchronizationContext context) {
            return new SynchronizationContextAdapter<TOwner, TInput>(owner, context);
        }
    }

    public class SynchronizationContext<TOwner> : ISynchronization {
        abstract class AbstractQueueItem {
            public int Retries;
            public abstract bool Execute(TOwner owner);
        }
        class QueueItem<TInput> : AbstractQueueItem {
            public readonly Func<TOwner, TInput, bool> Action;
            public readonly TInput Input;

            public QueueItem(Func<TOwner, TInput, bool> action, TInput input) {
                Action = action;
                Input = input;
                Retries = 5;
            }

            public override bool Execute(TOwner owner) => Action(owner, Input);
        }

        Queue<AbstractQueueItem> queue;
        Queue<AbstractQueueItem> queueSnapshot;

        readonly TOwner owner;
        readonly object _loker;
        int theadId;

        public bool IsChanged { get; private set; }

        public SynchronizationContext(TOwner owner) : this(owner, new object()) {
            theadId = -1;
        }
        SynchronizationContext(TOwner owner, object _loker) {
            this.queue = new Queue<AbstractQueueItem>();
            this.owner = owner;
            this._loker = _loker;
        }

        public void BeginSynchronize() {
            Monitor.Enter(_loker);
            queueSnapshot = new Queue<AbstractQueueItem>(queue);
            queue = new Queue<AbstractQueueItem>();
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
                    if (!item.Execute(owner) && item.Retries > 0) {
                        item.Retries--;
                        queue.Enqueue(item);
                    }
                } catch (Exception ex) {
                    System.Diagnostics.Trace.WriteLine($"retry, move action to next render iteration [{ex.Message}] Retries:{item.Retries}");
                }
            }
        }

        public void Synchronize(int theadId) {
            BeginSynchronize();
            EndSynchronize(theadId);
        }

        public void Add<TInput>(Func<TOwner, TInput, bool> action, TInput input) {
            lock (_loker) {
                IsChanged = true;
                queue.Enqueue(new QueueItem<TInput>(action, input));
            }
        }
        public void AddRange<TInput>(Func<TOwner, TInput, bool> action, IEnumerable<TInput> inputs) {
            lock (_loker) {
                IsChanged = true;
                foreach (var input in inputs) {
                    queue.Enqueue(new QueueItem<TInput>(action, input));
                }
            }
        }

        public void Dispose() {
            queue.Clear();
            queueSnapshot?.Clear();
        }
    }
}
