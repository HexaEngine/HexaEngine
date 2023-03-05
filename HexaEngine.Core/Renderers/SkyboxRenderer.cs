namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Unsafes;
    using System.Numerics;

    public unsafe struct SkyboxData
    {
        public void* Enviornment;

        public SkyboxData(Texture texture)
        {
            Enviornment = (void*)(texture?.ShaderResourceView?.NativePointer ?? 0);
        }
    }

    public class SkyboxRenderer : IRenderer
    {
        private readonly ObjectPool<SkyboxData> pool = new();
        private readonly DrawQueue drawQueue = new();
        private volatile bool _disposed = true;
#nullable disable
        private Sphere sphere;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;
        private ConstantBuffer<CBWorld> cb;
        private unsafe void** cbs;
#nullable enable

        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Background;

        public async void Initialize(IGraphicsDevice device, InstanceManager instanceManager)
        {
            sphere = new(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "forward/skybox/vs.hlsl",
                PixelShader = "forward/skybox/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = default,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                SampleMask = 0,
                StencilRef = 0,
                Topology = PrimitiveTopology.TriangleList
            });

            sampler = ResourceManager2.Shared.GetOrAddSamplerState("AnisotropicClamp", SamplerDescription.AnisotropicClamp).Value;

            unsafe
            {
                cb = new(device, CpuAccessFlags.Write);
                cbs = AllocArray(2);
                cbs[0] = (void*)cb.NativePointer;
                cbs[1] = (void*)(ResourceManager2.Shared.GetBuffer("CBCamera")?.Value.NativePointer ?? 0);
            }
            _disposed = false;
        }

        public unsafe void Uninitialize()
        {
            _disposed = true;
            sphere.Dispose();
            pipeline.Dispose();
            cb.Dispose();
            Free(cbs);
        }

        public Pointer<ObjectHandle> CreateInstance(SkyboxData data)
        {
            return pool.Add(data);
        }

        public void DestroyInstance(Pointer<ObjectHandle> handle)
        {
            pool.Remove(handle);
        }

        public void UpdateInstance(Pointer<ObjectHandle> handle, SkyboxData data)
        {
            pool[handle] = data;
        }

        public void Draw(Pointer<ObjectHandle> handle)
        {
            drawQueue.Enqueue(handle);
        }

        void IRenderer.DrawDepth(IGraphicsContext context)
        {
        }

        unsafe void IRenderer.Draw(IGraphicsContext context)
        {
            if (_disposed) return;

            if (drawQueue.IsEmpty)
            {
                drawQueue.Swap();
                return;
            }

            var camera = CameraManager.Current;
            if (camera == null)
                return;

            while (drawQueue.TryDequeue(out var handle))
            {
                if (!handle.Data->IsValid)
                    continue;
                var data = pool[handle];
                cb[0] = new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far - 0.1f) * Matrix4x4.CreateTranslation(camera.Transform.Position));
                cb.Update(context);

                unsafe
                {
                    context.VSSetConstantBuffers(cbs, 2, 0);
                    context.PSSetShaderResource(data.Enviornment, 0);
                    context.PSSetSampler(sampler, 0);
                    sphere.DrawAuto(context, pipeline);
                }
            }

            drawQueue.Swap();
        }

        void IRenderer.DrawIndirect(IGraphicsContext context, IBuffer drawArgs, uint offset)
        {
        }

        void IRenderer.VisibilityTest(IGraphicsContext context)
        {
        }
    }
}