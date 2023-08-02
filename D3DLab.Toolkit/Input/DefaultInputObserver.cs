using D3DLab.ECS.Input;
using D3DLab.Toolkit.Input.Commands;
using D3DLab.Toolkit.Input.Commands.Camera;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace D3DLab.Toolkit.Input {

    public enum AllInputStates {
        Idle = 0,
        Rotate = 1,
        Pan = 2,
        Zoom = 3,
        Target = 4,
        //UnTarget = 5,
        KeywordDown = 6,
      //  ChangeFocus = 7,
        ChangeRotateCenter = 8,

    }
  
    public abstract class DefaultInputObserver : InputObserver,
        DefaultInputObserver.ICameraInputHandler, DefaultInputObserver.ITargetingInputHandler {

        public interface ICameraInputHandler : InputObserver.IHandler {
            bool Rotate(InputStateData state);
            void Zoom(InputStateData state);
            void Pan(InputStateData state);
            void Idle();
            void KeywordMove(InputStateData state);
            void FocusToObject(InputStateData state);
            void ChangeRotateCenter(InputStateData state);
        }

        public interface ITargetingInputHandler : InputObserver.IHandler {
            void TargetCapture(InputStateData state);
            void TargetMove(InputStateData state);
            void UnTarget(InputStateData state);
        }

        protected sealed class InputIdleState : CurrentStateMachine {
            public InputIdleState(StateProcessor processor) : base(processor) {
                
            }

            public override void EnterState(InputStateData state) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Idle());

                switch (state.Buttons) {
                    case GeneralMouseButtons.Right:
                        SwitchTo((int)AllInputStates.Rotate, state);
                        break;
                    default:
                        
                        break;
                }                
            }

            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    //camera
                    case GeneralMouseButtons.Left | GeneralMouseButtons.Right:
                        SwitchTo((int)AllInputStates.Pan, state);
                        break;
                    case GeneralMouseButtons.Right:
                        SwitchTo((int)AllInputStates.Rotate, state);
                        break;
                    case GeneralMouseButtons.Middle:
                        break;
                    //manipulation
                    case GeneralMouseButtons.Left:
                        SwitchTo((int)AllInputStates.Target, state);
                        break;
                }
                return base.OnMouseDown(state);
            }

            public override bool OnMouseWheel(InputStateData state) {
                SwitchTo((int)AllInputStates.Zoom, state);
                return base.OnMouseWheel(state);
            }

            public override bool KeyDown(InputStateData state) {
                SwitchTo((int)AllInputStates.KeywordDown, state);
                return true;
            }

            //public override bool OnMouseDoubleDown(InputStateData state) {
            //    switch (state.Buttons) {
            //        case GeneralMouseButtons.Left:
            //            SwitchTo((int)AllInputStates.ChangeRotateCenter, state);
            //            break;
            //    }
            //    return base.OnMouseDoubleDown(state);
            //}
        }

        #region Camera

        protected sealed class InputRotateStateWithCursorReturning : CurrentStateMachine {
            public InputRotateStateWithCursorReturning(StateProcessor processor) : base(processor) {
               // System.Windows.Forms.Cursor.Hide();
            }
            public override void EnterState(InputStateData state) {
                //Processor.InvokeHandler<ICameraInputHandler>(x => x.Rotate(state));
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Right) != GeneralMouseButtons.Right) {
                    SwitchTo((int)AllInputStates.Idle, state);
                 //   System.Windows.Forms.Cursor.Show();
                }
                return base.OnMouseUp(state);
            }
            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Left | GeneralMouseButtons.Right:
                       SwitchTo((int)AllInputStates.Pan, state);
                    //   System.Windows.Forms.Cursor.Show();
                    break;
                }
                return base.OnMouseDown(state);
            }
            public override bool OnMouseMove(InputStateData state) {
                if(state.Buttons == GeneralMouseButtons.None) {
                    SwitchTo((int)AllInputStates.Idle, state);
                    return base.OnMouseMove(state);
                }
                //System.Windows.Forms.Cursor.Position = state.ButtonsStates[GeneralMouseButtons.Right].CursorPoint.ToDrawingPoint();
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Rotate(state));
                //return cursore to prev position ... allow to calculate delta from static position to new move
                //static postion is first positoin of rotation
                Cursor.Position = state.ButtonsStates[GeneralMouseButtons.Right].CursorPoint.ToDrawingPoint();
                return true;
            }
            //public override bool OnMouseDoubleDown(InputStateData state) {
            //    SwitchTo((int)AllInputStates.ChangeFocus, state);
            //    return base.OnMouseDoubleDown(state);
            //}
        }

        protected sealed class InputPanState : CurrentStateMachine {
            public InputPanState(StateProcessor processor) : base(processor) {
                //System.Windows.Forms.Cursor.Hide();
            }

            public override void LeaveState(InputStateData inputStateDate) {
                var right = inputStateDate.ButtonsStates[GeneralMouseButtons.Right];
                right.PointV2 = inputStateDate.CurrentPosition;
                right.CursorPoint = inputStateDate.CursorCurrentPosition;
                inputStateDate.ButtonsStates[GeneralMouseButtons.Right] = right;
                base.LeaveState(inputStateDate);
            }

            public override bool OnMouseUp(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Right:
                        SwitchTo((int)AllInputStates.Rotate, state);
                        break;
                    default:
                    SwitchTo((int)AllInputStates.Idle, state);
                        break;
                }

                return base.OnMouseUp(state);
            }

            public override bool OnMouseMove(InputStateData state) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Pan(state));
                return false;
            }
        }

        protected sealed class InputChangeRotateCenterState : CurrentStateMachine {
            public InputChangeRotateCenterState(StateProcessor processor) : base(processor) { }

            public override void EnterState(InputStateData state) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.ChangeRotateCenter(state));
            }

            public override bool OnMouseUp(InputStateData state) {
                SwitchTo((int)AllInputStates.Idle, state);
                return base.OnMouseUp(state);
            }
        }

        protected sealed class InputZoomState : CurrentStateMachine {
            public InputZoomState(StateProcessor processor) : base(processor) { }

            public override void EnterState(InputStateData ev) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Zoom(ev));
            }

            public override bool OnMouseDown(InputStateData state) {
                SwitchTo((int)AllInputStates.Idle, state);
                return base.KeyDown(state);
            }

            public override bool OnMouseWheel(InputStateData ev) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Zoom(ev));
                return true;
            }

            //public override bool OnMouseMove(InputStateData state) {
            //    SwitchTo((int)AllInputStates.Idle, state);
            //    return false;
            //}
        }

        #endregion

        #region moving

        protected class KeywordMovingState : CurrentStateMachine {
            public KeywordMovingState(StateProcessor processor) : base(processor) { }

            public override void EnterState(InputStateData state) {
                state.IsKeywordDown = true;
                Processor.InvokeHandler<ICameraInputHandler>(x => x.KeywordMove(state));
            }

            public override bool OnMouseMove(InputStateData state) {
                state.IsKeywordDown = false;
                SwitchTo((int)AllInputStates.Idle, state);

                return base.OnMouseMove(state);
            }

            public override bool KeyUp(InputStateData state) {
                state.IsKeywordDown = false;
                switch (state.Keyword) {
                    case GeneralKeywords.W:
                    case GeneralKeywords.S:
                    case GeneralKeywords.A:
                    case GeneralKeywords.D:
                        Processor.InvokeHandler<ICameraInputHandler>(x => x.KeywordMove(state));
                        return true;
                    default:
                        SwitchTo((int)AllInputStates.Idle, state);
                        return false;

                }
            }

            public override bool KeyDown(InputStateData state) {
                state.IsKeywordDown = true;
                switch (state.Keyword) {
                    case GeneralKeywords.W:
                        Processor.InvokeHandler<ICameraInputHandler>(x => x.KeywordMove(state));
                        return true;
                    default:
                        SwitchTo((int)AllInputStates.Idle, state);
                        return false;

                }
            }
        }

        protected class FocusToObjectState : CurrentStateMachine {
            public FocusToObjectState(StateProcessor processor) : base(processor) { }

            public override void EnterState(InputStateData state) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.FocusToObject(state));
                SwitchTo((int)AllInputStates.Idle, state);
            }
        }

        #endregion


        protected sealed class InputTargetState : CurrentStateMachine {
            public InputTargetState(StateProcessor processor) : base(processor) {

            }
            bool captured;
            InputStateData enterState;
            public override void EnterState(InputStateData state) {
                enterState = state.Clone();
            }

            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Left:
                        captured = true;
                        Processor.InvokeHandler<ITargetingInputHandler>(x => x.TargetCapture(state));
                        break;
                    case GeneralMouseButtons.Left | GeneralMouseButtons.Right:
                        UnCaptureTarget(state);
                        //SwitchTo((int)AllInputStates.Idle, state);
                        SwitchTo((int)AllInputStates.Pan, state);
                        break;
                }
                return base.OnMouseDown(state);
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Left) != GeneralMouseButtons.Left) {
                    UnCaptureTarget(state);
                    //select will be pushed always ... to handling should be not here 
                    //here just pushing a commands 
                    //Processor.InvokeHandler<ITargetingInputHandler>(x => x.SelectTarget(enterState));
                    SwitchTo((int)AllInputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }
            public override bool OnMouseMove(InputStateData state) {
                Processor.InvokeHandler<ITargetingInputHandler>(x => x.TargetMove(state));
                return true;
            }
            public override bool OnMouseDoubleDown(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Left:
                        SwitchTo((int)AllInputStates.ChangeRotateCenter, state);
                        break;
                }
                return base.OnMouseDoubleDown(state);
            }

            void UnCaptureTarget(InputStateData state) {
                if (captured) {
                    captured = false;
                    //untarget if it was captured
                    Processor.InvokeHandler<ITargetingInputHandler>(x => x.UnTarget(state));
                }
            }
        }



        protected abstract class CurrentStateMachine : InputStateMachine {
            protected CurrentStateMachine(StateProcessor processor) : base(processor) { }
        }


        protected float RotationSensitivity;

        public DefaultInputObserver(IInputPublisher publisher) : base(publisher) {
            this.currentSnapshot = new InputSnapshot();

            RotationSensitivity = 0.7f;
        }
        public void Zoom(InputStateData state) {
            currentSnapshot.AddEvent(new CameraZoomCommand(state.Clone()));
        }

        public virtual bool Rotate(InputStateData state) {
            currentSnapshot.AddEvent(new CameraRotateWithCursorReturntingCommand(state.Clone(), RotationSensitivity));
            return true;
        }
        public void Pan(InputStateData state) {
            currentSnapshot.AddEvent(new CameraPanCommand(state.Clone()));
        }
        public void FocusToObject(InputStateData state) {
            //currentSnapshot.AddEvent(new FocusToObjectCommand(state.Clone()));
        }

        public void Idle() {
            currentSnapshot.AddEvent(new InputIdleCommand());
        }

        public void KeywordMove(InputStateData state) {
            //currentSnapshot.AddEvent(new KeywordsMovingCommand(state.Clone()));
        }


        public void TargetCapture(InputStateData state) {
            currentSnapshot.AddEvent(new CaptureTargetUnderLeftMouseCommand(state.Clone()));
        }
        public void TargetMove(InputStateData state) {

        }
        public void UnTarget(InputStateData state) {
            //currentSnapshot.AddEvent(new CaptureTargetUnderMouseCameraCommand(state.Clone()));
        }
        public virtual void ChangeRotateCenter(InputStateData state) {
            currentSnapshot.AddEvent(new CameraSetRotationCenterUnderMouseCommand(state.Clone()));
        }

    }
}
