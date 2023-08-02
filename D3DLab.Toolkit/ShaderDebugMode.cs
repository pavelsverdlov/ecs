using D3DLab.SDX.Engine.Shader;
using System;
using System.IO;

namespace D3DLab.Toolkit {
    class ShaderDebugMode {
        readonly DirectoryInfo dir;
        readonly FileSystemWatcher watcher;
        readonly D3DShaderTechniquePass pass;

        public ShaderDebugMode(DirectoryInfo dir, D3DShaderTechniquePass pass) {
            this.dir = dir;
            this.pass = pass;
            if (!dir.Exists) {
                dir.Create();
            }
            watcher = new FileSystemWatcher(dir.FullName, "*.hlsl");
        }
        public void Activate() {
            foreach (var info in pass.ShaderInfos) {
                File.WriteAllText(Path.Combine(dir.FullName, $"{info.Name}{info.Stage}.hlsl"), info.ReadText());
            }
            watcher.EnableRaisingEvents = true;
            watcher.Changed += OmDirectoryChanged;
            watcher.Renamed += OmDirectoryChanged;
            watcher.Deleted += OmDirectoryChanged;
            watcher.Created += OmDirectoryChanged;
        }

        void OmDirectoryChanged(object sender, FileSystemEventArgs e) {
            foreach (var info in pass.ShaderInfos) {
                if (e.Name == $"{info.Name}{info.Stage}.hlsl") {
                    try {
                        System.Threading.Thread.Sleep(200);
                        info.WriteText(File.ReadAllText(e.FullPath));
                        pass.ClearCache();
                    } catch (Exception ex) {

                    }
                }
            }
        }

        public void Deactivate() {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OmDirectoryChanged;
            Directory.Delete(dir.FullName, true);
        }
    }
}
