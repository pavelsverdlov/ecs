using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DLab.ECS.Systems {
    public sealed class DefaultInputSystem : BaseEntitySystem, IGraphicSystem, IGraphicSystemContextDependent {
        public IContextState ContextState { get; set; }

        protected override void Executing(ISceneSnapshot snapshot) {
            var input = snapshot.InputSnapshot;

            foreach (var cmd in input.Events) {
                if (cmd.Execute(snapshot, ContextState)) {
                    input.RemoveEvent(cmd);
                }
            }
        }
    }
}
