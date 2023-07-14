namespace HexaEngine.PostFx
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System.Diagnostics.CodeAnalysis;

    public class PostProcessingManager : IDisposable
    {
        private ShaderMacro[] macros;
        private readonly List<IPostFx> effectsSorted = new();
        private readonly List<IPostFx> effects = new();
        private readonly List<Texture2D> buffers = new();
        private readonly IGraphicsContext deferredContext;
        private readonly ConfigKey config;
        private readonly IGraphicsDevice device;
        private int width;
        private int height;

#if PostFX_Deferred
        private ICommandList? list;
#endif
        private bool isDirty = true;
        private bool isInitialized = false;
        private bool isReloading;
        private int swapIndex;

        private readonly Quad quad;
        private readonly IGraphicsPipeline copy;

        public ResourceRef<Texture2D> Input;
        public ResourceRef<IRenderTargetView> Output;
        public ResourceRef<ITexture2D> OutputTex;
        public Viewport Viewport;
        private bool enabled;
        private bool disposedValue;

        public PostProcessingManager(IGraphicsDevice device, int width, int height, int bufferCount = 2)
        {
            config = Config.Global.GetOrCreateKey("Post Processing");
            this.device = device;
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Add(new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW));
            }

            deferredContext = device.CreateDeferredContext();
            macros = Array.Empty<ShaderMacro>();

            quad = new(device);
            copy = device.CreateGraphicsPipeline(new()
            {
                PixelShader = "effects/copy/ps.hlsl",
                VertexShader = "effects/copy/vs.hlsl",
            });

            Input = ResourceManager2.Shared.GetTexture("LightBuffer");
            Output = ResourceManager2.Shared.GetResource<IRenderTargetView>("SwapChain.RTV");
            OutputTex = ResourceManager2.Shared.GetResource<ITexture2D>("SwapChain");
            Input.ValueChanged += InputValueChanged;
            Output.ValueChanged += OutputValueChanged;
        }

        public int Count => effectsSorted.Count;

        public IReadOnlyList<IPostFx> Effects => effectsSorted;

        public bool Enabled { get => enabled; set => enabled = value; }

        private void OutputValueChanged(object? sender, IRenderTargetView? e)
        {
            isDirty = true;
        }

        private void InputValueChanged(object? sender, Texture2D? e)
        {
            isDirty = true;
        }

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
                if (effects[i].Enabled)
                    effects[i].Initialize(device, width, height, macros).Wait();
            }

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
                if (effects[i].Enabled)
                    await effects[i].Initialize(device, width, height, macros);
            }

            isInitialized = true;
        }

        private void OnPriorityChanged(int obj)
        {
            Reload();
        }

        private void OnEnabledChanged(bool obj)
        {
            Reload();
        }

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

        public void Reload()
        {
            Volatile.Write(ref isReloading, true);
            Sort();

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Enabled)
                    effects[i].Dispose();
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
                        tasks[i] = effects[i].Initialize(device, width, height, macros);
                }
                Task.WaitAll(tasks);
            }
            ).ContinueWith(t =>
            {
                Volatile.Write(ref isReloading, false);
            });

            isDirty = true;
        }

        public async Task ReloadAsync()
        {
            Volatile.Write(ref isReloading, true);
            Sort();

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Enabled)
                    effects[i].Dispose();
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
                    await effects[i].Initialize(device, width, height, macros);
            }
            Volatile.Write(ref isReloading, false);
            isDirty = true;
        }

        public void BeginResize()
        {
#if PostFX_Deferred
            list?.Dispose();
#endif
            isDirty = true;
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

            isDirty = true;
        }

        public void ResizeOutput()
        {
            isDirty = true;
        }

        public void SetViewport(Viewport viewport)
        {
            if (Viewport == viewport)
            {
                return;
            }

            Viewport = viewport;
            isDirty = true;
        }

        public void Add(IPostFx effect)
        {
            lock (effects)
            {
                effects.Add(effect);
            }
            config.GenerateSubKeyAuto(effect, effect.Name);
            Sort();
        }

        public void Remove(IPostFx effect)
        {
            lock (effects)
            {
                effects.Remove(effect);
            }
            Sort();
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

        public void Draw(IGraphicsContext context)
        {
            if (!enabled || isReloading)
            {
                context.SetRenderTarget(Output.Value, null);
                context.PSSetShaderResource(0, Input.Value?.SRV);
                context.SetViewport(Viewport);
                quad.DrawAuto(context, copy);
                context.ClearState();
                return;
            }

            if (Input.Value == null || Output.Value == null)
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

                    effect.Update(context);
                }
#if PostFX_Deferred

                if (isDirty)
                {
                    list?.Dispose();
                    deferredContext.ClearState();
                    swapIndex = 0;
                    IShaderResourceView previous = Input.Value.ShaderResourceView;
                    for (int i = 0; i < effectsSorted.Count; i++)
                    {
                        var effect = effectsSorted[i];
                        if (!effect.Enabled)
                            continue;

                        if ((effect.Flags & PostFxFlags.NoInput) == 0)
                        {
                            effect.SetInput(previous);
                        }

                        if ((effect.Flags & PostFxFlags.NoOutput) == 0)
                        {
                            var buffer = buffers[swapIndex];

                            if (i != effectsSorted.Count - 1)
                                effect.SetOutput(buffer.RenderTargetView, buffers[swapIndex].Viewport);
                            else
                                effect.SetOutput(Output.Value, Viewport);

                            previous = buffer.ShaderResourceView;
                        }

                        effect.Draw(deferredContext);
                        deferredContext.ClearState();

                        swapIndex++;
                        if (swapIndex == buffers.Count)
                            swapIndex = 0;
                    }
                    list = deferredContext.FinishCommandList(false);
                    isDirty = false;
                }
#else
                if (isDirty)
                {
                    context.ClearState();
                    swapIndex = 0;
                    IShaderResourceView previous = Input.Value.SRV;
                    for (int i = 0; i < effectsSorted.Count; i++)
                    {
                        var effect = effectsSorted[i];
                        if (!effect.Enabled)
                        {
                            continue;
                        }

                        if ((effect.Flags & PostFxFlags.NoInput) == 0)
                        {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'view' in 'void IPostFx.SetInput(IShaderResourceView view)'.
                            effect.SetInput(previous, Input.Value);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'view' in 'void IPostFx.SetInput(IShaderResourceView view)'.
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
                                effect.SetOutput(Output.Value, OutputTex.Value, Viewport);
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

                        effect.Draw(context);
                    }
                    isDirty = false;
                }
                else
                {
                    context.ClearState();
                    for (int i = 0; i < effectsSorted.Count; i++)
                    {
                        var effect = effectsSorted[i];
                        if (!effect.Enabled)
                        {
                            continue;
                        }

                        effect.Draw(context);
                    }
                }
#endif
            }
#if PostFX_Deferred
            if (list != null)
            {
                context.ExecuteCommandList(list, true);
            }
#endif
        }

        private void Sort()
        {
            lock (effectsSorted)
            {
                effectsSorted.Clear();
                lock (effects)
                {
                    effectsSorted.AddRange(effects.Where(x => x.Enabled).OrderByDescending(x => x.Priority));
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
#if PostFX_Deferred
                list?.Dispose();
#endif
                deferredContext.Dispose();
                quad.Dispose();
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