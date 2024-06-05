using D3DLab.ECS;
using D3DLab.Toolkit.Components;

using System;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.Toolkit.D3Objects {
    public class MultiVisualObject : GameObject {
        /// <summary>
        /// this status is only be actual if Show/Hide methods were used
        /// </summary>
        public bool IsVisible { get; set; }
        public IEnumerable<ElementTag> Tags => tags;

        protected readonly List<ElementTag> tags;
        public MultiVisualObject(IEnumerable<ElementTag> tags, string desc) : base(desc) {
            this.tags = tags.ToList();
        }
        public MultiVisualObject(string desc) : base(desc) {
            this.tags = new List<ElementTag>();
        }

        void Cleanup(ElementTag tag, IContextState context) {
            context.GetEntityManager().RemoveEntity(tag);
        }
        protected void Hide(ElementTag tag, IEntityManager manager) {
            var en = manager.GetEntity(tag);

            en.UpdateComponent(en.GetComponent<RenderableComponent>().Disable());
        }

        protected void Show(ElementTag tag, IEntityManager manager) {
            var en = manager.GetEntity(tag);
            en.UpdateComponent(en.GetComponent<RenderableComponent>().Enable());
        }


        public void AddVisualObject(ElementTag tag) {
            tags.Add(tag);
        }
        public void RemoveVisualObject(IContextState context, ElementTag tag) {
            if (tags.Remove(tag)) {
                Cleanup(tag, context);
            }
        }

        public override void Hide(IContextState context) {
            var m = context.GetEntityManager();
            foreach (var tag in tags) {
                Hide(tag, m);
            }
            IsVisible = false;
        }

        public override void Show(IContextState context) {
            var m = context.GetEntityManager();
            foreach (var tag in tags) {
                Show(tag, m);
            }
            IsVisible = true;
        }

        public override void Cleanup(IContextState context) {
            foreach(var tag in tags) {
                Cleanup(tag, context);
            }
            tags.Clear();
        }

        public override void AddComponent<TComponent>(IContextState context, TComponent component) {
            foreach (var tag in Tags) {
                context
                    .GetEntityManager()
                    .GetEntity(tag)
                    .AddComponent(component);
            }
        }
    }

    public class SingleVisualObject : GameObject {
        public ElementTag Tag { get; }
        /// <summary>
        /// this status is only be actual if Show/Hide methods were used
        /// </summary>
        public bool IsVisible { get; set; }

        public SingleVisualObject(ElementTag tag, string desc) : base(desc) {
            Tag = tag;
            IsVisible = true;
        }

        public override void Hide(IContextState context) {
            var en = context.GetEntityManager().GetEntity(Tag);
            en.UpdateComponent(en.GetComponent<RenderableComponent>().Disable());
            IsVisible = false;
        }

        public override void Show(IContextState context) {
            var en = context.GetEntityManager().GetEntity(Tag);
            en.UpdateComponent(en.GetComponent<RenderableComponent>().Enable());
            IsVisible = true;
        }

        public virtual GraphicEntity GetEntity(IEntityManager manager) => manager.GetEntity(Tag);

        public void LookAtSelf(IEntityManager manager) {
            //var entity = manager.GetEntity(Tag);
            //var geos = entity.GetComponents<IGeometryComponent>();
            //var hasTransformation = entity.GetComponents<TransformComponent>();
            //if (geos.Any()) {
            //    var geo = geos.First();
            //    var local = geo.Box.GetCenter();

            //    if (hasTransformation.Any()) {
            //        local = Vector3.TransformNormal(local, hasTransformation.First().MatrixWorld);
            //    }

            //    var com = new MoveCameraToTargetComponent { Target = Tag, TargetPosition = local };
            //    entity.AddComponent(com);
            //}
            throw new NotImplementedException();
        }

        public override void Cleanup(IContextState context) {
            context.GetEntityManager().RemoveEntity(Tag);
        }

        public override void AddComponent<TComponent>(IContextState context, TComponent component) {
            context
                .GetEntityManager()
                .GetEntity(Tag)
                .AddComponent(component);
        }
    }
}
