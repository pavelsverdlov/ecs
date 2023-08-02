using System;
using System.Collections.Generic;

namespace D3DLab.ECS {
    public interface IEntityManager : IDisposable {
        GraphicEntity CreateEntity(ElementTag tag);
        IEnumerable<GraphicEntity> GetEntities();

        GraphicEntity GetEntity(ElementTag tag);
        GraphicEntity GetEntityOf<T>(T com) where T : IGraphicComponent;
        IEnumerable<GraphicEntity> GetEntity(Func<GraphicEntity, bool> predicate);
        bool IsExisted(ElementTag tag);

        void RemoveEntity(ElementTag elementTag);
    }

}
