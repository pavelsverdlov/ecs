using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.D2;
using D3DLab.SDX.Engine.Shader;
using SharpDX.D3DCompiler;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;

namespace D3DLab.SDX.Engine.Rendering {
    public interface IRenderProperties {
       CameraState CameraState { get; }
    }

    public interface IRenderTechnique<TProperties> : INestedGraphicSystem where TProperties : IRenderProperties {
        IEnumerable<IRenderTechniquePass> GetPass();
        /// <summary>
        /// remove all entities from Technique
        /// </summary>
        void Cleanup();
        void Render(GraphicsDevice Graphics, TProperties game);
        void RegisterEntity(GraphicEntity entity);
        bool IsAplicable(GraphicEntity entity);
        /// <summary>
        /// Cleanup render cache, such as pass/shader/buffers/BlendState/render targets etc.
        /// should be invoked each time when render surface/device is recreared
        /// </summary>
        void CleanupRenderCache();
    }
    

    public abstract class D3DAbstractRenderTechnique<TProperties> where TProperties : IRenderProperties {
        public int OrderId { get; set; }

        protected readonly LinkedList<GraphicEntity> entities;
        protected D3DAbstractRenderTechnique() {
            entities = new LinkedList<GraphicEntity>();
        }

        public void Render(GraphicsDevice graphics, TProperties game) {
            Rendering(graphics, game);
        }

        public abstract bool IsAplicable(GraphicEntity entity);

        protected abstract void Rendering(GraphicsDevice graphics, TProperties game);
        
        public void RegisterEntity(GraphicEntity entity) {
            entities.AddLast(entity);
        }

        public void Cleanup() {
            entities.Clear();
        }
        protected ShaderResourceView ConvertToResource(System.Drawing.Bitmap btm, TextureLoader loader) {
            return loader.LoadBitmapShaderResource(btm);
        }

        protected ShaderResourceView[] ConvertToResources(TexturedMaterialComponent material, TextureLoader loader) {
            var resources = new ShaderResourceView[material.Images.Length];
            for (var i = 0; i < material.Images.Length; i++) {
                var file = material.Images[i];
                resources[i] = loader.LoadShaderResource(file);
            }
            return resources;
        }
        protected ShaderResourceView[] ConvertToResources(MemoryTexturedMaterialComponent material, TextureLoader loader) {
            var resources = new ShaderResourceView[material.MemoryImages.Length];
            for (var i = 0; i < material.MemoryImages.Length; i++) {
                var file = material.MemoryImages[i];
                resources[i] = loader.LoadShaderResource(file);
            }
            return resources;
        }

        public virtual void CleanupRenderCache() {

        }


    }
}
