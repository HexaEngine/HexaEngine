namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Weather;
    using System;
    using System.Numerics;

    public class SkyRenderer : IDisposable
    {
        private readonly UVSphere cube = null!;
        private readonly IGraphicsPipelineState skybox = null!;
        private readonly IGraphicsPipelineState uniformColorSky = null!;
        private readonly IGraphicsPipelineState hoseWilkieSky = null!;
        private readonly IGraphicsPipelineState preethamSky = null!;
        private readonly ConstantBuffer<CBWorld> worldBuffer = null!;

        private Texture2D environment = null!;

        private bool initialized;
        private bool disposedValue;

        public SkyRenderer(IGraphicsDevice device)
        {
            cube = new(new());
            worldBuffer = new(CpuAccessFlags.Write);

            skybox = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "forward/sky/vs.hlsl",
                PixelShader = "forward/sky/skybox.hlsl",
            }, new()
            {
                Rasterizer = RasterizerDescription.CullNone,
                DepthStencil = DepthStencilDescription.DepthRead,
                Blend = BlendDescription.Opaque
            });

            skybox.Bindings.SetCBV("WorldBuffer", worldBuffer);
            skybox.Bindings.SetSRV("cubeMap", environment);

            uniformColorSky = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "forward/sky/vs.hlsl",
                PixelShader = "forward/sky/uniformColorSky.hlsl",
            }, new()
            {
                Rasterizer = RasterizerDescription.CullNone,
                DepthStencil = DepthStencilDescription.DepthRead,
                Blend = BlendDescription.Opaque
            });

            uniformColorSky.Bindings.SetCBV("WorldBuffer", worldBuffer);
            uniformColorSky.Bindings.SetSRV("cubeMap", environment);

            hoseWilkieSky = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "forward/sky/vs.hlsl",
                PixelShader = "forward/sky/hoseWilkieSky.hlsl",
            }, new()
            {
                Rasterizer = RasterizerDescription.CullNone,
                DepthStencil = DepthStencilDescription.DepthRead,
                Blend = BlendDescription.Opaque
            });

            hoseWilkieSky.Bindings.SetCBV("WorldBuffer", worldBuffer);
            hoseWilkieSky.Bindings.SetSRV("cubeMap", environment);

            preethamSky = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "forward/sky/vs.hlsl",
                PixelShader = "forward/sky/preethamSky.hlsl",
            }, new()
            {
                Rasterizer = RasterizerDescription.CullNone,
                DepthStencil = DepthStencilDescription.DepthRead,
                Blend = BlendDescription.Opaque
            });

            preethamSky.Bindings.SetCBV("WorldBuffer", worldBuffer);
            preethamSky.Bindings.SetSRV("cubeMap", environment);
        }

        public void Initialize(Skybox skybox)
        {
            if (skybox.Environment == null)
            {
                return;
            }

            environment = skybox.Environment;

            this.skybox.Bindings.SetCBV("WorldBuffer", worldBuffer);
            this.skybox.Bindings.SetSRV("cubeMap", environment);

            initialized = true;
        }

        public void Uninitialize()
        {
            initialized = false;

            environment = null!;
        }

        public void Update(IGraphicsContext context)
        {
            var camera = CameraManager.Current;
            if (camera == null)
            {
                return;
            }

            worldBuffer.Update(context, new(Matrix4x4.CreateScale(camera.Transform.Far) * Matrix4x4.CreateTranslation(camera.Transform.GlobalPosition)));
        }

        public void Draw(IGraphicsContext context, SkyType type)
        {
            WeatherSystem.Current!.SkyModel = type;

            switch (type)
            {
                case SkyType.Skybox:
                    if (!initialized)
                    {
                        return;
                    }

                    cube.DrawAuto(context, skybox);
                    break;

                case SkyType.UniformColor:
                    cube.DrawAuto(context, uniformColorSky);
                    break;

                case SkyType.HosekWilkie:
                    cube.DrawAuto(context, hoseWilkieSky);
                    break;

                case SkyType.Preetham:
                    cube.DrawAuto(context, preethamSky);
                    break;

                case SkyType.Custom:
                    cube.DrawAuto(context, skybox);
                    break;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Uninitialize();
                cube.Dispose();
                skybox.Dispose();
                uniformColorSky.Dispose();
                hoseWilkieSky.Dispose();
                preethamSky.Dispose();
                worldBuffer.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}