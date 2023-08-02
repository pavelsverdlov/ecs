using D3DLab.ECS;
using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Input.Commands {
    /// <summary>
    /// For pushing render frame if no input
    /// </summary>
    public class ForceRenderCommand : IInputCommand {
        public InputStateData InputState { get; }

        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            return true;
        }
    }
}
