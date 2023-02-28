namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Culling;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using System;
    using System.Runtime.CompilerServices;

    public class InstanceManager : IInstanceManager
    {
        private readonly IGraphicsDevice device;

        private readonly List<ModelInstanceType> types = new();
        private readonly List<ModelInstance> instances = new();
        private readonly SemaphoreSlim semaphore = new(1);

        public IReadOnlyList<ModelInstance> Instances => instances;

        public IReadOnlyList<ModelInstanceType> Types => types;

        public int InstanceCount => instances.Count;

        public int TypeCount => types.Count;

        public event Action<ModelInstance>? InstanceCreated;

        public event Action<ModelInstance>? InstanceDestroyed;

        public event Action<ModelInstanceType>? TypeCreated;

        public event Action<ModelInstanceType>? TypeDestroyed;

        public event Action<ModelInstanceType, ModelInstance>? Updated;

        public static InstanceManager? Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => SceneManager.Current?.InstanceManager; }

        public unsafe InstanceManager(IGraphicsDevice device)
        {
            this.device = device;
        }

        public ModelInstance CreateInstance(string path, GameObject parent)
        {
            var model = MeshSource.Load(path);
            var material = ResourceManager.LoadMaterial(parent.GetScene().MaterialManager.TryAddMaterial(model.GetMaterial()));
            var mesh = ResourceManager.LoadMesh(model);
            var instance = CreateInstance(mesh, material, parent);
            return instance;
        }

        public async Task<ModelInstance> CreateInstanceAsync(string path, GameObject parent)
        {
            var model = MeshSource.Load(path);
            var material = await ResourceManager.LoadMaterialAsync(parent.GetScene().MaterialManager.TryAddMaterial(model.GetMaterial()));
            var mesh = await ResourceManager.LoadMeshAsync(model);
            var instance = await CreateInstanceAsync(mesh, material, parent);
            return instance;
        }

        public ModelInstance CreateInstance(Mesh mesh, Material material, GameObject parent)
        {
            semaphore.Wait();
            ModelInstanceType type;
            lock (types)
            {
                type = mesh.CreateInstanceType(device, CullingManager.DrawIndirectArgs, CullingManager.InstanceDataOutBuffer, CullingManager.InstanceOffsets, CullingManager.InstanceDataNoCull, CullingManager.InstanceOffsetsNoCull, material);
                if (!types.Contains(type))
                {
                    types.Add(type);
                    type.Updated += TypeUpdated;
                    TypeCreated?.Invoke(type);
                }
            }

            ModelInstance instance;
            lock (instances)
            {
                instance = type.CreateInstance(parent);
                if (!instances.Contains(instance))
                {
                    instances.Add(instance);
                    InstanceCreated?.Invoke(instance);
                }
            }
#if VERBOSE
            Debug.WriteLine(instance.ToString());
#endif
            semaphore.Release();
            return instance;
        }

        private void TypeUpdated(ModelInstanceType arg1, ModelInstance arg2)
        {
            Updated?.Invoke(arg1, arg2);
        }

        public async Task<ModelInstance> CreateInstanceAsync(Mesh mesh, Material material, GameObject parent)
        {
            await semaphore.WaitAsync();
            ModelInstanceType type;
            lock (types)
            {
                type = mesh.CreateInstanceType(device, CullingManager.DrawIndirectArgs, CullingManager.InstanceDataOutBuffer, CullingManager.InstanceOffsets, CullingManager.InstanceDataNoCull, CullingManager.InstanceOffsetsNoCull, material);
                if (!types.Contains(type))
                {
                    types.Add(type);
                    TypeCreated?.Invoke(type);
                }
            }

            ModelInstance instance;
            lock (instances)
            {
                instance = type.CreateInstance(parent);
                if (!instances.Contains(instance))
                {
                    instances.Add(instance);
                    InstanceCreated?.Invoke(instance);
                }
            }
#if VERBOSE
            Debug.WriteLine(instance.ToString());
#endif
            semaphore.Release();
            return instance;
        }

        public void DestroyInstance(ModelInstance instance)
        {
            semaphore.Wait();

            var type = instance.Type;

            lock (types)
            {
                if (!types.Contains(type))
                {
                    throw new InvalidOperationException();
                }
            }

            lock (instances)
            {
                if (!instances.Contains(instance))
                {
                    throw new InvalidOperationException();
                }
                instances.Remove(instance);
            }

            type.DestroyInstance(instance);
            InstanceDestroyed?.Invoke(instance);

            if (type.IsEmpty)
            {
                var mesh = type.Mesh;
                type.Updated -= TypeUpdated;
                types.Remove(type);
                mesh.DestroyInstanceType(type);
                TypeDestroyed?.Invoke(type);

                if (!mesh.IsUsed)
                {
                    ResourceManager.UnloadMesh(mesh);
                    ResourceManager.UnloadMaterial(type.Material);
                }
            }

            semaphore.Release();
        }

        public async Task DestroyInstanceAsync(ModelInstance instance)
        {
            await semaphore.WaitAsync();

            var type = instance.Type;

            lock (types)
            {
                if (!types.Contains(type))
                {
                    throw new InvalidOperationException();
                }
            }

            lock (instances)
            {
                if (!instances.Contains(instance))
                {
                    throw new InvalidOperationException();
                }
                instances.Remove(instance);
            }

            type.DestroyInstance(instance);
            InstanceDestroyed?.Invoke(instance);

            if (type.IsEmpty)
            {
                var mesh = type.Mesh;
                types.Remove(type);
                mesh.DestroyInstanceType(type);
                TypeDestroyed?.Invoke(type);

                if (!mesh.IsUsed)
                {
                    ResourceManager.UnloadMesh(mesh);
                    ResourceManager.UnloadMaterial(type.Material);
                }
            }

            semaphore.Release();
        }
    }
}