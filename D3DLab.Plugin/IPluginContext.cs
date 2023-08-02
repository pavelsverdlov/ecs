using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace D3DLab.Plugin
{

    public class PluginObservableCollection : IEnumerable<IPluginLoadedObjectDetails> {

        public event Action<IPluginLoadedObjectDetails> Added;
        public event Action<IPluginLoadedObjectDetails> Removed;

        readonly Dictionary<Guid,IPluginLoadedObjectDetails> items;

        public PluginObservableCollection() {
            items = new Dictionary<Guid,IPluginLoadedObjectDetails> ();
        }

        public void Add(IPluginLoadedObjectDetails item) {
            Added?.Invoke(item);
            items.Add(item.ID, item);
        }
        public void Remove(IPluginLoadedObjectDetails item) {
            Removed?.Invoke(items[item.ID]);
            items.Remove(item.ID);
        }

        public IEnumerator<IPluginLoadedObjectDetails> GetEnumerator() 
            => items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
            => items.Values.GetEnumerator();
    }

    public interface IPluginContext {
        public DirectoryInfo PluginDirectory { get; }
      
        public IPluginScene Scene { get; }
        public PluginObservableCollection Collection { get; }

        void AddResource(ResourceDictionary resource);
    }
}