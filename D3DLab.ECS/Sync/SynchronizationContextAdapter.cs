using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.ECS.Sync {
    public class SynchronizationContextAdapter<TOwner, TInput> : ISynchronizationQueue<TOwner, TInput> {

        readonly RenderLoopSynchronizationContext context;
        readonly TOwner owner;
        public SynchronizationContextAdapter(TOwner owner, RenderLoopSynchronizationContext context) {
            this.owner = owner;
            this.context = context;
        }

        public bool IsChanged { get; }

        public Task Add(Func<TOwner, TInput,bool> action, TInput input) {
            return context.Add(action, owner, input);
        }

        public void AddRange(Func<TOwner, TInput, bool> action, IEnumerable<TInput> inputs) {
            context.AddRange(action, owner, inputs);
        }
    }
    public class SynchronizationContextAdapter<TOwner>  {

        readonly RenderLoopSynchronizationContext context;
        readonly TOwner owner;
        public SynchronizationContextAdapter(TOwner owner, RenderLoopSynchronizationContext context) {
            this.owner = owner;
            this.context = context;
        }

        public bool IsChanged { get; }

        public Task Add<TInput>(Func<TOwner, TInput, bool> action, TInput input) {
            return context.Add(action, owner, input);
        }

        public void AddRange<TInput>(Func<TOwner, TInput, bool> action, IEnumerable<TInput> inputs) {
            context.AddRange(action, owner, inputs);
        }
    }
    //public class SynchronizationContextAdapter<TOwner, TInput> : ISynchronizationQueue<TOwner, TInput> {

    //    readonly RenderLoopSynchronizationContext context;
    //    readonly TOwner owner;
    //    internal SynchronizationContextAdapter(TOwner owner, RenderLoopSynchronizationContext context) {
    //        this.owner = owner;
    //        this.context = context;
    //    }

    //    public bool IsChanged { get; }

    //    public void Add(Func<TOwner, TInput, bool> action, TInput input) {
    //        context.Add(action, owner, input);
    //    }

    //    public void AddRange(Func<TOwner, TInput, bool> action, IEnumerable<TInput> inputs) {
    //        context.AddRange(action, owner, inputs);
    //    }
    //}
}
