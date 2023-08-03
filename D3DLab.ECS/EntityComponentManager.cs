using D3DLab.ECS;
using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace D3DLab.ECS {
    public sealed class EntityComponentManager : IEntityManager, IComponentManager {

        #region IEntityManager
        public GraphicEntity CreateEntity(ElementTag tag) {
            var en = _CreateEntity(tag);

            entitySynchronizer.Add((owner, input) => {
                // owner.entities.Add(tag);
                this.entities.Add(input.Tag, new Dictionary<Type, ElementTag>());
                owner.notify.NotifyAdd(input);
                // owner.components.Add(input.Tag, new Dictionary<Type, IGraphicComponent>());
                return true;
            }, en);

            return en;
        }
        public void RemoveEntity(ElementTag elementTag) {
            entitySynchronizer.Add((owner, input) => {
                if (owner.entities.ContainsKey(elementTag)) {
                    var entity = _CreateEntity(elementTag);

                    notify.NotifyRemove(entity);

                    foreach (var component in owner.GetComponents(entity.Tag)) {
                        owner._RemoveComponent(entity.Tag, component);
                    }
                    owner.entities.Remove(entity.Tag);
                    owner.components.Remove(entity.Tag);
                    //this.entities.Remove(entity.Tag);
                    //notify.NotifyChange(entity);
                }
                return true;
            }, default);
        }
        public IEnumerable<GraphicEntity> GetEntities() {
            return entities.Keys.Select(_CreateEntity);
        }
        public GraphicEntity GetEntity(ElementTag tag) {
            return GraphicEntity.Create(tag, this, this, orderContainer);
        }
        public GraphicEntity GetEntityOf<T>(T com) where T : IGraphicComponent {
            var typec = typeof(T);
            foreach (var en in entities) {
                if (en.Value.TryGetValue(typec, out var tag) && tag == com.Tag) {
                    return GraphicEntity.Create(en.Key, this, this, orderContainer);
                }
            }
            return GraphicEntity.Empty();
        }
        public IEnumerable<GraphicEntity> GetEntity(Func<GraphicEntity, bool> predicate) {
            var res = new List<GraphicEntity>();
            foreach (var tag in entities.Keys) {
                var en = _CreateEntity(tag);
                if (predicate(en)) {
                    res.Add(en);
                }
            }
            return res;
        }
        public bool IsExisted(ElementTag tag) {
            return entities.ContainsKey(tag);
        }

        GraphicEntity _CreateEntity(ElementTag tag) {
            return GraphicEntity.Create(tag, this, this, orderContainer);
        }

        #endregion

        #region IComponentManager        

        public IComponentManager AddComponent<T>(ElementTag tagEntity, T com) where T : IGraphicComponent {
            return AddComponent(tagEntity, com, out _);
        }

        public IComponentManager AddComponent<T>(ElementTag tagEntity, T com, out Task awaiter) where T : IGraphicComponent {
            var _type = typeof(T);
            if (_type == typeof(IGraphicComponent)) {
                throw new NotSupportedException("IGraphicComponent is incorrect type, must be the certain component type.");
            }
            if (components.ContainsKey(com.Tag)) {
                throw new NotSupportedException($"Component {typeof(T)} '{com.Tag}' is already belong to other Entity.");
            }
            if (entities.TryGetValue(tagEntity, out var map) && map.ContainsKey(_type)) {
                throw new NotSupportedException($"Component type {typeof(T)} is already exist in '{com.Tag}' Entity.");
            }

            awaiter = comSynchronizer.Add((owner, inp) => owner._AddComponent(tagEntity, inp), com);

            return this;
        }

        public void RemoveComponent<T>(ElementTag tagEntity) where T : IGraphicComponent {
            if (typeof(T) == typeof(IGraphicComponent)) {
                throw new NotSupportedException("IGraphicComponent is incorrect type, must be the certain component type.");
            }
            comSynchronizer.Add<T>((owner, c) => {
                var com = GetComponent<T>(tagEntity);
                return owner._RemoveComponent(tagEntity, com);
            }, default);
        }


        public T GetComponent<T>(ElementTag tagEntity) where T : IGraphicComponent {
            if (!entities[tagEntity].TryGetValue(typeof(T), out var comTag)) {
                return default;
            }
            if (!components.TryGetValue(comTag, out var found)) {
                return default;
            }
            return (T)found;
        }
        public IEnumerable<IGraphicComponent> GetComponents(ElementTag tagEntity) {
            if (!entities.ContainsKey(tagEntity)) {
                return new IGraphicComponent[0];
            }
            return entities[tagEntity].Values.Select(tag => components[tag]).ToArray();
        }
        public IEnumerable<T> GetComponents<T>() where T : IGraphicComponent {
            return components.Values.OfType<T>().ToArray();
        }
        public IEnumerable<IGraphicComponent> GetComponents(ElementTag entity, params Type[] types) {
            var res = new List<IGraphicComponent>();
            foreach (var t in types) {
                if (entities[entity].TryGetValue(t, out var ctag)) {
                    res.Add(components[ctag]);
                }
            }
            return res;
        }
        public T GetOrCreateComponent<T>(ElementTag tagEntity, T newone) where T : IGraphicComponent {
            var any = GetComponent<T>(tagEntity);
            if (any.IsValid && entities[tagEntity].ContainsKey(typeof(T))) {
                return any;
            }
            AddComponent(tagEntity, newone);
            return newone;
        }

        public bool TryGetComponent<TComponent>(ElementTag tagEntity, out TComponent component)
            where TComponent : IGraphicComponent {
            if (!entities[tagEntity].TryGetValue(typeof(TComponent), out var comTag)) {
                component = default;
                return false;
            }
            if(!components.TryGetValue(comTag, out var found)){
                component = default;
                return false;
            }
            component = (TComponent)found;
            return true;
        }        

        public bool HasEntityOfComponentContained<T>(T com) where T : IGraphicComponent {
            var type = typeof(T);
            foreach (var en in entities) {
                if (en.Value.TryGetValue(type, out var tag) && tag == com.Tag) {
                    return true;
                }
            }
            return false;
        }
        public bool HasEntityContained<T>(ElementTag entity) where T : IGraphicComponent {
            return entities[entity].ContainsKey(typeof(T));
        }
        public bool HasEntityContained(ElementTag entity, params Type[] types) {
            return types.All(type => entities[entity].ContainsKey(type));
        }

        public Task UpdateComponents<T>(ElementTag tagEntity, T newComponent) where T : IGraphicComponent {
            if (typeof(T) == typeof(IGraphicComponent)) {
                throw new NotSupportedException("IGraphicComponent is incorrect type, must be the certain component type.");
            }
            if (components.ContainsKey(newComponent.Tag)) {
                throw new NotSupportedException($"Component {typeof(T)} '{newComponent.Tag}' is already belong to other Entity.");
            }

            return comSynchronizer.Add((owner, newCom) => {
                var type = newCom.GetType();
                //do not check IsValid OR IsDisposed because it is not important for removing 
                if (entities[tagEntity].TryGetValue(type, out var oldTag)) {
                    components[oldTag].Dispose();
                    var removed = components.Remove(oldTag);
                    notify.NotifyRemove(oldTag);

                    entities[tagEntity][type] = newCom.Tag;
                } else {
                    //case: if it is updating not existed component
                    entities[tagEntity].Add(type, newCom.Tag);
                }
#if DEBUG
                Debug.Assert(!components.ContainsKey(newCom.Tag));
#endif
                components.Add(newCom.Tag, newCom);

                notify.NotifyAdd(newCom);
                return true;
            }, newComponent);
        }

        bool _AddComponent<T>(in ElementTag tagEntity, in T com) where T : IGraphicComponent {
            if (!entities.ContainsKey(tagEntity)) {
                return false;
            }
            var type = typeof(T);
            components.Add(com.Tag, com);
            entities[tagEntity].Add(type, com.Tag);
            notify.NotifyAdd(com);
            return true;
        }
        bool _RemoveComponent<T>(in ElementTag tagEntity, in T com) where T : IGraphicComponent {
            if (!entities.ContainsKey(tagEntity)) {
                return false;
            }
          
            var removed = components.Remove(com.Tag);
            if (!removed) {
                return false;
            }

            var type = typeof(T);
            removed = entities[tagEntity].Remove(type);
            com.Dispose();

            notify.NotifyRemove(com);
            return true;
        }

        #endregion

        readonly IManagerChangeNotify notify;

        readonly SynchronizationContextAdapter<EntityComponentManager, GraphicEntity> entitySynchronizer;
        readonly SynchronizationContextAdapter<EntityComponentManager> comSynchronizer;
        readonly SynchronizationContextAdapter<EntityComponentManager, IFlyweightGraphicComponent> flyweightComSynchronizer;

        readonly Dictionary<ElementTag, Dictionary<Type, ElementTag>> entities;
        readonly Dictionary<ElementTag, IGraphicComponent> components;

        readonly Dictionary<IFlyweightGraphicComponent, HashSet<ElementTag>> flyweightComponents;
        readonly EntityOrderContainer orderContainer;

        public EntityComponentManager(IManagerChangeNotify notify, EntityOrderContainer orderContainer, 
            RenderLoopSynchronizationContext syncContext) {
            this.orderContainer = orderContainer;
            this.notify = notify;
            entitySynchronizer = new SynchronizationContextAdapter<EntityComponentManager, GraphicEntity>(this, syncContext);
            comSynchronizer = new SynchronizationContextAdapter<EntityComponentManager>(this, syncContext);
            flyweightComSynchronizer = new SynchronizationContextAdapter<EntityComponentManager, IFlyweightGraphicComponent>(this, syncContext);
            entities = new Dictionary<ElementTag, Dictionary<Type, ElementTag>>();
            components = new Dictionary<ElementTag, IGraphicComponent>();
        }


        public void Dispose() {
            foreach (var com in components) {
                com.Value.Dispose();
            }

            components.Clear();
            entities.Clear();
            //flyweightComponents.Clear();
            entities.Clear();
        }


    }
}
