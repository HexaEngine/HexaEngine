namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class PostProcessingManager : IDisposable
    {
        private readonly object _lock = new();
        private readonly SemaphoreSlim semaphore = new(1);

        private ShaderMacro[] macros;
        private readonly PostProcessingContext postContext;
        private readonly List<IPostFx> activeEffects = new();
        private readonly List<IPostFx> effects = [];
        private readonly IGraphicsContext deferredContext;
        private readonly IGraphicsDevice device;
        private readonly PostProcessingFlags flags;
        private readonly PostFxGraphResourceBuilder creator;
        private readonly PostFxGraph graph = new();
        private int width;
        private int height;

        private readonly List<PostProcessingExecutionGroup> groups = [];

        private bool isDirty;
        private bool isInitialized = false;
        private volatile bool isReloading;

        private bool enabled;
        private bool disposedValue;

        public PostProcessingManager(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, int bufferCount = 4, PostProcessingFlags flags = PostProcessingFlags.HDR)
        {
            Format format = (flags & PostProcessingFlags.HDR) != 0 ? Format.R16G16B16A16Float : Format.R8G8B8A8UNorm;
            Current ??= this;
            this.device = device;
            this.flags = flags;
            this.creator = new(creator, device, format, width, height);

            postContext = new(device, format, width, height, bufferCount);

            deferredContext = device.CreateDeferredContext();
            macros = [];
        }

        public static PostProcessingManager? Current { get; private set; }

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

        public PostFxGraph Graph => graph;

        public IReadOnlyList<IPostFx> Effects => effects;

        public IReadOnlyList<IPostFx> ActiveEffects => activeEffects;

        public IReadOnlyList<PostProcessingExecutionGroup> Groups => groups;

        public object SyncObject => _lock;

        public bool Enabled { get => enabled; set => enabled = value; }

        public bool IsInitialized => isInitialized;

        public bool IsDirty => isDirty;

        public bool IsReloading => isReloading;

        public PostProcessingFlags Flags => flags;

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

            macros = new ShaderMacro[effects.Count + 1];
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }
            macros[^1] = new ShaderMacro("HDR", (flags & PostProcessingFlags.HDR) != 0 ? "1" : "0");

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect.Enabled)
                {
                    PostFxNode node = graph.GetNode(effect);
                    creator.Container = node.Container;
                    effect.Initialize(device, creator, width, height, macros);
                    creator.Container = null;
                }
            }

            UpdateGroups();

            isInitialized = true;

            semaphore.Release();
        }

        private enum PrivateFlags
        {
            None = 0,
            ReloadPending = 1,
            InvalidatePending = 2,
        }

        private bool suppressListeners;
        private PrivateFlags privateFlags;
        private Queue<IPostFx> reloadQueue = new();

        private void OnEnabledChanged(IPostFx postFx, bool enabled)
        {
            if (postFx.Flags.HasFlag(PostFxFlags.Optional))
            {
                return;
            }

            if (suppressListeners)
            {
                lock (this)
                {
                    privateFlags |= PrivateFlags.ReloadPending;
                }
                return;
            }

            ReloadAsync();
        }

        private void OnReload(IPostFx postFx)
        {
            if (!postFx.Enabled)
            {
                return;
            }

            if (suppressListeners)
            {
                lock (this)
                {
                    reloadQueue.Enqueue(postFx);
                }
                return;
            }

            semaphore.Wait();
            GraphResourceContainer container = graph.GetNode(postFx).Container;
            postFx.Dispose();
            container.DisposeResources();

            creator.Container = container;
            postFx.Initialize(device, creator, width, height, macros);
            creator.Container = null;
            Invalidate();
            semaphore.Release();
        }

        private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not IPostFx postFx || !postFx.Enabled)
            {
                return;
            }

            if (suppressListeners)
            {
                privateFlags |= PrivateFlags.InvalidatePending;
                return;
            }

            semaphore.Wait();
            Invalidate();
            semaphore.Release();
        }

        private class SuppressHandle : IDisposable
        {
            private readonly PostProcessingManager instance;

            public SuppressHandle(PostProcessingManager instance)
            {
                this.instance = instance;
            }

            ~SuppressHandle()
            {
                DisposeInternal();
            }

            private void DisposeInternal()
            {
                instance.ResumeReload();
            }

            public void Dispose()
            {
                DisposeInternal();
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Supresses internal reload mechanisms used for reducing overhead for high frequency changes, call <see cref="ResumeReload"/> or <see cref="IDisposable.Dispose"/> afterwards to avoid dead-locks.
        /// </summary>
        /// <remarks><c>Call <see cref="ResumeReload"/> or <see cref="IDisposable.Dispose"/> immediately</c> after your operations are done to avoid dead-locks.</remarks>
        public IDisposable SupressReload()
        {
            if (suppressListeners)
            {
                return new SuppressHandle(this);
            }

            semaphore.Wait();
            suppressListeners = true;
            return new SuppressHandle(this);
        }

        public void ResumeReload()
        {
            if (!suppressListeners)
            {
                return;
            }

            suppressListeners = false;

            if ((privateFlags & PrivateFlags.ReloadPending) != 0)
            {
                reloadQueue.Clear();
                ReloadAsync();
                privateFlags = PrivateFlags.None;
            }

            while (reloadQueue.TryDequeue(out var postFx))
            {
                GraphResourceContainer container = graph.GetNode(postFx).Container;
                postFx.Dispose();
                container.DisposeResources();

                creator.Container = container;
                postFx.Initialize(device, creator, width, height, macros);
                creator.Container = null;
                privateFlags |= PrivateFlags.InvalidatePending;
            }

            if ((privateFlags & PrivateFlags.InvalidatePending) != 0)
            {
                Invalidate();
            }

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
            lock (activeEffects)
            {
                for (int i = 0; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
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
            lock (activeEffects)
            {
                for (int i = 0; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
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
            lock (activeEffects)
            {
                for (int i = 0; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
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
                    graph.GetNode(effect).Container.DisposeResources();
                    effect.Dispose();
                }
            }

            Sort();

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect.Enabled)
                {
                    PostFxNode node = graph.GetNode(effect);
                    creator.Container = node.Container;
                    effect.Initialize(device, creator, width, height, macros);
                    creator.Container = null;
                }
            }

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
                        graph.GetNode(effect).Container.DisposeResources();
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

                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    if (effect.Enabled)
                    {
                        PostFxNode node = graph.GetNode(effect);
                        creator.Container = node.Container;
                        effect.Initialize(device, creator, width, height, macros);
                        creator.Container = null;
                    }
                }

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

            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (effects[i].Enabled)
                {
                    activeEffects[i].Resize(width, height);
                }
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
                for (int i = 0; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
                    if (!effect.Enabled || (effect.Flags & PostFxFlags.PrePass) == 0 || (effect.Flags & PostFxFlags.ComposeTarget) != 0)
                    {
                        continue;
                    }
                    effect.Update(context);
                }

                context.ClearState();
                for (int i = 0; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
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

                for (int i = 0; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
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
                graph.Build(activeEffects);
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

            if (activeEffects.Count == 0)
            {
                return;
            }

            lock (_lock)
            {
                IPostFx first = activeEffects[0];
                PostProcessingExecutionGroup group = new(first.Flags.HasFlag(PostFxFlags.Dynamic) || flags.HasFlag(PostProcessingFlags.ForceDynamic), true, false);
                group.Passes.Add(first);
                groups.Add(group);
                for (int i = 1; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
                    var isDynamic = effect.Flags.HasFlag(PostFxFlags.Dynamic) || flags.HasFlag(PostProcessingFlags.ForceDynamic);

                    if (group.IsDynamic != isDynamic)
                    {
                        var tmp = group;
                        group = new(isDynamic, false, false);
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
                if (Current == this)
                {
                    Current = null;
                }

                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    if (effect.Initialized)
                    {
                        graph.GetNode(effect).Container.DisposeResources();
                        effect.Dispose();
                    }

                    effect.OnEnabledChanged -= OnEnabledChanged;
                    effect.OnReload -= OnReload;
                    effect.PropertyChanged -= PropertyChanged;
                }
                effects.Clear();
                activeEffects.Clear();
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