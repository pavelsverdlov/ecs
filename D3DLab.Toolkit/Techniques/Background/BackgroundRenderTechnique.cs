using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Components;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Techniques.Background {
    /// <summary>
    /// Technique to render texture on whole surface, add as first technique in render system
    /// </summary>
    /// <typeparam name="TProperties"></typeparam>
    ///<example>
    /// var bmp = new System.Drawing.Bitmap(2, 2);
    /// bmp.SetPixel(1, 0, System.Drawing.Color.Red);
    /// bmp.SetPixel(0, 0, System.Drawing.Color.Blue);
    /// bmp.SetPixel(1, 1, System.Drawing.Color.Blue);
    /// bmp.SetPixel(0, 1, System.Drawing.Color.Red);
    /// 
    /// var stream = new System.IO.MemoryStream();
    /// bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
    /// stream.Position = 0;
    /// 
    /// manager
    ///   .CreateEntity(new ElementTag("Background"))
    ///   .AddComponents(
    ///       BackgroundRenderComponent.Create(),
    ///       new MemoryTexturedMaterialComponent(stream) { IsModified = true});
    ///</example>
    public class BackgroundRenderTechnique<TProperties> :
        NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties>
        where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.Background.shader.hlsl";

        readonly D3DShaderTechniquePass pass;
        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<DepthStencilState> depthStencilState;
        readonly RasterizerStateDescription2 rasterizerStateDescription;

        //readonly ShaderDebugMode debug1;

        public BackgroundRenderTechnique(){
            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(BackgroundRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "BG_"));

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            depthStencilState = new DisposableSetter<DepthStencilState>(disposer);

            rasterizerStateDescription = new RasterizerStateDescription2() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,

                IsFrontCounterClockwise = false,
                IsScissorEnabled = false,
                IsAntialiasedLineEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = .0f,
                SlopeScaledDepthBias = .0f
            };

            //debug1 = new ShaderDebugMode(new System.IO.DirectoryInfo(@"D:\Storage_D\trash\archive\shaders\"), pass);
            //debug1.Activate();
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };

        public override bool IsAplicable(GraphicEntity entity) {
            return entity.TryGetComponent<RenderableComponent>(out var ren)
                && ren.IsRenderable
                && ren.Technique == RenderTechniques.Background
                && entity.Contains(
                    typeof(BackgroundRenderComponent), 
                    typeof(MemoryTexturedMaterialComponent));
        }

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            }

            if (!depthStencilState.HasValue) {
                depthStencilState.Set(new DepthStencilState(graphics.D3DDevice, 
                    D3DDepthStencilDefinition.DepthDisabled.Description));
            }

            //SharpDX.Direct3D11.Device.FromPointer<SharpDX.Direct3D11.Device>(System.IntPtr.Zero);
            //var d = new D3DLab.CUDA.TestCudaLib();
            //d.SetDevice(graphics.D3DDevice);
            //var text = graphics.CreateTexture2D(new GraphicSurfaceSize(256, 256));
            //d.SetTexture(text.NativePointer);
            //d.SetTexture(text);

            foreach (var en in entities) {
                if (en.TryGetComponent<BackgroundRenderComponent>(out var render)) {

                    if (en.TryGetComponent<MemoryTexturedMaterialComponent>(out var texture) && texture.IsModified) {
                        render.TextureResources.Set(ConvertToResources(texture, graphics.TexturedLoader));
                        render.SampleState.Set(graphics.CreateSampler(SamplerStateDescriptions.Default));
                        texture.IsModified = false;
                    }

                    //if (en.TryGetComponent<MaterialColorComponent>(out var color) && color.IsValid) {

                    //}

                    graphics.ClearAllShader();
                    graphics.SetVertexShader(vertexShader);
                    graphics.SetPixelShader(pixelShader);

                    graphics.DisableIndexVertexBuffers();

                    if (render.TextureResources.HasValue && render.SampleState.HasValue) {
                        context.PixelShader.SetShaderResources(0, render.TextureResources.Get());
                        context.PixelShader.SetSampler(0, render.SampleState.Get());
                    } 

                    context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
                    context.OutputMerger.SetDepthStencilState(depthStencilState.Get());
                    
                    using (var rasterizerState = graphics.CreateRasterizerState(rasterizerStateDescription)) {
                        context.Rasterizer.State = rasterizerState;
                        context.Draw(4, 0);
                    }

                    break; //support only one BackgroundRenderComponent
                }
            }
        }
    }
}
