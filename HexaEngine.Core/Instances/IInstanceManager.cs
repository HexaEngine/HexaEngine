using HexaEngine.Core.Resources;

namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IInstanceManager
    {
        int InstanceCount { get; }

        IReadOnlyList<ModelInstance> Instances { get; }

        int TypeCount { get; }

        IReadOnlyList<ModelInstanceType> Types { get; }

        event Action<ModelInstance>? InstanceCreated;

        event Action<ModelInstance>? InstanceDestroyed;

        event Action<ModelInstanceType>? TypeCreated;

        event Action<ModelInstanceType>? TypeDestroyed;

        event Action<ModelInstanceType, ModelInstance>? Updated;

        ModelInstance CreateInstance(Mesh mesh, Material material, GameObject parent);

        ModelInstance CreateInstance(string model, GameObject parent);

        Task<ModelInstance> CreateInstanceAsync(Mesh mesh, Material material, GameObject parent);

        Task<ModelInstance> CreateInstanceAsync(string model, GameObject parent);

        void DestroyInstance(ModelInstance instance);

        Task DestroyInstanceAsync(ModelInstance instance);
    }
}