﻿namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
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

        public event Action<ModelInstance>? OnInstanceCreated;

        public event Action<ModelInstance>? OnInstanceDestroyed;

        public event Action<ModelInstanceType>? OnTypeCreated;

        public event Action<ModelInstanceType>? OnTypeDestroyed;

        public static InstanceManager? Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => SceneManager.Current?.InstanceManager; }

        public unsafe InstanceManager(IGraphicsDevice device)
        {
            this.device = device;
        }

        public ModelInstance CreateInstance(Model model, GameObject parent)
        {
            var material = ResourceManager.LoadMaterial(model.Material);
            var mesh = ResourceManager.LoadMesh(model.Mesh);
            var instance = CreateInstance(mesh, material, parent);
            return instance;
        }

        public async Task<ModelInstance> CreateInstanceAsync(Model model, GameObject parent)
        {
            var material = await ResourceManager.LoadMaterialAsync(model.Material);
            var mesh = await ResourceManager.LoadMeshAsync(model.Mesh);
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
                    OnTypeCreated?.Invoke(type);
                }
            }

            ModelInstance instance;
            lock (instances)
            {
                instance = type.CreateInstance(device, parent);
                if (!instances.Contains(instance))
                {
                    instances.Add(instance);
                    OnInstanceCreated?.Invoke(instance);
                }
            }
#if VERBOSE
            Debug.WriteLine(instance.ToString());
#endif
            semaphore.Release();
            return instance;
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
                    OnTypeCreated?.Invoke(type);
                }
            }

            ModelInstance instance;
            lock (instances)
            {
                instance = type.CreateInstance(device, parent);
                if (!instances.Contains(instance))
                {
                    instances.Add(instance);
                    OnInstanceCreated?.Invoke(instance);
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

            type.DestroyInstance(device, instance);
            OnInstanceDestroyed?.Invoke(instance);

            if (type.IsEmpty)
            {
                var mesh = type.Mesh;
                types.Remove(type);
                mesh.DestroyInstanceType(type);
                OnTypeDestroyed?.Invoke(type);

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

            type.DestroyInstance(device, instance);
            OnInstanceDestroyed?.Invoke(instance);

            if (type.IsEmpty)
            {
                var mesh = type.Mesh;
                types.Remove(type);
                mesh.DestroyInstanceType(type);
                OnTypeDestroyed?.Invoke(type);

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