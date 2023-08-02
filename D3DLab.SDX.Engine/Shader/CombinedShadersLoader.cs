﻿using System;
using System.Collections.Generic;
using System.IO;
using D3DLab.ECS.Shaders;
using D3DLab.ECS.Common;

namespace D3DLab.SDX.Engine.Shader {
    public class CombinedShadersLoader {
        const string entry = "main";
        readonly ManifestResourceLoader mloader;

        public CombinedShadersLoader() {
            mloader = new ManifestResourceLoader(this.GetType());
        }
        public CombinedShadersLoader(ManifestResourceLoader mloader) {
            this.mloader = mloader;
        }
       
        public IShaderInfo[] Load(string resource, string keyname) {
            var text = mloader.GetResourceTextByName(resource);

            return LoadText(text, keyname);
        }
        public IShaderInfo[] LoadText(string text, string keyname) {
            var res = new List<IShaderInfo>();
            var parts = text.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Length; ++i) {
                if (CreateInfo(res, keyname, parts, ShaderStages.Vertex.ToString().ToLower(), ref i)) { continue; }
                if (CreateInfo(res, keyname, parts, ShaderStages.Fragment.ToString().ToLower(), ref i)) { continue; }
                if (CreateInfo(res, keyname, parts, ShaderStages.Geometry.ToString().ToLower(), ref i)) { continue; }
            }
            return res.ToArray();
        }
        bool CreateInfo(List<IShaderInfo> infos, string keyname, string[] parts, string partName, ref int i) {
            var part = parts[i].Trim();
            if (part.Length == partName.Length && string.Compare(part, partName, true) == 0 && parts.Length > (i + 1)) {
                i++;
                var info = new ShaderInMemoryInfo($"{keyname}_{partName}Shader", parts[i].Trim(), null,
                    partName, entry);
                infos.Add(info);
                return true;
            }
            return false;
        }
    }
}
