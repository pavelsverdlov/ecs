using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace D3DLab.ECS {

    public interface IComponentManager : IDisposable {
        IComponentManager AddComponent<T>(ElementTag tagEntity, T com) where T : IGraphicComponent;
        void RemoveComponent<T>(ElementTag tagEntity) where T : IGraphicComponent;

        T GetComponent<T>(ElementTag tagEntity) where T : IGraphicComponent;
        IEnumerable<IGraphicComponent> GetComponents(ElementTag tagEntity);
        T GetOrCreateComponent<T>(ElementTag tagEntity, T newone) where T : IGraphicComponent;
        IEnumerable<T> GetComponents<T>() where T : IGraphicComponent;
        IEnumerable<IGraphicComponent> GetComponents(ElementTag tag, params Type[] types);
        Task UpdateComponent<T>(ElementTag tagEntity, T com) where T : IGraphicComponent;
        Task UpdateComponents<T>(ElementTag tagEntity, params T[] newComponents) where T : IGraphicComponent;

        bool TryGetComponent<T>(ElementTag tagEntity, out T component) where T : IGraphicComponent;
       
        bool HasEntityContained<T>(ElementTag tag) where T : IGraphicComponent;
        bool HasEntityContained(ElementTag tag, params Type[] types);
        bool HasEntityOfComponentContained<T>(T com) where T : IGraphicComponent;


        IComponentManager AddComponent<T>(ElementTag tagEntity, T com, out Task awaiter) where T : IGraphicComponent;

        void Dispose();
    }

}
