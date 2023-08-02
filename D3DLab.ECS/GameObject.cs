using System.Collections.Generic;
using System.Linq;

namespace D3DLab.ECS {
    public abstract class GameObject {
        public string Description { get; }

        protected GameObject(string desc) {
            Description = desc;
        }

        public abstract void Hide(IContextState context);
        public abstract void Show(IContextState context);
        public abstract void Cleanup(IContextState context);

        public abstract void AddComponent<TComponent>(IContextState context, TComponent component)
            where TComponent : IGraphicComponent;


    }
}
