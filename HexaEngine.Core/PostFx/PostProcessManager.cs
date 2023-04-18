namespace HexaEngine.Core.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System.Diagnostics.CodeAnalysis;
    using Texture = Graphics.Texture;

    public class PostProcessManager : IDisposable
    {
        private ShaderMacro[] macros;
        private readonly List<IPostFx> effectsSorted = new();
        private readonly List<IPostFx> effects = new();
        private readonly List<Texture> buffers = new();
        private readonly IGraphicsContext deferredContext;
        private readonly ConfigKey config;
        private readonly IGraphicsDevice device;
        private int width;
        private int height;

#pragma warning disable CS0649 // Field 'PostProcessManager.list' is never assigned to, and will always have its default value null
        private ICommandList? list;
#pragma warning restore CS0649 // Field 'PostProcessManager.list' is never assigned to, and will always have its default value null
        private bool isDirty = true;
        private bool isInitialized = false;
        private int swapIndex;

        private readonly Quad quad;
        private readonly IGraphicsPipeline copy;

        public ResourceRef<Texture> Input;
        public ResourceRef<IRenderTargetView> Output;
        public Viewport Viewport;
        private bool enabled;
        private bool disposedValue;

        public PostProcessManager(IGraphicsDevice device, int width, int height, int bufferCount = 2)
        {
            config = Config.Global.GetOrCreateKey("Post Processing");
            this.device = device;
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Add(new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.RGBA32Float)));
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

        private void InputValueChanged(object? sender, Texture? e)
        {
            isDirty = true;
        }

        public void Initialize(int width, int height)
        {
            this.width = width;
            this.height = height;
            macros = new ShaderMacro[effectsSorted.Count];
            for (int i = 0; i < effectsSorted.Count; i++)
            {
                var effect = effectsSorted[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            for (int i = 0; i < effectsSorted.Count; i++)
            {
                effectsSorted[i].Initialize(device, width, height, macros);
            }

            isInitialized = true;
        }

        public IPostFx? GetByName(string name)
        {
            lock (effectsSorted)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    var effect = effectsSorted[i];
                    if (effect.Name == name)
                        return effect;
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
                        return t;
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
                        return t;
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
            for (int i = 0; i < effectsSorted.Count; i++)
            {
                effectsSorted[i].Dispose();
            }

            macros = new ShaderMacro[effectsSorted.Count];

            for (int i = 0; i < effectsSorted.Count; i++)
            {
                var effect = effectsSorted[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? "1" : "0");
            }

            for (int i = 0; i < effectsSorted.Count; i++)
            {
                effectsSorted[i].Initialize(device, width, height, macros);
            }

            isDirty = true;
        }

        public void BeginResize()
        {
            list?.Dispose();
            isDirty = true;
        }

        public void EndResize(int width, int height)
        {
            this.width = width;
            this.height = height;
            if (!isInitialized)
                return;

            for (int i = 0; i < effectsSorted.Count; i++)
            {
                effectsSorted[i].Resize(width, height);
            }

            for (int i = 0; i < buffers.Count; i++)
            {
                buffers[i].Dispose();
                buffers[i] = new(device, TextureDescription.CreateTexture2D(width, height, 1, Format.RGBA32Float));
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
                return;
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

        public void Draw(IGraphicsContext context)
        {
            if (!enabled)
            {
                context.SetRenderTarget(Output.Value, null);
                context.PSSetShaderResource(Input.Value?.ShaderResourceView, 0);
                context.SetViewport(Viewport);
                quad.DrawAuto(context, copy);
                context.ClearState();
                return;
            }

            if (Input.Value == null || Output.Value == null)
                return;

            lock (effectsSorted)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    effectsSorted[i].Update(context);
                }
#if PostFX_Deferred

                if (isDirty)
                {
                    list?.Dispose();
                    deferredContext.ClearState();
                    swapIndex = 0;
                    IShaderResourceView previous = Input;
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
                                effect.SetOutput(Output, Viewport);

                            previous = buffer.ShaderResourceView;
                        }

                        effect.Draw(deferredContext);
                        deferredContext.ClearState();

                        swapIndex++;
                        if (swapIndex == buffers.Count)
                            swapIndex = 0;
                    }
                    list = deferredContext.FinishCommandList(0);
                    isDirty = false;
                }
#else
                if (isDirty)
                {
                    context.ClearState();
                    swapIndex = 0;
                    IShaderResourceView previous = Input.Value.ShaderResourceView;
                    for (int i = 0; i < effectsSorted.Count; i++)
                    {
                        var effect = effectsSorted[i];
                        if (!effect.Enabled)
                            continue;

                        if ((effect.Flags & PostFxFlags.NoInput) == 0)
                        {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'view' in 'void IPostFx.SetInput(IShaderResourceView view)'.
                            effect.SetInput(previous);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'view' in 'void IPostFx.SetInput(IShaderResourceView view)'.
                        }

                        if ((effect.Flags & PostFxFlags.NoOutput) == 0)
                        {
                            var buffer = buffers[swapIndex];

                            if (i != effectsSorted.Count - 1)
#pragma warning disable CS8604 // Possible null reference argument for parameter 'view' in 'void IPostFx.SetOutput(IRenderTargetView view, Viewport viewport)'.
                                effect.SetOutput(buffer.RenderTargetView, buffers[swapIndex].Viewport);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'view' in 'void IPostFx.SetOutput(IRenderTargetView view, Viewport viewport)'.
                            else
                                effect.SetOutput(Output.Value, Viewport);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                            previous = buffer.ShaderResourceView;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                            swapIndex++;
                            if (swapIndex == buffers.Count)
                                swapIndex = 0;
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
                            continue;

                        effect.Draw(context);
                    }
                }
#endif
            }
#if PostFX_Deferred
            if (list != null)
            {
                context.ExecuteCommandList(list, 1);
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
                    effectsSorted.AddRange(effects.OrderByDescending(x => x.Priority));
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < effectsSorted.Count; i++)
                {
                    effectsSorted[i].Dispose();
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
                quad.Dispose();
                copy.Dispose();
                disposedValue = true;
            }
        }

        ~PostProcessManager()
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