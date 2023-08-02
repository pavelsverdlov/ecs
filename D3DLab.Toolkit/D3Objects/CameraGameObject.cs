using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Systems;
using D3DLab.Toolkit.Components.Camera;
using System.Threading;

namespace D3DLab.Toolkit.D3Objects {
    public class CameraObject : SingleVisualObject {
        static int cameras = 0;

        public CameraObject(ElementTag tag, string descr) : base(tag, descr) { }

        public static CameraObject CreatePerspective<TRenderSystem, TToolkitFrameProperties>(IContextState context)
            where TRenderSystem : BaseRenderSystem<TToolkitFrameProperties>
            where TToolkitFrameProperties : IToolkitFrameProperties {

            IEntityManager manager = context.GetEntityManager();
            var cameraTag = new ElementTag("CameraEntity_" + Interlocked.Increment(ref cameras));

            var obj = new CameraObject(cameraTag, "PerspectiveCamera");

            manager.CreateEntity(cameraTag)
                   //.AddComponent(new OrthographicCameraComponent(Window.Width, Window.Height));
                   .AddComponent(new PerspectiveCameraComponent());

            {//entities ordering 
                context.EntityOrder
                       .RegisterOrder<TRenderSystem>(cameraTag, 0)
                       .RegisterOrder<DefaultInputSystem>(cameraTag, 0);
            }

            return obj;
        }

        public static CameraObject UpdatePerspective<TRenderSystem, TToolkitFrameProperties>(
            IContextState context, ElementTag tag)
                where TRenderSystem : BaseRenderSystem<TToolkitFrameProperties>
                where TToolkitFrameProperties : IToolkitFrameProperties {

            IEntityManager manager = context.GetEntityManager();

            var obj = new CameraObject(tag, "PerspectiveCamera");
            manager.GetEntity(tag)
                   .AddComponent(new PerspectiveCameraComponent());

            {//entities ordering 
                context.EntityOrder
                       .RegisterOrder<TRenderSystem>(tag, 0)
                       .RegisterOrder<DefaultInputSystem>(tag, 0);
            }

            return obj;
        }

        public static CameraObject CreateOrthographic<TRenderSystem, TToolkitFrameProperties>(
            IContextState context, IRenderableSurface win) 
                where TRenderSystem : BaseRenderSystem<TToolkitFrameProperties> 
                where TToolkitFrameProperties : IToolkitFrameProperties {

            var manager = context.GetEntityManager();
            var cameraTag = new ElementTag("CameraEntity_" + Interlocked.Increment(ref cameras));

            var obj = new CameraObject(cameraTag, "OrthographicCamera");

            manager.CreateEntity(cameraTag)
                   .AddComponent(new OrthographicCameraComponent(win.Size));

            {//entities ordering 
                context.EntityOrder
                       .RegisterOrder<TRenderSystem>(cameraTag, 0)
                       .RegisterOrder<DefaultInputSystem>(cameraTag, 0);
            }

            return obj;
        }

        public static CameraObject UpdateOrthographic<TRenderSystem>
            (ElementTag camera, IContextState context, IRenderableSurface win)  {

            var manager = context.GetEntityManager();

            var obj = new CameraObject(camera, "OrthographicCamera");

            manager.GetEntity(camera)
                   .AddComponent(new OrthographicCameraComponent(win.Size));

            {//entities ordering 
                context.EntityOrder
                       .RegisterOrder<TRenderSystem>(camera, 0)
                       .RegisterOrder<DefaultInputSystem>(camera, 0);
            }

            return obj;
        }
    }
}
