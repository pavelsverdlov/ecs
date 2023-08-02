using D3DLab.ECS.Camera;
using D3DLab.SDX.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit {
    public interface IToolkitFrameProperties : IRenderProperties {

        //TODO: chamge to disposable setter
        SharpDX.Direct3D11.Buffer Game { get; }
        SharpDX.Direct3D11.Buffer Lights { get; }
        
    }
}
