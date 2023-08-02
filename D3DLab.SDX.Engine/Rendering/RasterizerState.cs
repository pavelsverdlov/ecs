﻿using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.SDX.Engine.Rendering {
    /// <summary>
    /// wrapper over structure for posibility to change from online properties editor 
    /// </summary>
    public class D3DRasterizerState {
        public FillMode FillMode {
            get => state.FillMode;
            set {
                state.FillMode = value;
            }
        }
        public CullMode CullMode {
            get => state.CullMode;
            set {
                state.CullMode = value;
            }
        }
        public bool IsAntialiasedLineEnabled {
            get => state.IsAntialiasedLineEnabled;
            set {
                state.IsAntialiasedLineEnabled = value;
            }
        }
        public bool IsMultisampleEnabled {
            get => state.IsMultisampleEnabled;
            set {
                state.IsMultisampleEnabled = value;
            }
        }

        public ConservativeRasterizationMode ConservativeRasterizationMode {
            get => state.ConservativeRasterizationMode;
            set {
                state.ConservativeRasterizationMode = value;
            }
        }


        RasterizerStateDescription2 state;
        public D3DRasterizerState(RasterizerStateDescription2 state) {
            this.state = state;
        }
        public RasterizerStateDescription2 GetDescription() {
            //state.DepthBias = -10;
            //state.DepthBiasClamp = -1000;
            //state.SlopeScaledDepthBias = -1;


            return state;
        }
    }
}
