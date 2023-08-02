using System.IO;

namespace D3DLab.Plugin
{
    public class LoadedPlugin {
        public LoadedPlugin(IPlugin plugin, FileInfo file) {
            Plugin = plugin;
            File = file;
        }

        public IPlugin Plugin { get; }
        public FileInfo File { get; }

        public bool IsResourcesLoaded { get; set; }
    }
}