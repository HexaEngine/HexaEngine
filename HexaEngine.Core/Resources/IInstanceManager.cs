namespace HexaEngine.Resources
{
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IInstanceManager
    {
        int InstanceCount { get; }

        IReadOnlyList<ModelInstance> Instances { get; }

        int TypeCount { get; }

        IReadOnlyList<ModelInstanceType> Types { get; }

        event Action<ModelInstance>? OnInstanceCreated;

        event Action<ModelInstance>? OnInstanceDestroyed;

        event Action<ModelInstanceType>? OnTypeCreated;

        event Action<ModelInstanceType>? OnTypeDestroyed;

        ModelInstance CreateInstance(Mesh mesh, Material material, GameObject parent);

        ModelInstance CreateInstance(Model model, GameObject parent);

        Task<ModelInstance> CreateInstanceAsync(Mesh mesh, Material material, GameObject parent);

        Task<ModelInstance> CreateInstanceAsync(Model model, GameObject parent);

        void DestroyInstance(ModelInstance instance);

        Task DestroyInstanceAsync(ModelInstance instance);
    }
}