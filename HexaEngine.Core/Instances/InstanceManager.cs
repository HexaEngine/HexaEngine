namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using System;
    using System.Runtime.CompilerServices;

    public class InstanceManager : IInstanceManager
    {
        private readonly Func<object?, IInstance> CreateInstanceDelegate;
        private readonly Action<object?> DestroyInstanceDelegate;

        private readonly List<IInstanceType> types = new();
        private readonly List<IInstance> instances = new();

        public IReadOnlyList<IInstance> Instances => instances;

        public IReadOnlyList<IInstanceType> Types => types;

        public int InstanceCount => instances.Count;

        public int TypeCount => types.Count;

        public event Action<IInstance>? InstanceCreated;

        public event Action<IInstance>? InstanceDestroyed;

        public event Action<IInstanceType>? TypeCreated;

        public event Action<IInstanceType>? TypeDestroyed;

        public event Action<IInstanceType, IInstance>? Updated;

        public static InstanceManager? Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => SceneManager.Current?.InstanceManager; }

        public unsafe InstanceManager()
        {
            CreateInstanceDelegate = CreateInstanceAsyncInternal;
            DestroyInstanceDelegate = DestroyInstanceAsyncInternal;
        }

        public IInstance CreateInstance(IInstanceType type, GameObject parent)
        {
            lock (types)
            {
                if (!types.Contains(type))
                {
                    types.Add(type);
                    type.Updated += TypeUpdated;
                    TypeCreated?.Invoke(type);
                }
            }

            IInstance instance;
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

            return instance;
        }

        private void TypeUpdated(IInstanceType arg1, IInstance arg2)
        {
            Updated?.Invoke(arg1, arg2);
        }

        public Task<IInstance> CreateInstanceAsync(IInstanceType type, GameObject parent)
        {
            Tuple<InstanceManager, IInstanceType, GameObject> state = new(this, type, parent);
            return Task.Factory.StartNew(CreateInstanceDelegate, state);
        }

        private static IInstance CreateInstanceAsyncInternal(object? state)
        {
#nullable disable
            Tuple<InstanceManager, IInstanceType, GameObject> args = (Tuple<InstanceManager, IInstanceType, GameObject>)state;
            return args.Item1.CreateInstance(args.Item2, args.Item3);
#nullable enable
        }

        public void DestroyInstance(IInstance instance)
        {
            var type = instance.Type;

            lock (types)
            {
                if (!types.Contains(type))
                {
                    throw new InvalidOperationException("The instance don't exist");
                }
            }

            lock (instances)
            {
                if (!instances.Contains(instance))
                {
                    throw new InvalidOperationException("The instance don't exist");
                }
                instances.Remove(instance);
            }

            type.DestroyInstance(instance);
            InstanceDestroyed?.Invoke(instance);

            if (type.IsEmpty)
            {
                type.Updated -= TypeUpdated;
                types.Remove(type);
                type.Dispose();
                TypeDestroyed?.Invoke(type);
            }
        }

        public Task DestroyInstanceAsync(IInstance instance)
        {
            Tuple<InstanceManager, IInstance> state = new(this, instance);
            return Task.Factory.StartNew(DestroyInstanceDelegate, state);
        }

        private static void DestroyInstanceAsyncInternal(object? state)
        {
#nullable disable
            Tuple<InstanceManager, IInstance> args = (Tuple<InstanceManager, IInstance>)state;
            args.Item1.DestroyInstance(args.Item2);
#nullable enable
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            for (int i = 0; i < types.Count; i++)
            {
                types[i].DrawDepth(context, camera);
            }
        }

        public void Draw(IGraphicsContext context, IBuffer camera)
        {
            for (int i = 0; i < types.Count; i++)
            {
                if (types[i].Forward)
                    continue;
                types[i].Draw(context, camera);
            }
        }
    }
}