using D3DLab.ECS;

using System;

namespace D3DLab.Plugin {
    public readonly struct PluginVisibilityComponent : IGraphicComponent {
        public static PluginVisibilityComponent Create(bool visible) {
            return new PluginVisibilityComponent(visible);
        }

        public ElementTag Tag { get; }
        public bool IsValid { get; }
        public bool IsVisible { get; }

        PluginVisibilityComponent(bool visible) : this() {
            IsValid = true;
            Tag = ElementTag.New();
            IsVisible = visible;
        }

        public void Dispose() { }
    }
}
