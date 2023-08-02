using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine.ProxyDevice {
    sealed class AdapterFactory {
        public static event Func<Adapter[], int, Adapter> SelectAdapter;

        public static Adapter GetBestAdapter(global::SharpDX.DXGI.Factory f) {
            Adapter bestAdapter = null;
            var bestLevel = global::SharpDX.Direct3D.FeatureLevel.Level_11_1;

            var selectedId = -1;
            for (int i = 0; i < f.Adapters.Length; i++) {
                Adapter adapter = f.Adapters[i];
                var level = global::SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(adapter);
                if (bestAdapter == null || level > bestLevel) {
                    selectedId = adapter.Description.DeviceId;
                    bestAdapter = adapter;
                    bestLevel = level;
                    break;
                }
            }

            if (SelectAdapter != null) {
                bestAdapter = SelectAdapter(f.Adapters, selectedId) ?? bestAdapter;
            }

            return bestAdapter;
        }
    }
}
