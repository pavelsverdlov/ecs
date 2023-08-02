using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Input;
using D3DLab.ECS.Render;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit._CommonShaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace D3DLab.Toolkit.Render {

    class IncludeResourse : IIncludeResourse {
        readonly string path;
        public string Key { get; }

        readonly ManifestResourceLoader loader;
        public IncludeResourse(string key, string path) {
            Key = key;
            this.path = path;
            loader = new ManifestResourceLoader(this.GetType());
        }

        public Stream GetResourceStream() {
            return loader.GetResourceStreamByName(path);
        }
    }
    public sealed class RenderEngine : DefaultEngine {

        public static RenderEngine Create(IRenderableWindow window,
            IInputManager inputManager, IContextState context, EngineNotificator notificator) {
            GraphicsDeviceFactory.CreateOutputHandleDevice(window);
            return new RenderEngine(GraphicsDeviceFactory.CreateOutputHandleDevice(window),
               window, inputManager, context, notificator);
        }

        public static RenderEngine Create(IFrameRenderableSurface surface,
            IInputManager inputManager, IContextState context, EngineNotificator notificator) {
            return new RenderEngine(GraphicsDeviceFactory.CreateOutputTargetView(surface),
                surface, inputManager, context, notificator);
        }


        /// <summary>
        /// DO NOT COPY OR PASS THIS OBJECT
        /// </summary>
        public readonly SynchronizedGraphics Graphics;

        RenderEngine(GraphicsDevice device, IRenderableSurface surface,
            IInputManager inputManager, IContextState context, EngineNotificator notificator)
            : base(surface, inputManager, context, notificator) {

            Graphics = new SynchronizedGraphics(surface, device);

            var includes = new System.Collections.Generic.Dictionary<string, IIncludeResourse>();

            includes.Add("Common", new IncludeResourse("Common", "D3DLab.Toolkit._CommonShaders.Common.hlsl"));

            Graphics.Device.Compilator.AddInclude(new D3DLab.SDX.Engine.Shader.D3DIncludeAdapter(includes));
        }

        protected override ISceneSnapshot CreateSceneSnapshot(InputSnapshot isnap, TimeSpan frameRateTime)
            => new ToolkitFrameSnapshot(WorldTag, CameraTag) {
                Surface = Surface,
                Notifier = Notificator,
                InputSnapshot = isnap,
                FrameRateTime = frameRateTime,
                Lights = new LightState[LightStructBuffer.MaxCount],
            };

        protected override void Initializing() {


        }

        public override void Dispose() {
            base.Dispose();
            Graphics.Dispose();
        }

        protected override bool Synchronize() {
            var changed = Graphics.IsChanged;
            Graphics.Synchronize(Thread.CurrentThread.ManagedThreadId);
            return changed || base.Synchronize();
        }
    }

}
