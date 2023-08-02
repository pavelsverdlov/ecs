using System;
using System.Collections.Generic;
using System.IO;
using D3DLab.ECS;

namespace D3DLab.Plugin
{
    public interface IPluginLoadedObjectDetails {
        Guid ID { get; }
        public FileInfo FilePath { get; }
        public IEnumerable<ElementTag> VisualObjectTags { get; }
    }
}