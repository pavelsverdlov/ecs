using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct3D11;

namespace D3DLab.SDX.Engine {
    public static class D3DBlendStateDescriptions {
        public static BlendStateDescription BlendStateDisabled {
            get {
                var blendStateDesc = new BlendStateDescription();
                blendStateDesc.RenderTarget[0].IsBlendEnabled = false;
                blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.One;
                blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
                blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                return blendStateDesc;
            }
        }
        public static BlendStateDescription BlendStateEnabled {
            get {
                var blendStateDesc = new BlendStateDescription();

                //This should only be used for multi-sampling renderings.
               // blendStateDesc.AlphaToCoverageEnable = true;
               // blendStateDesc.IndependentBlendEnable = false;

                blendStateDesc.RenderTarget[0].IsBlendEnabled = true; // enable transparency
                blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;//SourceAlpha
                blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

                blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
                blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                
                return blendStateDesc;
            }
        }

    }

    public readonly struct D3DDepthStencilDefinition {
        public static D3DDepthStencilDefinition Default(int stencilRef = 0)
            => new D3DDepthStencilDefinition(D3DDepthStencilStateDescriptions.Default, stencilRef);

        public static D3DDepthStencilDefinition DepthDisabled
            => new D3DDepthStencilDefinition(D3DDepthStencilStateDescriptions.DepthDisabled, 0);

        public static D3DDepthStencilDefinition DepthEnabled
            => new D3DDepthStencilDefinition(D3DDepthStencilStateDescriptions.DepthEnabled, 0);



        public DepthStencilStateDescription Description { get; }
        public int StencilRef { get; }
        public bool IsValid { get; }

        public D3DDepthStencilDefinition(DepthStencilStateDescription description, int stencilRef = 0) {
            Description = description;
            StencilRef = stencilRef;
            IsValid = true;
        }
    }

    static class D3DDepthStencilStateDescriptions {

        public static DepthStencilStateDescription Default => DepthStencilStateDescription.Default();

        // Now create a second depth stencil state which turns off the Z buffer for 2D rendering.
        // The difference is that DepthEnable is set to false.
        // All other parameters are the same as the other depth stencil state.
        /// <summary>
        /// Correct overlap objects based on depth 
        /// </summary>
        public readonly static DepthStencilStateDescription DepthDisabled = new DepthStencilStateDescription {
            IsDepthEnabled = false, 
            DepthWriteMask = DepthWriteMask.All,
            DepthComparison = Comparison.Less,
            IsStencilEnabled = true,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            // Stencil operation if pixel front-facing.
            FrontFace = new DepthStencilOperationDescription() {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Increment,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            },
            // Stencil operation if pixel is back-facing.
            BackFace = new DepthStencilOperationDescription() {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Decrement,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            }
        };

        /// <summary>
        /// Overlap based on rendering order
        /// </summary>
        public readonly static DepthStencilStateDescription DepthEnabled = new DepthStencilStateDescription {
            // true - correct overlap objects based on depth 
            // false - overlap based on rendering order
            IsDepthEnabled = true,

            DepthWriteMask = DepthWriteMask.All,
            DepthComparison = Comparison.Less,
            IsStencilEnabled = true,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            // Stencil operation if pixel front-facing.
            FrontFace = new DepthStencilOperationDescription() {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Increment,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            },
            // Stencil operation if pixel is back-facing.
            BackFace = new DepthStencilOperationDescription() {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Decrement,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            }
        };

    }

    public static class D3DRasterizerStateDescriptions {
        public static RasterizerStateDescription2 Default(CullMode mode) =>
            new RasterizerStateDescription2() {
                CullMode = mode,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,

                IsFrontCounterClockwise = false,
                IsScissorEnabled = false,
                IsAntialiasedLineEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = .0f,
                SlopeScaledDepthBias = .0f
            };

        public static RasterizerStateDescription2 Lines = new RasterizerStateDescription2() {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            IsMultisampleEnabled = true,
            IsAntialiasedLineEnabled = true,
            IsFrontCounterClockwise = true
        };
    }
}
