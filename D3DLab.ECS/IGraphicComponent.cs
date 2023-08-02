using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS {
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// DO NOT ADD {set} in this interface because it is used for 'readonly struct'
    /// </remarks>
    public interface IGraphicComponent : IDisposable {
        ElementTag Tag { get; }
        /// <summary>
        /// Important flag, set true if the component is valid
        /// otherwise, it will treat as invalid it means can't be updated correctly
        /// </summary>
        bool IsValid { get; }
    }

    public interface IFlyweightGraphicComponent : IDisposable {
        ElementTag Tag { get; }
        bool IsModified { get; set; }
        bool IsDisposed { get; }
    }
}
