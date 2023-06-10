namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Effects;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    [EditorComponent<SkyboxRenderer>("Skybox", false, true)]
    public class SkyboxRenderer : IRendererComponent
    {
        private GameObject gameObject;
        private IGraphicsDevice? device;
        private Cube sphere;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;
        private ConstantBuffer<CBWorld> cb;
        private unsafe void** cbs;
        private Texture? env;
        private string environment = string.Empty;
        private bool drawable;

        private IGraphicsPipeline light;
        private ConstantBuffer<CBSkylight> cbSkylight;

        [JsonIgnore]
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Background;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.Update | RendererFlags.Depth | RendererFlags.Geometry;

        [JsonIgnore]
        public BoundingBox BoundingBox { get; }

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
            light = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "forward/skylight/vs.hlsl",
                PixelShader = "forward/skylight/ps.hlsl",
            }, new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = default,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                SampleMask = 0,
                StencilRef = 0,
                Topology = PrimitiveTopology.TriangleList
            });
            cbSkylight = new(device, CpuAccessFlags.Write);

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

            cb[0] = new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far / 2) * Matrix4x4.CreateTranslation(camera.Transform.Position));
            cb.Update(context);

            cbSkylight[0] = new((float)Math.PI, (float)Math.PI, 0, 0);
            cbSkylight.Update(context);
        }

        public void DrawDepth(IGraphicsContext context)
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
                /*
                context.PSSetConstantBuffer(cbSkylight, 0);
                sphere.DrawAuto(context, light);*/
            }
        }

        private Task UpdateEnvAsync(IGraphicsDevice device)
        {
            var state = new Tuple<IGraphicsDevice, SkyboxRenderer>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, SkyboxRenderer>)state;
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

        public void DrawShadows(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotImplementedException();
        }
    }
}