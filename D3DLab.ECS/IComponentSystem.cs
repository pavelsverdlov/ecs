

using System;

namespace D3DLab.ECS {
    internal interface IComponentSystemIncrementId {
        int ID { set; }
    }

    /// <summary>
    /// setting context is happened implicitly in system manager while creating system
    /// </summary>
    /// <remarks>
    /// DO NOT PUSH SETTING TO ContextState MANUALLY!!!
    /// </remarks>
    public interface IGraphicSystemContextDependent {
        IContextState ContextState { set; }
    }

    public interface IGraphicSystem : IDisposable {
        int ID { get; }
        TimeSpan ExecutionTime { get; }
        void Execute(ISceneSnapshot snapshot);        
    }
}
