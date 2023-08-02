using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3DLab.ECS;

namespace D3DLab.Plugin {
    public interface IPluginGraphicSystem : INestedGraphicSystem, IGraphicSystem {

    }

    public class PluginContainerSystem : ContainerSystem<IPluginGraphicSystem>, IGraphicSystem {
        protected override void Executing(ISceneSnapshot snapshot) {
            Synchronize();

            foreach(var nest in nested) {
                nest.Execute(snapshot);
            }
        }
    }
}
