using D3DLab.ECS;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D3DLab.ECS.Sync {
    public class RenderLoopSynchronizationContext : ISynchronizationContext {
        abstract class AbstractQueueItem {
            public int Retries;
            public abstract bool Execute();
        }
        class QueueItem<TOwner, TInput> : AbstractQueueItem {
            public readonly Func<TOwner, TInput, bool> Action;
            public readonly TInput Input;
            public readonly TOwner Owner;
            public readonly Task OperationTask;

            public QueueItem(Func<TOwner, TInput, bool> action, TOwner owner, TInput input) {
                Action = action;
                Input = input;
                Owner = owner;
                Retries = 5;
                OperationTask = new Task(TaskAwaiter);
            }
            void TaskAwaiter() {}
            public override bool Execute() {
                try {
                    Action(Owner, Input);
                    OperationTask.Start();
                    return true;
                } catch (Exception ex) {
                    return false;
                }
            }
        }

        Queue<AbstractQueueItem> queue;
        Queue<AbstractQueueItem> queueSnapshot;

        readonly object _loker;
        int theadId;

        public bool HasChanges {
            get {
                return isChanged;
            }
        }

        bool isChanged;

        public RenderLoopSynchronizationContext() : this(new object()) {
            theadId = -1;
        }
        RenderLoopSynchronizationContext(object _loker) {
            this.queue = new Queue<AbstractQueueItem>();
            this._loker = _loker;
        }

        public void BeginSynchronize() {
            Monitor.Enter(_loker);
            queueSnapshot = new Queue<AbstractQueueItem>(queue);
            queue = new Queue<AbstractQueueItem>();
            isChanged = false;
        }

        public void EndSynchronize(int theadId) {
            this.theadId = theadId;

            var local = queueSnapshot;
            queueSnapshot = null;
            Monitor.Exit(_loker);

            while (local.Any()) {
                var item = local.Dequeue();
                try {
                    if (!item.Execute() && item.Retries > 0) {
                        System.Diagnostics.Trace.WriteLine($"retry, move action to next render iteration, Retries: [{item.Retries}]");
                        item.Retries--;
                        queue.Enqueue(item);
                    }
                } catch (Exception ex) {
                    System.Diagnostics.Trace.WriteLine($"throw queue item because of [{ex.Message}]");
                }
            }
        }


        public Task Add<TOwner, TInput>(Func<TOwner, TInput, bool> action, TOwner owner, TInput input) {
            Task task;
            lock (_loker) {
                isChanged = true;
                var item = new QueueItem<TOwner, TInput>(action, owner, input);
                task = item.OperationTask;
                queue.Enqueue(item);
            }
            return task;
        }
        public void AddRange<TOwner, TInput>(Func<TOwner, TInput, bool> action, TOwner owner, IEnumerable<TInput> inputs) {
            lock (_loker) {
                isChanged = true;
                foreach (var input in inputs) {
                    queue.Enqueue(new QueueItem<TOwner, TInput>(action, owner, input));
                }
            }
        }

        public void Dispose() {
            queue.Clear();
            queueSnapshot?.Clear();
        }

        public void Synchronize(int theadId) {
            BeginSynchronize();
            EndSynchronize(theadId);
        }

    }
}
