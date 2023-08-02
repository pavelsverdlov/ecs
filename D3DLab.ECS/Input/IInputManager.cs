using D3DLab.ECS;
using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Input {
    public interface IInputManager : ISynchronization {
        void PushCommand(IInputCommand cmd);
        InputSnapshot GetInputSnapshot();
        void Dispose();
    }
}
