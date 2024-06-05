using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.ECS.Input {  

    public sealed class InputManager : IInputManager  {
        readonly InputObserver observer;
        readonly SynchronizationContext<InputManager, IInputCommand> synchronization;

        public InputManager(InputObserver observer) {
            this.observer = observer;
            synchronization = new SynchronizationContext<InputManager, IInputCommand>(this);
        }
        public void Dispose() {
            observer.Dispose();
            synchronization.Dispose();
        }

        public InputSnapshot GetInputSnapshot() {
            return observer.GetInputSnapshot();
        }

        public void PushCommand(IInputCommand cmd) {
            synchronization.Add((own, input) => own.observer.PushCommand(input), cmd);
        }

        public void Synchronize(int theadId) {
            synchronization.Synchronize(theadId);
        }

        public Task InvokeAsync<TOwner>(TOwner owner, Action<TOwner> action) {
            throw new NotImplementedException();
            //return context.Add((owner, obj) => {
            //    action();
            //    return true;
            //}, owner, new object());
        }
    }
}
