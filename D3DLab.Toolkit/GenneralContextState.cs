using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.ECS.Sync;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    public class GenneralContextState : BaseContextState {
        public static GenneralContextState Full(ContextStateProcessor processor, AxisAlignedBox octreeBounds, 
            EngineNotificator notificator, ILabLogger logger) {
            var syncContext = new RenderLoopSynchronizationContext();
            var octree = new OctreeManager(processor, octreeBounds, 5, syncContext);
            notificator.Subscribe(octree);
            var geoPool = new GeometryPool(notificator, syncContext);
            notificator.Subscribe(geoPool);

            return new GenneralContextState(processor, octree, geoPool, notificator, syncContext, logger);
        }

        GenneralContextState(ContextStateProcessor processor, IOctreeManager octree, 
            IGeometryMemoryPool geoPool, EngineNotificator notificator, 
            RenderLoopSynchronizationContext context, ILabLogger logger)
            : base(processor, new ManagerContainer(notificator, octree, processor, geoPool, context, logger)) {
        }
    }
}
