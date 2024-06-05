using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

using McMaster.NETCore.Plugins;

namespace D3DLab.Plugin {
    public class PluginProxy {
        public List<LoadedPlugin> Plugins { get; }
        public List<LoadedFileFormatPlugin> FileFormatPlugins { get; }

        readonly string directory;
        readonly string pluginFormatName;
        const string dll = ".dll";
        const string nameFormat = "*Plugin*";

        public PluginProxy(string directory, string pluginFormatName = nameFormat) {
            Plugins = new List<LoadedPlugin>();
            FileFormatPlugins = new List<LoadedFileFormatPlugin>();
            this.directory = directory;
            this.pluginFormatName = pluginFormatName;
        }

        public void Load() {
            Plugins.Clear();

            var types = new[]{
                typeof(ID3DLabPlugin),
                typeof(ID3DLabFileFormatPlugin),
            };
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

                        var loadedTypes = loader.LoadDefaultAssembly().GetTypes();

                        foreach (var pluginType in loadedTypes.Where(t => types.Any( x=> x.IsAssignableFrom(t)) && !t.IsAbstract)) {
                            var instance = Activator.CreateInstance(pluginType);
                            var path = new FileInfo(pluginDll);

                            if (instance is ID3DLabPlugin plugin) {
                                Plugins.Add(new LoadedPlugin(plugin, path));
                            } 
                            if(instance is ID3DLabFileFormatPlugin ffplugin) {
                                FileFormatPlugins.Add(new LoadedFileFormatPlugin(ffplugin, path));
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