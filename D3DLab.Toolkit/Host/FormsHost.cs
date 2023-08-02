using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;

namespace D3DLab.Toolkit.Host {
    public class FormsHost : WindowsFormsHost {
        
        public static readonly DependencyProperty OverlayProperty =
          DependencyProperty.Register(nameof(OverlayProperty), typeof(FrameworkElement), typeof(FormsHost));

        public static readonly DependencyProperty HostLoadedProperty =
           DependencyProperty.Register(nameof(HostLoaded), typeof(ICommand), typeof(FormsHost));
        public static readonly DependencyProperty HostUnLoadedProperty =
           DependencyProperty.Register(nameof(HostUnLoaded), typeof(ICommand), typeof(FormsHost));

        public ICommand HostLoaded {
            get { return (ICommand)this.GetValue(HostLoadedProperty); }
            set { this.SetValue(HostLoadedProperty, value); }
        }
        public ICommand HostUnLoaded {
            get { return (ICommand)this.GetValue(HostUnLoadedProperty); }
            set { this.SetValue(HostUnLoadedProperty, value); }
        }

        public FrameworkElement Overlay {
            get { return (FrameworkElement)this.GetValue(OverlayProperty); }
            set { this.SetValue(OverlayProperty, value); }
        }


        public event Action<WinFormsD3DControl> HandleCreated = x => { };
        public FormsHost() {
            this.Loaded += OnLoaded;
            this.Unloaded += Unloded;
        }

        void OnLoaded(object sender, RoutedEventArgs e) {
            var control = new WinFormsD3DControl();
            HostLoaded?.Execute(sender);
            control.HandleCreated += OnHandleCreated;
            Child = control;
        }

        void OnHandleCreated(object sender, EventArgs e) {
            HandleCreated((WinFormsD3DControl)sender);
        }

        void Unloded(object sender, RoutedEventArgs e) {
            HostUnLoaded?.Execute(sender);
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
        }
    }
}
