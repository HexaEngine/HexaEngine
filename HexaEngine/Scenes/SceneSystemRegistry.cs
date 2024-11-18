﻿namespace HexaEngine.Scenes
{
    using HexaEngine.Audio;
    using HexaEngine.Core;
    using HexaEngine.Coroutines;
    using HexaEngine.Graphics;
    using HexaEngine.Input;
    using HexaEngine.Lights;
    using HexaEngine.Network;
    using HexaEngine.Physics;
    using HexaEngine.Queries;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Scenes.Systems;
    using HexaEngine.Scripts;
    using HexaEngine.UI;
    using HexaEngine.Volumes;
    using HexaEngine.Weather;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics.CodeAnalysis;

    public static class SceneSystemRegistry
    {
        private static readonly ServiceCollection descriptors = new();

        static SceneSystemRegistry()
        {
            bool graphicsDisabled = Application.GraphicsDisabled;
            if (!graphicsDisabled)
                descriptors.AddSingleton(Application.GraphicsDevice);
            descriptors.AddSingleton<ModelManager>();
            descriptors.AddSingleton<MaterialManager>();
            descriptors.AddSingleton<DrawLayerManager>();
            descriptors.AddSingleton<AnimationManager>();

            if (!graphicsDisabled)
                Register<UISystem>();

            Register<QuerySystem>();
            Register<TransformSystem>();
            Register<InputSystem>();
            Register<NetworkSystem>();
            Register<AudioSystem>();
            Register<AnimationSystem>();
            Register<ScriptManager>();
            if (!graphicsDisabled)
                Register<LightManager>();
            Register<PhysicsSystem>();
            if (!graphicsDisabled)
                Register<RenderManager>();
            Register<WeatherSystem>();
            Register<ObjectPickerManager>();
            if (!graphicsDisabled)
                Register<LODSystem>();
            if (!graphicsDisabled)
                Register<VolumeSystem>();
            Register<CoroutineSystem>();
        }

        public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>() where T : class, ISceneSystem
        {
            descriptors.AddSingleton<T>();
        }

        public static void RegisterOverwrite<TTarget, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSystem>() where TSystem : class, ISceneSystem where TTarget : class, ISceneSystem
        {
            for (int i = 0; i < descriptors.Count; i++)
            {
                var descriptor = descriptors[i];
                if (descriptor.ImplementationType == typeof(TTarget))
                {
                    descriptors.RemoveAt(i);
                    break;
                }
            }

            descriptors.AddSingleton<ISceneSystem, TSystem>();
        }

        public static void Remove<TTarget>() where TTarget : class, ISceneSystem
        {
            for (int i = 0; i < descriptors.Count; i++)
            {
                var descriptor = descriptors[i];
                if (descriptor.ImplementationType == typeof(TTarget))
                {
                    descriptors.RemoveAt(i);
                    break;
                }
            }
        }

        public static void RemoveAt(int index)
        {
            descriptors.RemoveAt(index);
        }

        public static void Clear()
        {
            descriptors.Clear();
        }

        public static ServiceCollection GetDescriptors() => descriptors;

        public static ServiceCollection GetServices(Scene scene)
        {
            ServiceCollection services = new();
            services.AddSingleton(scene);
            for (int i = 0; i < descriptors.Count; i++)
            {
                ((ICollection<ServiceDescriptor>)services).Add(descriptors[i]);
                /*
                var descriptor = descriptors[i];
                if (descriptor.ImplementationInstance != null)
                {
                    services.AddSingleton(descriptors[i].ServiceType, descriptor.ImplementationInstance);
                }
                else
                {
                    services.AddSingleton(descriptors[i].ServiceType, descriptors[i].ImplementationType ?? descriptors[i].ServiceType);
                }*/
            }
            return services;
        }

        public static IEnumerable<T> GetAllSystems<T>(this IServiceProvider provider, IServiceCollection descriptors) where T : class
        {
            var type = typeof(T);
            foreach (ServiceDescriptor descriptor in descriptors)
            {
                if (descriptor.ServiceType.IsAssignableTo(type))
                {
                    yield return (T)provider.GetRequiredService(descriptor.ServiceType);
                }
            }
        }
    }
}