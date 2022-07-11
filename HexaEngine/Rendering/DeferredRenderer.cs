﻿namespace HexaEngine.Rendering
{
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Scenes;
    using HexaEngine.Shaders;
    using HexaEngine.Shaders.Effects;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class DeferredRenderer : ISceneRenderer
    {
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;

        private Quad quad;
        private UVSphere skycube;

        private IBuffer cameraBuffer;
        private MTLShader materialShader;
        private MTLDepthShaderBack materialDepthBackface;
        private MTLDepthShaderFront materialDepthFrontface;
        private SkyboxShader skyboxShader;

        private RenderTextureArray gbuffers;

        private Texture ssaoBuffer;
        private Texture lightMap;
        private Texture fxaaBuffer;
        private Texture ssrBuffer;
        private Texture frontdepthbuffer;

        private ISamplerState pointSampler;

        private HBAOEffect ssaoEffect;
        private DDASSREffect ssrEffect;
        private BlendBoxBlurEffect ssrBlurEffect;
        private BlendEffect blendEffect;
        private FXAAEffect fxaaEffect;

        private BRDFEffect brdfFilter;

        private ISamplerState envsmp;
        private Texture env;
        private Texture envirr;
        private Texture envfilter;

        private Texture brdflut;

        public DeferredRenderer()
        {
            ImGui.StyleColorsDark();
        }

        public void Initialize(IGraphicsDevice device, SdlWindow window)
        {
            this.device = device;
            context = device.Context;
            swapChain = device.SwapChain;
            swapChain.Resizing += OnResizeBegin;
            swapChain.Resized += OnResizeEnd;

            quad = new(device);
            skycube = new(device);

            cameraBuffer = device.CreateBuffer(new CBCamera(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            materialShader = new(device);
            materialDepthBackface = new(device);
            materialDepthFrontface = new(device);
            skyboxShader = new(device);

            gbuffers = new RenderTextureArray(device, 1280, 720, 8, Format.RGBA32Float);
            gbuffers.RenderTargets.DepthStencil = swapChain.BackbufferDSV;
            gbuffers.Add(new(ShaderStage.Pixel, 0));

            frontdepthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(1280, 720, 1, Format.R32Float), DepthStencilDesc.Default);
            pointSampler = device.CreateSamplerState(SamplerDescription.PointClamp);

            lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(1280, 720, 1));

            ssaoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(1280, 720, 1, Format.R32Float));
            ssaoEffect = new(device);
            ssaoEffect.Target = ssaoBuffer.RenderTargetView;
            ssaoEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            fxaaBuffer = new(device, swapChain.BackbufferDSV, TextureDescription.CreateTexture2DWithRTV(1280, 720, 1));
            fxaaEffect = new(device);
            fxaaEffect.Target = swapChain.BackbufferRTV;
            fxaaEffect.Resources.Add(new(fxaaBuffer.ResourceView, ShaderStage.Pixel, 0));
            fxaaEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(1280, 720, 1));
            ssrEffect = new(device);
            ssrEffect.Target = ssrBuffer.RenderTargetView;
            ssrEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            ssrBlurEffect = new(device);
            ssrBlurEffect.Target = fxaaBuffer.RenderTargetView;
            ssrBlurEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));
            ssrBlurEffect.Resources.Add(new(ssrBuffer.ResourceView, ShaderStage.Pixel, 0));

            blendEffect = new(device);
            blendEffect.Target = fxaaBuffer.RenderTargetView;
            blendEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            env = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "env_o.dds", TextureDimension.TextureCube));
            env.AddBinding(new(ShaderStage.Pixel, 0));
            envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
            envirr.AddBinding(new(ShaderStage.Pixel, 0));
            envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));

            envsmp = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

            brdflut = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RG32Float));

            brdfFilter = new(device);
            brdfFilter.Target = brdflut.RenderTargetView;
            brdfFilter.Draw(context, null);

            ImGuiConsole.RegisterCommand("recompile_shaders", _ =>
            {
                SceneManager.Current.Dispatcher.Invoke(() => Pipeline.ReloadShaders());
            });

            FramebufferDebugger.AddRange(new IShaderResourceView[]
            {
                gbuffers.GetResourceView(0),
                gbuffers.GetResourceView(1),
                gbuffers.GetResourceView(2),
                gbuffers.GetResourceView(3),
                gbuffers.GetResourceView(4),
                gbuffers.GetResourceView(5),
                gbuffers.GetResourceView(6),
                gbuffers.GetResourceView(7),
                lightMap.ResourceView,
                ssaoBuffer.ResourceView,
                ssrBuffer.ResourceView,
                fxaaBuffer.ResourceView,
            });
        }

        private void OnResizeBegin(object sender, EventArgs e)
        {
            FramebufferDebugger.Clear();
            gbuffers.Dispose();
            lightMap.Dispose();
            ssaoBuffer.Dispose();
            fxaaBuffer.Dispose();
            ssrBuffer.Dispose();
            frontdepthbuffer.Dispose();
        }

        private void OnResizeEnd(object sender, ResizedEventArgs e)
        {
            gbuffers = new RenderTextureArray(device, e.NewWidth, e.NewHeight, 8, Format.RGBA32Float);
            gbuffers.RenderTargets.DepthStencil = swapChain.BackbufferDSV;
            gbuffers.Add(new(ShaderStage.Pixel, 0));
            frontdepthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1), DepthStencilDesc.Default);

            lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));

            ssaoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1, Format.R32Float));
            ssaoEffect.Target = ssaoBuffer.RenderTargetView;

            fxaaBuffer = new(device, swapChain.BackbufferDSV, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));
            fxaaEffect.Target = swapChain.BackbufferRTV;
            fxaaEffect.Resources.Clear();
            fxaaEffect.Resources.Add(new(fxaaBuffer.ResourceView, ShaderStage.Pixel, 0));

            ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));
            ssrEffect.Target = ssrBuffer.RenderTargetView;

            ssrBlurEffect.Target = fxaaBuffer.RenderTargetView;
            ssrBlurEffect.Resources.Clear();
            ssrBlurEffect.Resources.Add(new(ssrBuffer.ResourceView, ShaderStage.Pixel, 0));

            blendEffect.Target = fxaaBuffer.RenderTargetView;
            FramebufferDebugger.AddRange(new IShaderResourceView[]
            {
                gbuffers.GetResourceView(0),
                gbuffers.GetResourceView(1),
                gbuffers.GetResourceView(2),
                gbuffers.GetResourceView(3),
                gbuffers.GetResourceView(4),
                gbuffers.GetResourceView(5),
                gbuffers.GetResourceView(6),
                gbuffers.GetResourceView(7),
                lightMap.ResourceView,
                ssaoBuffer.ResourceView,
                ssrBuffer.ResourceView,
                fxaaBuffer.ResourceView,
            });

            gbuffers.GetResourceView(0).DebugName = "Albedo";
            gbuffers.GetResourceView(1).DebugName = "Position Depth";
            gbuffers.GetResourceView(2).DebugName = "Normal Roughness";
            gbuffers.GetResourceView(3).DebugName = "Clearcoat Metallness";
            gbuffers.GetResourceView(4).DebugName = "Emission";
            gbuffers.GetResourceView(5).DebugName = "Misc 0";
            gbuffers.GetResourceView(6).DebugName = "Misc 1";
            gbuffers.GetResourceView(7).DebugName = "Misc 2";
            lightMap.ResourceView.DebugName = "Light Buffer";
            ssaoBuffer.ResourceView.DebugName = "SSAO/HBAO Buffer";
            ssrBuffer.ResourceView.DebugName = "SSR/DDASSR Buffer";
            fxaaBuffer.ResourceView.DebugName = "FXAA Buffer";
        }

        public void Render(IGraphicsContext context, SdlWindow window, Viewport viewport, Scene scene, Camera camera)
        {
            context.Write(cameraBuffer, new CBCamera(camera));
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            // Fill Geometry Buffer
            gbuffers.RenderTargets.ClearTargets(context);
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                Mesh mesh = scene.Meshes[i];
                if (!mesh.Drawable) continue;
                gbuffers.RenderTargets.SetTargets(context);
                context.SetConstantBuffer(cameraBuffer, ShaderStage.Domain, 1);
                mesh.Material.Bind(context);
                mesh.DrawAuto(context, materialShader, gbuffers.Viewport, camera);
            }

            // Draw backface depth
            frontdepthbuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.Stencil | DepthStencilClearFlags.Depth);
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                Mesh mesh = scene.Meshes[i];
                if (!mesh.Drawable) continue;
                frontdepthbuffer.SetTarget(context);
                context.SetConstantBuffer(cameraBuffer, ShaderStage.Domain, 1);
                mesh.Material.Bind(context);
                mesh.DrawAuto(context, materialDepthBackface, frontdepthbuffer.Viewport, null);
            }

            // Draw light depth
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];
                if (!light.CastShadows) continue;
                switch (light.Type)
                {
                    case LightType.Directional:
                        DirectionalLight directional = (DirectionalLight)light;
                        directional.DepthMap.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil);
                        for (int j = 0; j < scene.Meshes.Count; j++)
                        {
                            Mesh mesh = scene.Meshes[j];
                            if (!mesh.Drawable) continue;
                            directional.DepthMap.SetTarget(context);
                            directional.Bind(context);
                            mesh.Material.Bind(context);
                            mesh.DrawAuto(context, materialDepthFrontface, directional.DepthMap.Viewport, null);
                        }
                        break;

                    case LightType.Point:
                        break;

                    case LightType.Spot:
                        break;

                    case LightType.Area:
                        break;
                }
            }

            gbuffers.Bind(context);
            ssaoEffect.Draw(context, camera);

            lightMap.ClearTarget(context, Vector4.Zero);
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];

                context.SetSampler(pointSampler, ShaderStage.Pixel, 0);
                gbuffers.Bind(context);
                envirr.Bind(context, 9);
                envfilter.Bind(context, 10);
                brdflut.Bind(context, 11);
                ssaoBuffer.Bind(context, 12);
                light.BindDepth(context, 8);
                lightMap.SetTarget(context);
                quad.Bind(context, out _, out int indexCount, out _);
                light.Render(context, lightMap.Viewport, SurfaceCamera.Instance, camera, indexCount);
            }

            fxaaBuffer.ClearTarget(context, Vector4.Zero);
            lightMap.Bind(context, 0);
            blendEffect.Draw(context, null);

            gbuffers.Bind(context);
            lightMap.Bind(context, 0);
            frontdepthbuffer.Bind(context, 3);
            ssrEffect.Draw(context, camera);

            ssrBuffer.Bind(context, 0);
            ssrBlurEffect.Draw(context, null);

            {
                fxaaBuffer.SetTarget(context);
                env.Bind(context);
                context.SetSampler(envsmp, ShaderStage.Pixel, 0);
                skycube.DrawAuto(context, skyboxShader, fxaaBuffer.Viewport, camera, Matrix4x4.Identity);
            }

            foreach (var item in scene.ForwardRenderers)
            {
                fxaaBuffer.SetTarget(context);
                item.Render(context, viewport, camera);
            }

            fxaaEffect.Draw(context, viewport);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                cameraBuffer.Dispose();
                quad.Dispose();
                skycube.Dispose();
                materialShader.Dispose();
                materialDepthBackface.Dispose();
                materialDepthFrontface.Dispose();
                skyboxShader.Dispose();
                gbuffers.Dispose();

                ssaoBuffer.Dispose();
                lightMap.Dispose();
                fxaaBuffer.Dispose();
                ssrBuffer.Dispose();
                frontdepthbuffer.Dispose();

                pointSampler.Dispose();

                ssaoEffect.Dispose();
                ssrEffect.Dispose();
                ssrBlurEffect.Dispose();
                blendEffect.Dispose();
                fxaaEffect.Dispose();

                brdfFilter.Dispose();

                envsmp.Dispose();
                env.Dispose();
                envirr.Dispose();
                envfilter.Dispose();
                brdflut.Dispose();

                disposedValue = true;
            }
        }

        ~DeferredRenderer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public struct CBCamera
    {
        public Matrix4x4 View;
        public Matrix4x4 Proj;
        public Matrix4x4 ViewInv;
        public Matrix4x4 ProjInv;

        public CBCamera(Camera camera)
        {
            Proj = Matrix4x4.Transpose(camera.Transform.Projection);
            View = Matrix4x4.Transpose(camera.Transform.View);
            ProjInv = Matrix4x4.Transpose(camera.Transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.Transform.ViewInv);
        }

        public CBCamera(CameraTransform camera)
        {
            Proj = Matrix4x4.Transpose(camera.Projection);
            View = Matrix4x4.Transpose(camera.View);
            ProjInv = Matrix4x4.Transpose(camera.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.ViewInv);
        }
    }

    public struct CBWorld
    {
        public Matrix4x4 World;
        public Matrix4x4 WorldInv;

        public CBWorld(Mesh mesh)
        {
            World = Matrix4x4.Transpose(mesh.Transform.Matrix);
            WorldInv = Matrix4x4.Transpose(mesh.Transform.MatrixInv);
        }
    }
}