using D3DLab.ECS.Input;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;


namespace D3DLab.Toolkit.Input.Publishers {
    public class WPFInputPublisher : BaseInputPublisher {
        readonly System.Windows.FrameworkElement control;
        readonly DispatcherTimer clickTimer;
        System.Windows.Input.MouseButtonEventArgs mouseDown;

        public WPFInputPublisher(System.Windows.FrameworkElement control) {
            this.control = control;
            clickTimer = new DispatcherTimer() {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            clickTimer.Tick += EvaluateClicks;

            this.control.MouseDown += OnMouseDown;
            this.control.MouseUp += OnMouseUp;
            this.control.MouseMove += OnMouseMove;
            this.control.MouseWheel += OnMouseWheel;
            //this.control.MouseDoubleClick += OnMouseDoubleClick;
            this.control.MouseLeave += OnMouseLeave;

            //    this.control.Leave += OnLeave;
            this.control.GotFocus += control_GotFocus;
            this.control.LostFocus += control_LostFocus;

            //keywords

            this.control.KeyDown += OnKeyDown;
            this.control.KeyUp += OnKeyUp;

        }

        /*
        Mouse events occur in the following order:
            MouseEnter
            MouseMove
            MouseHover / MouseDown / MouseWheel
            MouseUp
            MouseLeave
        */

        #region event handlers


        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var btn = Control.MouseButtons;
            //state.Buttons = state.Buttons | btn;
        }

        private void control_LostFocus(object sender, System.Windows.RoutedEventArgs e) {

        }

        private void control_GotFocus(object sender, System.Windows.RoutedEventArgs e) {

        }

        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            state.Keyword = Convert(e.Key);
            InvokeSubscribers((s, ev) => s.KeyUp(ev));
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            state.Keyword = Convert(e.Key);
            InvokeSubscribers((s, ev) => s.KeyDown(ev));
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            var point = e.GetPosition(control);
            var cp = Cursor.Position;
            state.CursorCurrentPosition = point.ToWindowPoint();
            state.CurrentPosition = point.ToNumericsV2();
            state.Delta = e.Delta;
            InvokeSubscribers((s, ev) => s.OnMouseWheel(ev));
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var btn = GetMouseButton(e.ChangedButton);

            // Additional check "state.Buttons!= GeneralMouseButtons.None" need for specific case:
            // Minimize window then drag&move
            // If TopPanel implemented as separate control, after drag&move we receive only keyUp.
            // So this check is used to prevent situation when keyUp calls without KeyUp.
            if (state.Buttons!= GeneralMouseButtons.None) {
                //state.Buttons ^= btn;
                state.Buttons &= ~btn;
            }
            state.ButtonsStates[btn] = new ButtonsState {
                CursorPoint = new WindowPoint(),
                PointV2 = System.Numerics.Vector2.Zero,
                Condition = ButtonStates.Released
            };
            //System.Windows.Input.Mouse.LeftButton.
            //if (state.Buttons == MouseButtons.None) {
            //    ReleaseCapture();
            //}

            InvokeSubscribers((s, ev) => s.OnMouseUp(ev));
        }

        void EvaluateClicks(object source, EventArgs e) {
            clickTimer.Stop();

            var btn = GetMouseButton(mouseDown.ChangedButton);
            state.Buttons |= btn;// btn;
            state.ButtonsStates[btn] = new ButtonsState {
                CursorPoint = Cursor.Position.ToWindowPoint(),
                PointV2 = mouseDown.GetPosition(control).ToNumericsV2(),
                Condition = ButtonStates.Pressed
            };
            InvokeSubscribers((s, ev) => s.OnMouseDown(ev));
        }
        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var count = e.ClickCount;
            
            //if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) {
                mouseDown = e;
                EvaluateClicks(sender, e);
                return;
            //}
            //if (count > 1) { // double click
            //    clickTimer.Stop();

            //    var btn = GetMouseButton(e.ChangedButton);
            //    log.Info($"{btn} dbl down");
            //    state.Buttons = btn;//|= btn;
            //    log.Info($"Buttons state : {state.Buttons}");
            //    state.ButtonsStates[btn] = new ButtonsState {
            //        CursorPoint = Cursor.Position.ToWindowPoint(),
            //        PointV2 = e.GetPosition(control).ToNumericsV2(),
            //        Condition = ButtonStates.Pressed,
            //    };

            //    state.CursorCurrentPosition = Cursor.Position.ToWindowPoint();
            //    state.CurrentPosition = e.GetPosition(control).ToNumericsV2();

            //    InvokeSubscribers((s, ev) => s.OnMouseDoubleDown(ev));
            //    return;
            //}
            //mouseDown = e;
            //clickTimer.Start();
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (clickTimer.IsEnabled) {
                EvaluateClicks(sender, e);
                return;
            }
            var point = e.GetPosition(control);
            state.CursorCurrentPosition = Cursor.Position.ToWindowPoint();
            state.CurrentPosition = e.GetPosition(control).ToNumericsV2();
            InvokeSubscribers((s, ev) => s.OnMouseMove(ev));
            state.PrevPosition = state.CurrentPosition;
            //System.Diagnostics.Trace.WriteLine($"OnMouseMove {state.CurrentPosition}");
        }

        protected static GeneralMouseButtons GetMouseButton(System.Windows.Input.MouseButton state) {
            switch (state) {
                case System.Windows.Input.MouseButton.Left:
                    return GeneralMouseButtons.Left;
                case System.Windows.Input.MouseButton.Right:
                    return GeneralMouseButtons.Right;
                case System.Windows.Input.MouseButton.Middle:
                    return GeneralMouseButtons.Middle;
            }
            return GeneralMouseButtons.None;
        }

        static GeneralKeywords Convert(System.Windows.Input.Key key) {
            switch (key) {
                case System.Windows.Input.Key.W:
                    return GeneralKeywords.W;
                case System.Windows.Input.Key.S:
                    return GeneralKeywords.S;
                case System.Windows.Input.Key.A:
                    return GeneralKeywords.A;
                case System.Windows.Input.Key.D:
                    return GeneralKeywords.D;
                default:
                    return GeneralKeywords.None;
            }
        }

        #endregion

        public override void Dispose() {
            this.control.MouseDown -= OnMouseDown;
            this.control.MouseUp -= OnMouseUp;
            this.control.MouseMove -= OnMouseMove;
            this.control.MouseWheel -= OnMouseWheel;

            this.control.KeyDown -= OnKeyDown;
            this.control.KeyUp -= OnKeyUp;

            this.control.GotFocus -= control_GotFocus;
            this.control.LostFocus -= control_LostFocus;
        }
    }
}
