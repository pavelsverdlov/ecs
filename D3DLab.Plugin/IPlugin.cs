using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using D3DLab.FileFormats.GeoFormats;

namespace D3DLab.Plugin
{
    public interface ID3DLabPlugin {
        string Name { get; }
        string Description { get; }

        Task ExecuteAsWindowAsync(IPluginContext context);
        IPluginViewModel ExecuteAsComponent(IPluginContext context);
        Task CloseAsync();
        void LoadResources(IPluginContext context);
    }

    public interface ID3DLabFileFormatPlugin {
        string Name { get; }
        string Description { get; }
        IEnumerable<IFileGeometry3D> Load(FileInfo path);
    }
}