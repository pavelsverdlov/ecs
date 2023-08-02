using D3DLab.ECS.Common;
using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace D3DLab.ECS {
   
    public interface IRenderableSurface {
        SurfaceSize Size { get; }

        event Action Resized;
        event Action Invalidated;
    }
    public interface IRenderableWindow : IRenderableSurface {
        bool IsActive { get; }
        IntPtr Handle { get; }
        [Obsolete("Remove from this interface")]
        IInputManager InputManager { get; }

        WaitHandle BeginInvoke(Action action);
    }
}
