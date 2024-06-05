using System.Collections.Generic;
using System.IO;

using D3DLab.FileFormats.GeoFormats;

namespace D3DLab.Plugin
{
    public class LoadedPlugin {
        public LoadedPlugin(ID3DLabPlugin plugin, FileInfo file) {
            Plugin = plugin;
            File = file;
        }

        public ID3DLabPlugin Plugin { get; }
        public FileInfo File { get; }

        public bool IsResourcesLoaded { get; set; }
    }

    public class LoadedFileFormatPlugin {
        public LoadedFileFormatPlugin(ID3DLabFileFormatPlugin plugin, FileInfo file) {
            Plugin = plugin;
            File = file;
        }

        public ID3DLabFileFormatPlugin Plugin { get; }
        public FileInfo File { get; }

        public IEnumerable<IFileGeometry3D> LoadGeometryFile(FileInfo path) {
            return Plugin.Load(path);
        }
    }
}