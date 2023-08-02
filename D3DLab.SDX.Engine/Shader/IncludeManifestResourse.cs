using D3DLab.ECS.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.SDX.Engine.Shader {
    public class IncludeManifestResourse : IIncludeResourse {
        readonly string path;
        public string Key { get; }

        readonly ManifestResourceLoader loader;
        public IncludeManifestResourse(string key, string path) {
            Key = key;
            this.path = path;
            loader = new ManifestResourceLoader(this.GetType());
        }
        public IncludeManifestResourse(string key, string path, ManifestResourceLoader loader) {
            Key = key;
            this.path = path;
            this.loader = loader;
        }

        public Stream GetResourceStream() {
            return loader.GetResourceStreamByName(path);
        }
    }
}
