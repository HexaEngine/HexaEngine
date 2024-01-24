namespace HexaEngine.PostFx
{
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using System.Diagnostics.CodeAnalysis;

    public class PostProcessingManager : IDisposable
    {
        private readonly object _lock = new();
        private readonly SemaphoreSlim semaphore = new(1);

        private ShaderMacro[] macros;
        private readonly PostProcessingContext postContext;
        private readonly List<IPostFx> effectsSorted = new();
        private readonly List<IPostFx> effects = [];
        private readonly IGraphicsContext deferredContext;
        private readonly ConfigKey config;
        private readonly IGraphicsDevice device;
        private readonly GraphResourceBuilder creator;
        private readonly PostFxGraph graph = new();
        private readonly bool debug;
        private readonly bool forceDynamic;
        private int width;
        private int height;

        private List<PostProcessingExecutionGroup> groups = new();

        private bool isDirty;
        private bool isInitialized = false;
        private volatile bool isReloading;

        private bool enabled;
        private bool disposedValue;

        public PostProcessingManager(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, int bufferCount = 4, bool debug = false, bool forceDynamic = true)
        {
            config = Config.Global.GetOrCreateKey("Post Processing");
            this.device = device;
            this.creator = creator;
            this.debug = debug;
            this.forceDynamic = forceDynamic;
            postContext = new(device, Format.R16G16B16A16Float, width, height, bufferCount);

            deferredContext = device.CreateDeferredContext();
            macros = Array.Empty<ShaderMacro>();
        }

        public Texture2D Input { get => postContext.Input; set => postContext.Input = value; }

        public IRenderTargetView Output { get => postContext.Output; set => postContext.Output = value; }

        public ITexture2D OutputTex { get => postContext.OutputTex; set => postContext.OutputTex = value; }

        public Viewport Viewport
        {
            get => postContext.OutputViewport;
            set
            {
                if (postContext.OutputViewport == value)
                {
                    return;
                }

                postContext.OutputViewport = value;
                Invalidate();
            }
        }

        public int Count => effectsSorted.Count;

        public IReadOnlyList<IPostFx> Effects => effectsSorted;

        public bool Enabled { get => enabled; set => enabled = value; }

        public bool Debug => debug;

        public void Initialize(int width, int height, ICPUProfiler? profiler)
        {
            semaphore.Wait();

            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnEnabledChanged += OnEnabledChanged;
                effects[i].OnReload += OnReload;
                effects[i].PropertyChanged += PropertyChanged;
            }

            this.width = width;
            this.height = height;

            Sort();

            macros = new ShaderMacro[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Enabled)
                {
                    effects[i].Initialize(device, creator, width, height, macros);
                }
            }

            UpdateGroups();

            isInitialized = true;

            semaphore.Release();
        }

        private void OnEnabledChanged(IPostFx postFx, bool enabled)
        {
            if (postFx.Flags.HasFlag(PostFxFlags.Optional))
            {
                return;
            }
            ReloadAsync();
        }

        private void OnReload(IPostFx postFx)
        {
            semaphore.Wait();
            postFx.Dispose();
            postFx.Initialize(device, creator, width, height, macros);
            Invalidate();
            semaphore.Release();
        }

        private void PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            semaphore.Wait();
            Invalidate();
            semaphore.Release();
        }

        public void Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : class, IPostFx, new()
        {
            Add(new T());
        }

        public void Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T effect) where T : class, IPostFx
        {
            lock (effects)
            {
                graph.Add(effect);
                effects.Add(effect);
            }

            config.GenerateSubKeyAuto(effect, effect.Name);

            if (isInitialized)
            {
                effect.OnEnabledChanged += OnEnabledChanged;
                effect.OnReload += OnReload;
                effect.PropertyChanged += PropertyChanged;
                Reload();
            }
        }

        public void Remove(IPostFx effect)
        {
            lock (effects)
            {
                graph.Remove(effect);
                effects.Remove(effect);
            }

            if (isInitialized)
            {
                effect.OnEnabledChanged -= OnEnabledChanged;
                effect.OnReload -= OnReload;
                effect.PropertyChanged -= PropertyChanged;
                Reload();
            }
        }

        public void Invalidate()
        {
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].Invalidate();
            }
            isDirty = true;
        }

        #region Getter

        public IPostFx? GetByName(string name)
        {
            lock (effectsSorted)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (effect.Name == name)
                    {
                        return effect;
                    }
                }
            }
            return null;
        }

        public T? GetByName<T>(string name) where T : class, IPostFx
        {
            lock (effectsSorted)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (effect is T t && effect.Name == name)
                    {
                        return t;
                    }
                }
            }
            return null;
        }

        public T? GetByType<T>() where T : class, IPostFx
        {
            lock (effectsSorted)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (effect is T t)
                    {
                        return t;
                    }
                }
            }
            return null;
        }

        public bool TryGetByName(string name, [NotNullWhen(true)] out IPostFx? effect)
        {
            effect = GetByName(name);
            return effect != null;
        }

        public bool TryGetByName<T>(string name, [NotNullWhen(true)] out T? effect) where T : class, IPostFx
        {
            effect = GetByName<T>(name);
            return effect != null;
        }

        public bool TryGetByType<T>([NotNullWhen(true)] out T? effect) where T : class, IPostFx
        {
            effect = GetByType<T>();
            return effect != null;
        }

        #endregion Getter

        public void Reload()
        {
            semaphore.Wait();

            isReloading = true;

            macros = new ShaderMacro[effects.Count];

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect.Initialized)
                {
                    effect.Dispose();
                }
            }

            Sort();

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            Parallel.For(0, effects.Count, i =>
            {
                var effect = effects[i];
                if (effect.Enabled)
                {
                    effect.Initialize(device, creator, width, height, macros);
                }
            });

            UpdateGroups();

            isReloading = false;

            semaphore.Release();
        }

        public Task ReloadAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                semaphore.Wait();

                isReloading = true;

                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    if (effect.Initialized)
                    {
                        effect.Dispose();
                    }
                }

                Sort();

                macros = new ShaderMacro[effects.Count];

                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
                }

                Parallel.For(0, effects.Count, i =>
                {
                    var effect = effects[i];
                    if (effect.Enabled)
                    {
                        effect.Initialize(device, creator, width, height, macros);
                    }
                });

                UpdateGroups();

                isReloading = false;

                semaphore.Release();
            });
        }

        public void BeginResize()
        {
            semaphore.Wait();
            Invalidate();
        }

        public void EndResize(int width, int height)
        {
            this.width = width;
            this.height = height;
            if (!isInitialized)
            {
                return;
            }

            for (int i = 0; i < effectsSorted.Count; i++)
            {
                if (effects[i].Enabled)
                    effectsSorted[i].Resize(width, height);
            }

            postContext.Resize(device, width, height);

            Invalidate();
            semaphore.Release();
        }

        public void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (!enabled || isReloading)
            {
                return;
            }

            lock (_lock)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (!effect.Enabled || (effect.Flags & PostFxFlags.PrePass) == 0 || (effect.Flags & PostFxFlags.ComposeTarget) != 0)
                    {
                        continue;
                    }
                    effect.Update(context);
                }

                context.ClearState();
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (!effect.Enabled || (effect.Flags & PostFxFlags.PrePass) == 0)
                    {
                        continue;
                    }

                    effect.PrePassDraw(context, creator);
                }
            }
        }

        public void Draw(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            if (!enabled || isReloading)
            {
                postContext.CopyInputToOutput(context);
                return;
            }

            if (!postContext.CanDraw)
            {
                return;
            }

            lock (_lock)
            {
                postContext.Clear(context);

                if (isDirty)
                {
                    postContext.Reset();
                    for (int i = 0; i < groups.Count; i++)
                    {
                        groups[i].SetupInputOutputs(postContext);
                    }
                    isDirty = false;
                }

                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (!effect.Enabled || (effect.Flags & PostFxFlags.PrePass) != 0 || (effect.Flags & PostFxFlags.ComposeTarget) != 0)
                    {
                        continue;
                    }
                    profiler?.Begin(effect.Name);
                    effect.Update(context);
                    profiler?.End(effect.Name);
                }

                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].Execute(context, deferredContext, creator);
                }
            }
        }

        private void Sort()
        {
            lock (_lock)
            {
                graph.Build(effectsSorted);
            }
        }

        private void UpdateGroups()
        {
            lock (_lock)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].Dispose();
                }
                groups.Clear();
            }

            if (effectsSorted.Count == 0)
            {
                return;
            }

            lock (_lock)
            {
                IPostFx first = effectsSorted[0];
                PostProcessingExecutionGroup group = new(first.Flags.HasFlag(PostFxFlags.Dynamic) || forceDynamic, false);
                group.Passes.Add(first);
                groups.Add(group);
                for (int i = 1; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    var isDynamic = effect.Flags.HasFlag(PostFxFlags.Dynamic) || forceDynamic;

                    if (group.IsDynamic != isDynamic)
                    {
                        var tmp = group;
                        group = new(isDynamic, false);
                        tmp.Next = group;
                        groups.Add(group);
                    }

                    group.Passes.Add(effect);
                }

                var last = groups[^1];
                last.IsLast = true;
                groups[^1] = last;
            }

            Invalidate();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    if (effect.Initialized)
                    {
                        effect.Dispose();
                    }

                    effect.OnEnabledChanged -= OnEnabledChanged;
                    effect.OnReload -= OnReload;
                    effect.PropertyChanged -= PropertyChanged;
                }
                effects.Clear();
                effectsSorted.Clear();
                graph.Clear();

                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].Dispose();
                }
                groups.Clear();

                postContext.Dispose();
                deferredContext.Dispose();
                disposedValue = true;
            }
        }

        ~PostProcessingManager()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}