using D3DLab.ECS;
using D3DLab.ECS.Input;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Input.Commands {
    abstract class CaptureTargetUnderMouseCommand : IInputCommand {
        public InputStateData InputState { get; }
        readonly GeneralMouseButtons type;
        public CaptureTargetUnderMouseCommand(InputStateData inputState, GeneralMouseButtons type) {
            InputState = inputState;
            this.type = type;
        }

        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var p1 = InputState.ButtonsStates[type].PointV2;

            var manager = context.GetEntityManager();
            var world = manager.GetEntity(snapshot.WorldTag);

            world.UpdateComponent(CaptureTargetUnderMouseComponent.Create(p1));

            return true;
        }
    }

    class CaptureTargetUnderMiddleMouseCommand : CaptureTargetUnderMouseCommand {
        public CaptureTargetUnderMiddleMouseCommand(InputStateData inputState)
            : base(inputState, GeneralMouseButtons.Middle){ }
    }
    class CaptureTargetUnderLeftMouseCommand : CaptureTargetUnderMouseCommand {
        public CaptureTargetUnderLeftMouseCommand(InputStateData inputState)
            : base(inputState, GeneralMouseButtons.Left) { }
    }
}
