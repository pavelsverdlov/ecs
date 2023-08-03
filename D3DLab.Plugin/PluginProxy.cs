using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

using McMaster.NETCore.Plugins;

namespace D3DLab.Plugin {
    public class PluginProxy {
        public List<LoadedPlugin> Plugins { get; }

        readonly string directory;
        readonly string pluginFormatName;
        const string dll = ".dll";
        const string nameFormat = "*Plugin*";

        public PluginProxy(string directory, string pluginFormatName = nameFormat) {
            Plugins = new List<LoadedPlugin>();
            this.directory = directory;
            this.pluginFormatName = pluginFormatName;
        }

        public void Load() {
            Plugins.Clear();

            var type = typeof(IPlugin);
            foreach (var pluginDll in Directory.GetFiles(directory, pluginFormatName, SearchOption.AllDirectories)) {
                if (pluginDll.EndsWith(dll)) {
                    try {
                        var loader = PluginLoader.CreateFromAssemblyFile(
                            assemblyFile: pluginDll,
                            isUnloadable: false,
                            sharedTypes: new Type[] {
                               // type 
                            },
                            configure: config => config.DefaultContext = AssemblyLoadContext.Default);

                        var types = loader.LoadDefaultAssembly().GetTypes();

                        foreach (var pluginType in types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract)) {
                            if (Activator.CreateInstance(pluginType) is IPlugin plugin) {
                                Plugins.Add(new LoadedPlugin(plugin, new FileInfo(pluginDll)));
                            }
                        }

                        loader.Dispose();
                    } catch (Exception ex){
                        //ignore 
                    }
                }
            }

        }
    }
}