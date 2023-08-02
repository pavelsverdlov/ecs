using System;
using System.Collections.Generic;
using System.Text;

using D3DLab.ECS.Input;

namespace D3DLab.Toolkit.Input {
    public class EmptyCameraInputHandler : DefaultInputObserver.ICameraInputHandler {
        public static readonly EmptyCameraInputHandler Instance = new EmptyCameraInputHandler();

        private EmptyCameraInputHandler() { }

        public void ChangeRotateCenter(InputStateData state) {
        }

        public void ChangeTransparencyOnObjectUnderCursor(InputStateData state, bool isMmbHolded2sec) {
        }

        public void FocusToObject(InputStateData state) {
        }

        public void HideOrShowObjectUnderCursor(InputStateData state) {
        }
        public void Idle() {
        }
        public void KeywordMove(InputStateData state) {
        }
        public void Pan(InputStateData state) {
        }
        public bool Rotate(InputStateData state) {
            return true;
        }
        public void Zoom(InputStateData state) {
        }
    }
}
