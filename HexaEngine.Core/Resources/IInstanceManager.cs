namespace HexaEngine.Core.Resources
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

        ModelInstance CreateInstance(Model model, GameObject parent);

        Task<ModelInstance> CreateInstanceAsync(Mesh mesh, Material material, GameObject parent);

        Task<ModelInstance> CreateInstanceAsync(Model model, GameObject parent);

        void DestroyInstance(ModelInstance instance);

        Task DestroyInstanceAsync(ModelInstance instance);
    }
}