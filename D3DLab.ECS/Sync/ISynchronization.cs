using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.ECS.Sync {
    public interface ISynchronization {
        void Synchronize(int theadId);
        Task InvokeAsync<TOwner>(TOwner owner, Action<TOwner> action);
    }
    public interface ISynchronizationContext : ISynchronization {
        bool HasChanges { get; }
        void Dispose();
    }

    public interface ISynchronizationQueue<TOwner, TInput> {
        Task Add(Func<TOwner, TInput, bool> action, TInput input);
        void AddRange(Func<TOwner, TInput, bool> action, IEnumerable<TInput> inputs);
    }
}
