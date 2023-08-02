using System.Threading.Tasks;

namespace D3DLab.Plugin
{
    public interface IPlugin {
        string Name { get; }
        string Description { get; }

        Task ExecuteAsWindowAsync(IPluginContext context);
        IPluginViewModel ExecuteAsComponent(IPluginContext context);
        Task CloseAsync();
        void LoadResources(IPluginContext context);
    }
}