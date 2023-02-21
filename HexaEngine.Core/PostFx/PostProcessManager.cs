namespace HexaEngine.Core.Fx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using Texture = Graphics.Texture;

    public class PostProcessManager : IDisposable
    {
        private ShaderMacro[] macros;
        private readonly List<IPostFx> effectsSorted = new();
        private readonly List<IPostFx> effects = new();
        private readonly List<Texture> buffers = new();
        private readonly IGraphicsContext deferredContext;
        private readonly IGraphicsDevice device;
        private ICommandList? list;
        private bool isDirty = true;
        private bool isInitialized = false;
        private int swapIndex;

        public IShaderResourceView? Input;
        public IRenderTargetView? Output;
        public Viewport Viewport;
        private bool enabled;
        private bool disposedValue;

        public PostProcessManager(IGraphicsDevice device, int width, int height, int bufferCount = 2)
        {
            this.device = device;
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Add(new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.RGBA32Float)));
            }

            deferredContext = device.CreateDeferredContext();
            macros = Array.Empty<ShaderMacro>();
        }

        public int Count => effectsSorted.Count;

        public IReadOnlyList<IPostFx> Effects => effectsSorted;

        public bool Enabled { get => enabled; set => enabled = value; }

        public void Initialize(int width, int height)
        {
            macros = new ShaderMacro[effectsSorted.Count];
            for (int i = 0; i < effectsSorted.Count; i++)
            {
                var effect = effectsSorted[i];
                macros[i] = new ShaderMacro(effect.Name, effect.Enabled ? 1 : 0);
            }

            for (int i = 0; i < effectsSorted.Count; i++)
            {
                effectsSorted[i].Initialize(device, width, height, macros);
            }

            isInitialized = true;

            Input = ResourceManager.GetTextureSRV("LightBuffer");
            Output = ResourceManager.GetResourceAsync<IRenderTargetView>("SwapChain.RTV").Result;
        }

        public void Reload()
        {
            isDirty = true;
        }

        public void BeginResize()
        {
            list?.Dispose();
            isDirty = true;
        }

        public void EndResize(int width, int height)
        {
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

            Input = ResourceManager.GetTextureSRV("LightBuffer");
            Output = ResourceManager.GetResourceAsync<IRenderTargetView>("SwapChain.RTV").Result;
            isDirty = true;
        }

        public void ResizeOutput()
        {
            Output = ResourceManager.GetResourceAsync<IRenderTargetView>("SwapChain.RTV").Result;
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
                return;
            if (Input == null || Output == null)
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