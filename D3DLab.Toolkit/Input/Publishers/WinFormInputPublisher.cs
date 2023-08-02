using D3DLab.ECS.Input;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace D3DLab.Toolkit.Input.Publishers {

    public class WinFormInputPublisher : BaseInputPublisher {
        private readonly Control control;


        public WinFormInputPublisher(Control control) {
            this.control = control;

            this.control.MouseDown += OnMouseDown;
            this.control.MouseUp += OnMouseUp;
            this.control.MouseMove += OnMouseMove;
            this.control.MouseWheel += OnMouseWheel;
            this.control.MouseDoubleClick += OnMouseDoubleClick;
            this.control.MouseLeave += OnMouseLeave;

            this.control.KeyDown += OnKeyDown;
            this.control.KeyUp += OnKeyUp;
            this.control.Leave += OnLeave;
            this.control.GotFocus += control_GotFocus;
            this.control.LostFocus += control_LostFocus;

        }

        /*
        Mouse events occur in the following order:
            MouseEnter
            MouseMove
            MouseHover / MouseDown / MouseWheel
            MouseUp
            MouseLeave
        */

        #region com

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool ReleaseCapture();

        #endregion

        #region event handlers


        private void OnMouseLeave(object sender, System.EventArgs e) {

        }

        private void control_LostFocus(object sender, System.EventArgs e) {

        }

        private void control_GotFocus(object sender, System.EventArgs e) {

        }

        private void OnLeave(object sender, System.EventArgs e) {

        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {

        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e) {
            var btn = GetMouseButton(e.Button);
            state.Buttons |= btn;

            var bs = new ButtonsState {
                CursorPoint = Cursor.Position.ToWindowPoint(),
                PointV2 = control.PointToClient(Cursor.Position).ToNumericsV2(),
                Condition = ButtonStates.Pressed,
            };

            state.ButtonsStates[btn] = bs;
            InvokeSubscribers((s, ev) => s.OnMouseDoubleDown(ev));
        }

        private void OnMouseWheel(object sender, MouseEventArgs e) {
            var p = e.Location;
            state.CursorCurrentPosition = p.ToWindowPoint();
            state.CurrentPosition = control.PointToClient(p).ToNumericsV2();
            state.Delta = e.Delta;
            InvokeSubscribers((s, ev) => s.OnMouseWheel(ev));
        }

        private void OnMouseUp(object sender, MouseEventArgs e) {
            var btn = GetMouseButton(e.Button);
            //state.Buttons ^= btn;
            state.Buttons &= ~btn;
            var bs = new ButtonsState {
                CursorPoint = new WindowPoint(),
                PointV2 = Vector2.Zero,
                Condition = ButtonStates.Released
            };
            state.ButtonsStates[btn] = bs;

            if (btn == GeneralMouseButtons.None) {
                ReleaseCapture();
            }

            InvokeSubscribers((s, ev) => s.OnMouseUp(ev));
        }
        private void OnMouseDown(object sender, MouseEventArgs e) {
            var btn = GetMouseButton(e.Button);
            state.Buttons |= btn;

            var bs = new ButtonsState {
                CursorPoint = Cursor.Position.ToWindowPoint(),
                PointV2 = control.PointToClient(Cursor.Position).ToNumericsV2(),
                Condition = ButtonStates.Pressed,
            };

            state.ButtonsStates[btn] = bs;
            InvokeSubscribers((s, ev) => s.OnMouseDown(ev));
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            var btn = GeneralMouseButtons.None;
            if (Control.MouseButtons.HasFlag(MouseButtons.Left)) {
                btn |=  GeneralMouseButtons.Left;
            }
            if (Control.MouseButtons.HasFlag(MouseButtons.Right)) {
                btn |= GeneralMouseButtons.Right;
            }
            if (Control.MouseButtons.HasFlag(MouseButtons.Middle)) {
                btn |= GeneralMouseButtons.Middle;
            }
            state.Buttons = btn;
            state.CursorCurrentPosition = Cursor.Position.ToWindowPoint();
            state.PrevPosition = state.CurrentPosition;
            state.CurrentPosition = control.PointToClient(Cursor.Position).ToNumericsV2();
            InvokeSubscribers((s, ev) => s.OnMouseMove(ev));
        }

        protected static GeneralMouseButtons GetMouseButton(MouseButtons state) {
            switch (state) {
                case MouseButtons.Left:
                    return GeneralMouseButtons.Left;
                case MouseButtons.Right:
                    return GeneralMouseButtons.Right;
                case MouseButtons.Middle:
                    return GeneralMouseButtons.Middle;
            }
            return GeneralMouseButtons.None;
        }

        #endregion

        public override void Dispose() {
            this.control.MouseDown -= OnMouseDown;
            this.control.MouseUp -= OnMouseUp;
            this.control.MouseMove -= OnMouseMove;
            this.control.MouseWheel -= OnMouseWheel;
            this.control.MouseDoubleClick -= OnMouseDoubleClick;
            this.control.KeyDown -= OnKeyDown;
            this.control.KeyUp -= OnKeyUp;
            this.control.Leave -= OnLeave;
            this.control.GotFocus -= control_GotFocus;
            this.control.LostFocus -= control_LostFocus;
            base.Dispose();
        }
    }
}
