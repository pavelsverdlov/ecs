using D3DLab.ECS;
using D3DLab.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D3DLab.ECS {

    public readonly struct GraphicEntity  {

        struct EmptyManager : IComponentManager, IEntityManager {
            public bool HasChanges { get; }
            public IComponentManager AddComponent<T>(ElementTag tagEntity, T com) where T : IGraphicComponent => this;

            public IComponentManager AddComponent<T>(ElementTag tagEntity, T com, out Task awaiter) where T : IGraphicComponent {
                throw new NotImplementedException();
            }

            public GraphicEntity CreateEntity(ElementTag tag) => GraphicEntity.Empty();
            public void Dispose() {}
            public void FrameSynchronize(int theadId) {}
            public T GetComponent<T>(ElementTag tagEntity) where T : IGraphicComponent => default;
            public IEnumerable<IGraphicComponent> GetComponents(ElementTag tagEntity) => Enumerable.Empty<IGraphicComponent>();
            public IEnumerable<T> GetComponents<T>() where T : IGraphicComponent => Enumerable.Empty<T>();
            public IEnumerable<IGraphicComponent> GetComponents(ElementTag tag, params Type[] types)
                 => Enumerable.Empty<IGraphicComponent>();
            public IEnumerable<GraphicEntity> GetEntities() => Enumerable.Empty<GraphicEntity>();
            public GraphicEntity GetEntity(ElementTag tag) => default;
            public IEnumerable<GraphicEntity> GetEntity(Func<GraphicEntity, bool> predicate) => Enumerable.Empty<GraphicEntity>();
            public GraphicEntity GetEntityOf<T>(T com) where T : IGraphicComponent => GraphicEntity.Empty();
            public T GetOrCreateComponent<T>(ElementTag tagEntity, T newone) where T : IGraphicComponent => default;
            public bool HasEntityContained<T>(ElementTag tag) where T : IGraphicComponent => false;
            public bool HasEntityContained(ElementTag tag, params Type[] types) => false;
            public bool HasEntityOfComponentContained<T>(T com) where T : IGraphicComponent => false;
            public bool IsExisted(ElementTag tag) => false;
            public void RemoveComponent<T>(ElementTag tagEntity) where T : IGraphicComponent { }
            public void RemoveEntity(ElementTag elementTag) { }
            public void Synchronize(int theadId) { }
            public bool TryGetComponent<T>(ElementTag tagEntity, out T component)
                where T : IGraphicComponent {
                component = default;
                return false;
            }
            public void UpdateComponents<T>(ElementTag tagEntity, T com) where T : IGraphicComponent { }

            public Task UpdateComponents<T>(ElementTag tagEntity, params T[] newComponents) where T : IGraphicComponent {
                throw new NotImplementedException();
            }

            Task IComponentManager.UpdateComponent<T>(ElementTag tagEntity, T com) {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Should be lazy implementation in this case
        /// need to add some operation to sync to wait entity creation to invoke chained operations
        /// </summary>
        /// <returns></returns>
        public static GraphicEntity Empty() {
            return new GraphicEntity(ElementTag.Empty, new EmptyManager(), new EmptyManager(), new EntityOrderContainer(), true);
        }
        public static GraphicEntity Create(ElementTag tag, IComponentManager manager, IEntityManager emanager, EntityOrderContainer order) {
          return new GraphicEntity(tag, manager, emanager, order, false);
        }


        public ElementTag Tag { get; }
        readonly IComponentManager manager;
        readonly IEntityManager emanager;
        readonly EntityOrderContainer order;

        GraphicEntity(ElementTag tag, IComponentManager manager, IEntityManager emanager,EntityOrderContainer order, bool empty) {
            this.order = order;
            this.manager = manager;
            this.emanager = emanager;
            Tag =tag;
            IsEmpty = empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Only certain type, not base or interface</typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : IGraphicComponent {
            return manager.GetComponent<T>(Tag);
        }
        public IEnumerable<IGraphicComponent> GetComponents(params Type[] types) {
            return manager.GetComponents(Tag, types);
        }
        public IEnumerable<IGraphicComponent> GetComponents() {
            return manager.GetComponents(Tag);
        }
        public T GetOrCreateComponent<T>(T newone) where T : IGraphicComponent {
            return manager.GetOrCreateComponent<T>(Tag, newone);
        }
        public bool TryGetComponent<TComponent>(out TComponent component) where TComponent : IGraphicComponent {
            return manager.TryGetComponent(Tag, out component);
        }
        public bool TryGetComponents<T1, T2>(out T1 c1, out T2 c2)
           where T1 : IGraphicComponent
           where T2 : IGraphicComponent {
            c2 = default;
            return manager.TryGetComponent(Tag, out c1)
                && manager.TryGetComponent(Tag, out c2);
        }
        public bool TryGetComponents<T1, T2, T3>(out T1 c1, out T2 c2, out T3 c3)
        where T1 : IGraphicComponent
        where T2 : IGraphicComponent
        where T3 : IGraphicComponent {
            c2 = default;
            c3 = default;
            return manager.TryGetComponent(Tag, out c1) 
                && manager.TryGetComponent(Tag, out c2) 
                && manager.TryGetComponent(Tag, out c3);
        }

        public GraphicEntity AddComponent<T>(T component) where T : IGraphicComponent {
            manager.AddComponent(Tag, component);
            return this;
        }
        public GraphicEntity RemoveComponent<T>() where T : IGraphicComponent {
            manager.RemoveComponent<T>(Tag);
            return this;
        }
        public GraphicEntity RemoveComponents<T1, T2>() where T1 : IGraphicComponent where T2 : IGraphicComponent {
            manager.RemoveComponent<T1>(Tag);
            manager.RemoveComponent<T2>(Tag);
            return this;
        }
        public Task UpdateComponent<T>(T com) where T : IGraphicComponent {
            return manager.UpdateComponent(Tag, com);
        }
        public Task UpdateComponents<T>(params T[] coms) where T : IGraphicComponent {
            return manager.UpdateComponents(Tag, coms);
        }

        public bool Contains(params Type[] types) {
            return manager.HasEntityContained(Tag, types);
        }
        public bool Contains<T>() where T : IGraphicComponent {
            return manager.HasEntityContained<T>(Tag);
        }

        public int GetOrderIndex<TSys>()
            where TSys : IGraphicSystem {
            return order.Get<TSys>(Tag);
        }

        public void Remove() {
            emanager.RemoveEntity(Tag);
        }

        public bool IsDestroyed => !emanager.IsExisted(Tag);
        public bool IsEmpty { get; }

        public override string ToString() {
            return $"Entity[{Tag}]";
        }
    }

    public class OrderSystemContainer : Dictionary<Type, int> {

    }

    public class EntityOrderContainer {
        readonly Dictionary<ElementTag, OrderSystemContainer> componentOrderIndex;
        readonly Dictionary<Type, int> systemsOrder;

        public EntityOrderContainer() {
            componentOrderIndex = new Dictionary<ElementTag, OrderSystemContainer>();
            systemsOrder = new Dictionary<Type, int>();
        }
        public EntityOrderContainer RegisterOrder<TSys>(ElementTag tag,int index) {
            if (!componentOrderIndex.TryGetValue(tag, out var ordering)) {
                ordering = new OrderSystemContainer();
                componentOrderIndex.Add(tag, ordering);
            }
            var t = typeof(TSys);

            ordering.Add(t, index);
            IncrementSystemOrderIndex(t);

            return this;
        }

        public EntityOrderContainer RegisterOrder<TSys>(ElementTag tag) {
            if (!componentOrderIndex.TryGetValue(tag, out var ordering)) {
                ordering = new OrderSystemContainer();
                componentOrderIndex.Add(tag, ordering);
            }
            var t = typeof(TSys);

            ordering.Add(t, IncrementSystemOrderIndex(t));

            return this;
        }

        public int Get<TSys>(ElementTag tag)
            where TSys : IGraphicSystem {
            if (!componentOrderIndex.ContainsKey(tag)) {
                return int.MaxValue;
            }
            return componentOrderIndex[tag][typeof(TSys)];
        }

        int IncrementSystemOrderIndex(Type t) {
            if (!systemsOrder.ContainsKey(t)) {
                systemsOrder.Add(t, 0);
            } else {
                systemsOrder[t] = systemsOrder[t] + 1;
            }
            return systemsOrder[t];
        }
    }
}
