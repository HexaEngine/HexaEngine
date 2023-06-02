namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    [EditorComponent<SkyboxRendererComponent>("Skybox", false, true)]
    public class SkyboxRendererComponent : IRendererComponent
    {
        private GameObject gameObject;
        private IGraphicsDevice? device;
        private Sphere sphere;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;
        private ConstantBuffer<CBWorld> cb;
        private unsafe void** cbs;
        private Texture? env;
        private string environment = string.Empty;
        private bool drawable;

        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Background;

        [EditorProperty("Env", null)]
        public string Environment
        {
            get => environment;
            set
            {
                environment = value;
                if (device == null)
                {
                    return;
                }

                Volatile.Write(ref drawable, false);
                UpdateEnvAsync(device);
            }
        }

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.device = device;
            if (!gameObject.GetScene().TryGetSystem<RenderManager>(out var manager))
            {
                return;
            }

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

            await UpdateEnvAsync(device);
        }

        public unsafe void Destory()
        {
            Volatile.Write(ref drawable, false);

            env?.Dispose();
            sphere.Dispose();
            pipeline.Dispose();
            cb.Dispose();
            Free(cbs);
        }

        public void Update(IGraphicsContext context)
        {
            if (!drawable)
                return;
            var camera = CameraManager.Current;
            if (camera == null)
            {
                return;
            }

            cb[0] = new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far - 0.1f) * Matrix4x4.CreateTranslation(camera.Transform.Position));
            cb.Update(context);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
        }

        public void VisibilityTest(IGraphicsContext context)
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (!Volatile.Read(ref drawable) || !gameObject.IsEnabled)
            {
                return;
            }

            var camera = CameraManager.Current;
            if (camera == null)
            {
                return;
            }

            if (env == null)
            {
                return;
            }

            unsafe
            {
                context.VSSetConstantBuffers(cbs, 2, 0);
                context.PSSetShaderResource(env.ShaderResourceView, 0);
                context.PSSetSampler(sampler, 0);
                sphere.DrawAuto(context, pipeline);
            }
        }

        private Task UpdateEnvAsync(IGraphicsDevice device)
        {
            var state = new Tuple<IGraphicsDevice, SkyboxRendererComponent>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, SkyboxRendererComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.environment;

                unsafe
                {
                    // Inform renderer to stop render the skybox.
                    Volatile.Write(ref component.drawable, false);
                }

                component.env?.Dispose();
                if (FileSystem.Exists(path))
                {
                    try
                    {
                        component.env = await Texture.CreateTextureAsync(device, new TextureFileDescription(path, TextureDimension.TextureCube));
                    }
                    catch (Exception ex)
                    {
                        ImGuiConsole.Log(ex);
                        component.env = await Texture.CreateTextureAsync(device, TextureDimension.TextureCube, default);
                    }
                }
                else
                {
                    component.env = await Texture.CreateTextureAsync(device, TextureDimension.TextureCube, default);
                }

                Volatile.Write(ref component.drawable, true);
            }, state);
        }
    }
}