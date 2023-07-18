namespace HexaEngine.PostFx
{
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering.Graph;
    using System.Diagnostics.CodeAnalysis;

    public class PostProcessingManager : IDisposable
    {
        private ShaderMacro[] macros;
        private readonly List<IPostFx> effectsSorted = new();
        private readonly List<IPostFx> effects = new();
        private readonly List<PostFxNode> nodes = new();
        private readonly TopologicalSorter<PostFxNode> topologicalSorter = new();
        private readonly List<Texture2D> buffers = new();
        private readonly IGraphicsContext deferredContext;
        private readonly ConfigKey config;
        private readonly IGraphicsDevice device;
        private int width;
        private int height;

        private ICommandList? list;

        private bool isDirty = true;
        private bool isInitialized = false;
        private bool isReloading;
        private int swapIndex;

        private readonly IGraphicsPipeline copy;

        public Texture2D input;
        public IRenderTargetView output;
        public ITexture2D outputTex;
        public Viewport viewport;
        private bool enabled;
        private bool disposedValue;

        public PostProcessingManager(IGraphicsDevice device, int width, int height, int bufferCount = 4)
        {
            config = Config.Global.GetOrCreateKey("Post Processing");
            this.device = device;
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Add(new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW, ResourceMiscFlag.None, lineNumber: i));
            }

            deferredContext = device.CreateDeferredContext();
            macros = Array.Empty<ShaderMacro>();

            copy = device.CreateGraphicsPipeline(new()
            {
                PixelShader = "effects/copy/ps.hlsl",
                VertexShader = "quad.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen);
        }

        public Texture2D Input { get => input; set => input = value; }

        public IRenderTargetView Output { get => output; set => output = value; }

        public ITexture2D OutputTex { get => outputTex; set => outputTex = value; }

        public Viewport Viewport
        {
            get => viewport;
            set
            {
                if (viewport == value)
                {
                    return;
                }

                viewport = value;
                Invalidate();
            }
        }

        public int Count => effectsSorted.Count;

        public IReadOnlyList<IPostFx> Effects => effectsSorted;

        public bool Enabled { get => enabled; set => enabled = value; }

        public void Initialize(int width, int height)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnEnabledChanged += OnEnabledChanged;
                effects[i].OnPriorityChanged += OnPriorityChanged;
            }

            this.width = width;
            this.height = height;
            macros = new ShaderMacro[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            for (int i = 0; i < effects.Count; i++)
            {
                nodes[i].Clear();
                if (effects[i].Enabled)
                    effects[i].Initialize(device, nodes[i].Builder, width, height, macros).Wait();
            }

            Sort();

            isInitialized = true;
        }

        public async Task InitializeAsync(int width, int height)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnEnabledChanged += OnEnabledChanged;
                effects[i].OnPriorityChanged += OnPriorityChanged;
            }

            this.width = width;
            this.height = height;
            macros = new ShaderMacro[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            for (int i = 0; i < effects.Count; i++)
            {
                nodes[i].Clear();
                if (effects[i].Enabled)
                    await effects[i].Initialize(device, nodes[i].Builder, width, height, macros);
            }

            Sort();

            isInitialized = true;
        }

        public void Add(IPostFx effect)
        {
            lock (effects)
            {
                nodes.Add(new PostFxNode(effect));
                effects.Add(effect);
            }
            config.GenerateSubKeyAuto(effect, effect.Name);

            if (isInitialized)
                Reload();
        }

        public void Remove(IPostFx effect)
        {
            lock (effects)
            {
                nodes.Remove(new PostFxNode(effect));
                effects.Remove(effect);
            }

            if (isInitialized)
                Reload();
        }

        public void Invalidate()
        {
            list?.Dispose();
            list = null;
            isDirty = true;
        }

        private void OnPriorityChanged(int obj)
        {
            Reload();
        }

        private void OnEnabledChanged(bool obj)
        {
            Reload();
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
            Volatile.Write(ref isReloading, true);

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Enabled)
                    effects[i].Dispose();
                nodes[i].Clear();
            }

            macros = new ShaderMacro[effects.Count];

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            Task.Factory.StartNew(async () =>
            {
                Task[] tasks = new Task[effects.Count];
                for (int i = 0; i < effects.Count; i++)
                {
                    if (effects[i].Enabled)
                        tasks[i] = effects[i].Initialize(device, nodes[i].Builder, width, height, macros);
                }
                Task.WaitAll(tasks);
                Sort();
                Invalidate();
            }
            ).ContinueWith(t =>
            {
                Volatile.Write(ref isReloading, false);
            });
        }

        public async Task ReloadAsync()
        {
            Volatile.Write(ref isReloading, true);
            Sort();

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Enabled)
                    effects[i].Dispose();
                nodes[i].Clear();
            }

            macros = new ShaderMacro[effects.Count];

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Enabled)
                    await effects[i].Initialize(device, nodes[i].Builder, width, height, macros);
            }
            Sort();

            Invalidate();

            Volatile.Write(ref isReloading, false);
        }

        public void BeginResize()
        {
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

            for (int i = 0; i < buffers.Count; i++)
            {
                buffers[i].Dispose();
                buffers[i] = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            }

            Invalidate();
        }

        public void PrePassDraw(IGraphicsContext context)
        {
            if (!enabled || isReloading)
            {
                return;
            }

            lock (effectsSorted)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (!effect.Enabled || (effect.Flags & PostFxFlags.PrePass) == 0)
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

                    effect.PrePassDraw(context);
                }
            }
        }

        public void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (!enabled || isReloading)
            {
                context.SetRenderTarget(output, null);
                context.PSSetShaderResource(0, input.SRV);
                context.SetViewport(viewport);
                context.SetGraphicsPipeline(copy);
                context.DrawInstanced(4, 1, 0, 0);
                context.ClearState();
                return;
            }

            if (input == null || output == null)
            {
                return;
            }

            lock (effectsSorted)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (!effect.Enabled || (effect.Flags & PostFxFlags.PrePass) != 0)
                    {
                        continue;
                    }

                    effect.Update(deferredContext);
                }

                if (isDirty)
                {
                    list?.Dispose();
                    deferredContext.ClearState();
                    swapIndex = 0;
                    IShaderResourceView previous = input.SRV;
                    for (int i = 0; i < effectsSorted.Count; i++)
                    {
                        var effect = effectsSorted[i];
                        if (!effect.Enabled)
                        {
                            continue;
                        }

                        if ((effect.Flags & PostFxFlags.NoInput) == 0)
                        {
                            effect.SetInput(previous, input);
                        }

                        if ((effect.Flags & PostFxFlags.NoOutput) == 0)
                        {
                            var buffer = buffers[swapIndex];

                            if (i != effectsSorted.Count - 1)
                            {
                                effect.SetOutput(buffer.RTV, buffer, buffers[swapIndex].Viewport);
                            }
                            else
                            {
                                effect.SetOutput(output, outputTex, viewport);
                            }

                            bool skipSwap = false;
                            if (i < effectsSorted.Count - 1)
                            {
                                skipSwap = effectsSorted[i + 1].Flags == PostFxFlags.Inline;
                            }

                            if (!skipSwap)
                            {
                                previous = buffer.SRV;
                                swapIndex++;
                                if (swapIndex == buffers.Count)
                                {
                                    swapIndex = 0;
                                }
                            }
                        }

                        effect.Draw(deferredContext, creator);
                    }
                    list = deferredContext.FinishCommandList(true);

                    context.ExecuteCommandList(list, false);
                    isDirty = false;
                }
                else
                {
                    context.ExecuteCommandList(list, false);
                }
            }
        }

        private void Sort()
        {
            lock (effectsSorted)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (node.Enabled)
                    {
                        node.Builder.Build(nodes, new List<ResourceBinding>());
                    }
                }

                var sorted = topologicalSorter.TopologicalSort(nodes);

                effectsSorted.Clear();
                for (int i = 0; i < sorted.Count; i++)
                {
                    if (sorted[i].Enabled)
                    {
                        effectsSorted.Add(sorted[i].PostFx);
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    if (effects[i].Enabled)
                        effects[i].Dispose();
                }
                effects.Clear();
                effectsSorted.Clear();

                for (int i = 0; i < buffers.Count; i++)
                {
                    buffers[i].Dispose();
                }
                buffers.Clear();
                list?.Dispose();
                deferredContext.Dispose();
                copy.Dispose();
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