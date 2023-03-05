namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IInstanceManager
    {
        int InstanceCount { get; }

        IReadOnlyList<IInstance> Instances { get; }

        int TypeCount { get; }

        IReadOnlyList<IInstanceType> Types { get; }

        event Action<IInstance>? InstanceCreated;

        event Action<IInstance>? InstanceDestroyed;

        event Action<IInstanceType>? TypeCreated;

        event Action<IInstanceType>? TypeDestroyed;

        event Action<IInstanceType, IInstance>? Updated;

        IInstance CreateInstance(IInstanceType type, GameObject parent);

        Task<IInstance> CreateInstanceAsync(IInstanceType type, GameObject parent);

        void DestroyInstance(IInstance instance);

        Task DestroyInstanceAsync(IInstance instance);
    }
}