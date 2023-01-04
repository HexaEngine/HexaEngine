namespace HexaEngine.Resources
{
    using HexaEngine.Mathematics;
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

        ModelInstance CreateInstance(Mesh mesh, Material material, Transform transform);

        ModelInstance CreateInstance(Model model, Transform transform);

        Task<ModelInstance> CreateInstanceAsync(Mesh mesh, Material material, Transform transform);

        Task<ModelInstance> CreateInstanceAsync(Model model, Transform transform);

        void DestroyInstance(ModelInstance instance);

        Task DestroyInstanceAsync(ModelInstance instance);
    }
}