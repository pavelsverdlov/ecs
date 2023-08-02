using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DLab.Toolkit {
    public abstract class BaseRenderSystem<TToolkitFrameProperties> : D3DRenderSystem<TToolkitFrameProperties>
        where TToolkitFrameProperties : IToolkitFrameProperties {

        protected abstract void RenderFrame(GraphicsFrame frame, ISceneSnapshot snapshot, RenderTechniqueRegistrator<TToolkitFrameProperties> registrator);

        protected override void Executing(ISceneSnapshot snapshot) {
            Synchronize();
            var registrator = new RenderTechniqueRegistrator<TToolkitFrameProperties>(nested);
            try {
                using (var frame = graphics.FrameBegin()) {
                    RenderFrame(frame, snapshot, registrator);
                }
            } catch (SharpDX.CompilationException cex) {
                System.Diagnostics.Trace.WriteLine($"CompilationException[\n{cex.Message.Trim()}]");
            } catch (SharpDX.SharpDXException shex) {
                var reason = graphics.Device.D3DDevice.DeviceRemovedReason;
                System.Diagnostics.Trace.WriteLine(shex.Message);
                throw shex;
            } catch (System.Runtime.InteropServices.SEHException seh) {
                System.Diagnostics.Trace.WriteLine(seh.Message);
                throw seh;
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                throw ex;
            } finally {
                registrator.Cleanup();
                Pass = registrator.Techniques.SelectMany(x => x.GetPass()).ToArray();
            }
        }

    }
}
