namespace HexaEngine.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Physics;
    using HexaEngine.Queries;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Scenes.Systems;
    using HexaEngine.Volumes;
    using HexaEngine.Weather;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics.CodeAnalysis;

    public static class SceneSystemRegistry
    {
        private static readonly ServiceCollection descriptors = new();

        static SceneSystemRegistry()
        {
            descriptors.AddSingleton(Application.GraphicsDevice);
            descriptors.AddSingleton<ModelManager>();
            descriptors.AddSingleton<MaterialManager>();
            descriptors.AddSingleton<DrawLayerManager>();
            descriptors.AddSingleton<AnimationManager>();

            Register<QuerySystem>();
            Register<AudioSystem>();
            Register<AnimationSystem>();
            Register<ScriptManager>();
            Register<LightManager>();
            Register<PhysicsSystem>();
            Register<TransformSystem>();
            Register<RenderManager>();
            Register<WeatherSystem>();
            Register<ObjectPickerManager>();
            Register<LODSystem>();
            Register<VolumeSystem>();
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