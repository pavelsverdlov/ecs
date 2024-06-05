using D3DLab.ECS;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    enum RenderTechniques {
        Undefined,
        TriangleColored,
        TriangleTextured,
        Lines,
        OneFrameFlatWhite,
        Background,
        SpherePoints, 
    }

    /// <summary>
    /// Component to allow rendering
    /// </summary>
    public struct RenderableComponent : IGraphicComponent {

        #region creators

        public static RenderableComponent AsPoints() => 
            new RenderableComponent(CullMode.None, PrimitiveTopology.PointList, RenderTechniques.SpherePoints) {
                Tag = ElementTag.New(),
                IsValid = true,
                DepthStencilStateDefinition = D3DDepthStencilDefinition.DepthEnabled,
                RasterizerStateDescription = new RasterizerStateDescription2() {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsMultisampleEnabled = false,

                    IsFrontCounterClockwise = false,
                    IsScissorEnabled = false,
                    IsAntialiasedLineEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = .0f,
                    SlopeScaledDepthBias = .0f
                },
                HasBlendState = true,
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateDisabled,
            };

        public static RenderableComponent AsBackground()
            => new RenderableComponent(CullMode.None, PrimitiveTopology.TriangleStrip, RenderTechniques.Background) {
                Tag = ElementTag.New(),
                IsValid = true,
                DepthStencilStateDefinition = D3DDepthStencilDefinition.DepthDisabled,
                RasterizerStateDescription = new RasterizerStateDescription2() {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsMultisampleEnabled = false,

                    IsFrontCounterClockwise = false,
                    IsScissorEnabled = false,
                    IsAntialiasedLineEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = .0f,
                    SlopeScaledDepthBias = .0f
                },
                HasBlendState = true,
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled,
            };

        public static RenderableComponent AsFlatWhiteTriangleList()
             => AsTriangleList(CullMode.Front, D3DDepthStencilDefinition.DepthEnabled, RenderTechniques.OneFrameFlatWhite);

        public static RenderableComponent AsTriangleColoredList(CullMode mode, D3DDepthStencilDefinition depth)
             => AsTriangleList(mode, depth, RenderTechniques.TriangleColored);
        public static RenderableComponent AsTriangleColored(PrimitiveTopology topology) {
            var mode = CullMode.Front;
            return new RenderableComponent(mode, topology, RenderTechniques.TriangleColored) {
                Tag = ElementTag.New(),
                IsValid = true,
                HasBlendState = true,
                DepthStencilStateDefinition = D3DDepthStencilDefinition.DepthEnabled,
                RasterizerStateDescription = D3DRasterizerStateDescriptions.Default(mode),
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled,
            };
        }
        public static RenderableComponent AsTriangleColoredList(CullMode mode)
            => new RenderableComponent(mode, PrimitiveTopology.TriangleList, RenderTechniques.TriangleColored) {
                Tag = ElementTag.New(),
                IsValid = true,
                HasBlendState = true,
                RasterizerStateDescription = D3DRasterizerStateDescriptions.Default(mode),
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled,
            };

        public static RenderableComponent AsTriangleTexturedList(CullMode mode)
            => AsTriangleList(mode, D3DDepthStencilDefinition.DepthEnabled, RenderTechniques.TriangleTextured);
        public static RenderableComponent AsLineList()
           => AsLineList(D3DRasterizerStateDescriptions.Lines, RenderTechniques.Lines);


        static RenderableComponent AsTriangleList(CullMode mode, D3DDepthStencilDefinition depth,
            RenderTechniques technique)
            => new RenderableComponent(mode, PrimitiveTopology.TriangleList, technique) {
                Tag = ElementTag.New(),
                IsValid = true,
                HasBlendState = true,
                DepthStencilStateDefinition = depth,
                RasterizerStateDescription = D3DRasterizerStateDescriptions.Default(mode),
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled,
            };
       
        static RenderableComponent AsLineList(RasterizerStateDescription2 rast, RenderTechniques technique)
          => new RenderableComponent(CullMode.None, PrimitiveTopology.LineList, technique) {
              Tag = ElementTag.New(),
              IsValid = true,
              HasBlendState = true,
              RasterizerStateDescription = rast,
              DepthStencilStateDefinition = D3DDepthStencilDefinition.DepthEnabled,
              BlendStateDescription = D3DBlendStateDescriptions.BlendStateDisabled,
          };

        #endregion


        internal RenderTechniques Technique { get; }
        public CullMode CullMode { get; private set; }
        public PrimitiveTopology PrimitiveTopology { get;}


        //perhaps these blocks should be separate components as DepthStencilStateComponent and BlendStateComponent
        
        public D3DDepthStencilDefinition DepthStencilStateDefinition { get; private set; }

        public bool HasBlendState { get; private set; }
        public BlendStateDescription BlendStateDescription { get; private set; }


        public RasterizerStateDescription2 RasterizerStateDescription { get; private set; }

        RenderableComponent(CullMode cullMode, PrimitiveTopology primitiveTopology, RenderTechniques technique) : this() {
            CullMode = cullMode;
            PrimitiveTopology = primitiveTopology;
            Technique = technique;
            IsRenderable = true;
        }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsValid { get; private set; }
        public bool IsRenderable { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = false;
        }

        public RenderableComponent Disable() {
            Tag = ElementTag.New();
            IsRenderable = false;
            return this;
        }
        public RenderableComponent Enable() {
            Tag = ElementTag.New();
            IsRenderable = true;
            return this;
        }

        public RenderableComponent SwitchFillModeTo(FillMode mode) {
            Tag = ElementTag.New();
            IsRenderable = true;
            var rast = RasterizerStateDescription;
            rast.FillMode = mode;
            RasterizerStateDescription = rast;
            return this;
        }

        public RenderableComponent SwitchCullModeTo(CullMode mode) {
            Tag = ElementTag.New();
            IsRenderable = true;
            var rast = RasterizerStateDescription;
            rast.CullMode = mode;
            RasterizerStateDescription = rast;
            return this;
        }
    }
}
