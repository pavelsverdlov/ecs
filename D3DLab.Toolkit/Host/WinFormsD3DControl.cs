using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Toolkit.Host {
    public sealed class WinFormsD3DControl : UserControl {
        // public event Action HandleCreated = () => { };
        public WinFormsD3DControl() {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);
        }

        protected override void CreateHandle() {
            base.CreateHandle();
        }

        protected override void DestroyHandle() {

            base.DestroyHandle();
        }

        protected override void OnPaint(PaintEventArgs e) {
            RaisePaintEvent(this, e);
        }
    }
}
