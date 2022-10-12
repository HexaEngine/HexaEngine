namespace HexaEngine.Rendering
{
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Pipelines;
    using HexaEngine.Pipelines.Deferred;
    using HexaEngine.Pipelines.Deferred.Lighting;
    using HexaEngine.Pipelines.Deferred.PrePass;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Pipelines.Forward;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class DeferredRenderer : ISceneRenderer
    {
#nullable disable
        private ResourceManager resourceManager;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;

        private Quad quad;
        private UVSphere skycube;

        private IBuffer cameraBuffer;
        private IBuffer lightBuffer;
        private IBuffer skyboxBuffer;
        private PrepassShader materialShader;
        private MTLDepthShaderBack materialDepthBackface;
        private MTLDepthShaderFront materialDepthFrontface;
        private BRDFPipeline pbrlightShader;
        private FlatPipeline flatlightShader;
        private Pipeline activeShader;
        private SkyboxPipeline skyboxShader;

        private RenderTextureArray gbuffers;

        private RenderTexture ssaoBuffer;
        private RenderTexture lightMap;
        private RenderTexture fxaaBuffer;
        private RenderTexture ssrBuffer;
        private RenderTexture depthbuffer;

        private CSMPipeline csmPipeline;
        private RenderTexture csmDepthBuffer;
        private IBuffer csmMvpBuffer;

        private OSMPipeline osmPipeline;
        private IBuffer osmBuffer;
        private IBuffer osmParamBuffer;
        private RenderTexture[] osmDepthBuffers;
        private IShaderResourceView[] osmSRVs;

        private PSMPipeline psmPipeline;
        private IBuffer psmBuffer;
        private RenderTexture[] psmDepthBuffers;
        private IShaderResourceView[] psmSRVs;

        private ISamplerState pointSampler;
        private ISamplerState ansioSampler;
        private ISamplerState linearSampler;

        private HBAOEffect ssaoEffect;
        private DDASSREffect ssrEffect;
        private BlendBoxBlurEffect ssrBlurEffect;
        private BlendEffect blendEffect;
        private FXAAEffect fxaaEffect;

        private BRDFEffect brdfFilter;

        private ISamplerState envsmp;
        private RenderTexture env;
        private RenderTexture envirr;
        private RenderTexture envfilter;

        private RenderTexture brdflut;

        private ShadingModel currentShadingModel;
        private ShadingModel[] availableShadingModels;
        private string[] availableShadingModelStrings;
        private int currentShadingModelIndex;
        private bool enableSSR;
        private bool enableAO;
#nullable enable

        public DeferredRenderer()
        {
        }

        public unsafe void Initialize(IGraphicsDevice device, SdlWindow window)
        {
            resourceManager = new ResourceManager(device);
            availableShadingModels = new ShadingModel[] { ShadingModel.Flat, ShadingModel.PbrBrdf };
            availableShadingModelStrings = availableShadingModels.Select(x => x.ToString()).ToArray();
            currentShadingModel = ShadingModel.PbrBrdf;
            currentShadingModelIndex = Array.IndexOf(availableShadingModels, currentShadingModel);
            var size = sizeof(Matrix4x4);
            var size2 = sizeof(CBWorld);
            var size3 = sizeof(CBCamera);
            this.device = device;
            context = device.Context;
            swapChain = device.SwapChain ?? throw new NotSupportedException("Device needs a swapchain to operate properly");
            swapChain.Resizing += OnResizeBegin;
            swapChain.Resized += OnResizeEnd;

            quad = new(device);
            skycube = new(device);

            pointSampler = device.CreateSamplerState(SamplerDescription.PointClamp);
            ansioSampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            linearSampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            cameraBuffer = device.CreateBuffer(new CBCamera(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            lightBuffer = device.CreateBuffer(new CBLight(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            skyboxBuffer = device.CreateBuffer(new CBWorld(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            csmDepthBuffer = new(device, TextureDescription.CreateTexture2DArrayWithRTV(2048, 2048, 3, 1, Format.R32Float), DepthStencilDesc.Default);
            csmMvpBuffer = device.CreateBuffer(new Matrix4x4[16], BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            csmPipeline = new(device);
            csmPipeline.Constants.Add(new(csmMvpBuffer, ShaderStage.Geometry, 0));

            osmDepthBuffers = new RenderTexture[8];
            osmSRVs = new IShaderResourceView[8];
            for (int i = 0; i < 8; i++)
            {
                osmDepthBuffers[i] = new(device, TextureDescription.CreateTextureCubeWithRTV(2048, 1, Format.R32Float), DepthStencilDesc.Default);
                osmSRVs[i] = osmDepthBuffers[i].ResourceView;
            }

            osmBuffer = device.CreateBuffer(new Matrix4x4[6], BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            osmParamBuffer = device.CreateBuffer(new Vector4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            osmPipeline = new(device);
            osmPipeline.Constants.Add(new(osmBuffer, ShaderStage.Geometry, 0));
            osmPipeline.Constants.Add(new(osmParamBuffer, ShaderStage.Pixel, 0));

            psmDepthBuffers = new RenderTexture[8];
            psmSRVs = new IShaderResourceView[8];
            for (int i = 0; i < 8; i++)
            {
                psmDepthBuffers[i] = new(device, TextureDescription.CreateTexture2DWithRTV(2048, 2048, 1, Format.R32Float), DepthStencilDesc.Default);
                psmSRVs[i] = psmDepthBuffers[i].ResourceView;
            }

            psmBuffer = device.CreateBuffer(new Matrix4x4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            psmPipeline = new(device);
            psmPipeline.Constants.Add(new(psmBuffer, ShaderStage.Domain, 1));

            materialShader = new(device);
            materialShader.Constants.Add(new(cameraBuffer, ShaderStage.Domain, 1));
            materialDepthBackface = new(device);
            materialDepthBackface.Constants.Add(new(cameraBuffer, ShaderStage.Domain, 1));
            materialDepthFrontface = new(device);
            materialDepthFrontface.Constants.Add(new(cameraBuffer, ShaderStage.Domain, 1));
            pbrlightShader = new(device);
            pbrlightShader.Constants.Add(new(lightBuffer, ShaderStage.Pixel, 0));
            pbrlightShader.Constants.Add(new(cameraBuffer, ShaderStage.Pixel, 1));
            pbrlightShader.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));
            pbrlightShader.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 1));
            flatlightShader = new(device);
            flatlightShader.Constants.Add(new(lightBuffer, ShaderStage.Pixel, 0));
            flatlightShader.Constants.Add(new(cameraBuffer, ShaderStage.Pixel, 1));
            flatlightShader.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));
            flatlightShader.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 1));

            activeShader = pbrlightShader;

            skyboxShader = new(device);
            skyboxShader.Constants.Add(new(skyboxBuffer, ShaderStage.Vertex, 0));
            skyboxShader.Constants.Add(new(cameraBuffer, ShaderStage.Vertex, 1));

            gbuffers = new RenderTextureArray(device, window.Width, window.Height, 8, Format.RGBA32Float);

            depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(window.Width, window.Height, 1, Format.R32Float), DepthStencilDesc.Default);

            lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(window.Width, window.Height, 1));

            ssaoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(window.Width / 2, window.Height / 2, 1, Format.R32Float));
            ssaoEffect = new(device);
            ssaoEffect.Target = ssaoBuffer.RenderTargetView;
            ssaoEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            fxaaBuffer = new(device, swapChain.BackbufferDSV, TextureDescription.CreateTexture2DWithRTV(window.Width, window.Height, 1));
            fxaaEffect = new(device);
            fxaaEffect.Target = swapChain.BackbufferRTV;
            fxaaEffect.Resources.Add(new(fxaaBuffer.ResourceView, ShaderStage.Pixel, 0));
            fxaaEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(window.Width, window.Height, 1));
            ssrEffect = new(device);
            ssrEffect.Target = ssrBuffer.RenderTargetView;
            ssrEffect.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            ssrBlurEffect = new(device);
            ssrBlurEffect.Target = fxaaBuffer.RenderTargetView;
            ssrBlurEffect.Samplers.Add(new(linearSampler, ShaderStage.Pixel, 0));
            ssrBlurEffect.Resources.Add(new(ssrBuffer.ResourceView, ShaderStage.Pixel, 0));

            blendEffect = new(device);
            blendEffect.Target = fxaaBuffer.RenderTargetView;
            blendEffect.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 0));

            env = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "env_o.dds", TextureDimension.TextureCube));
            envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
            envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));

            envsmp = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

            brdflut = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RG32Float));

            brdfFilter = new(device);
            brdfFilter.Target = brdflut.RenderTargetView;
            brdfFilter.Draw(context);

            context.ClearState();
        }

        private void OnResizeBegin(object? sender, EventArgs e)
        {
            gbuffers.Dispose();
            lightMap.Dispose();
            ssaoBuffer.Dispose();
            fxaaBuffer.Dispose();
            ssrBuffer.Dispose();
            depthbuffer.Dispose();
        }

        private void OnResizeEnd(object? sender, ResizedEventArgs e)
        {
            gbuffers = new RenderTextureArray(device, e.NewWidth, e.NewHeight, 8, Format.RGBA32Float);
            depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1), DepthStencilDesc.Default);

            lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));

            ssaoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth / 2, e.NewHeight / 2, 1, Format.R32Float));
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

            gbuffers.SRVs[0].DebugName = "Albedo";
            gbuffers.SRVs[1].DebugName = "Position Depth";
            gbuffers.SRVs[2].DebugName = "Normal Roughness";
            gbuffers.SRVs[3].DebugName = "Clearcoat Metallness";
            gbuffers.SRVs[4].DebugName = "Emission";
            gbuffers.SRVs[5].DebugName = "Misc 0";
            gbuffers.SRVs[6].DebugName = "Misc 1";
            gbuffers.SRVs[7].DebugName = "Misc 2";
#nullable disable
            lightMap.ResourceView.DebugName = "Light Buffer";
            ssaoBuffer.ResourceView.DebugName = "SSAO/HBAO Buffer";
            ssrBuffer.ResourceView.DebugName = "SSR/DDASSR Buffer";
            fxaaBuffer.ResourceView.DebugName = "FXAA Buffer";
#nullable enable
        }

        public async Task Update(Scene scene)
        {
            while (scene.CommandQueue.TryDequeue(out var cmd))
            {
                if (cmd.Sender is SceneNode node)
                {
                    switch (cmd.Type)
                    {
                        case CommandType.Load:
                            for (int i = 0; i < node.Meshes.Count; i++)
                            {
                                await resourceManager.AsyncCreateInstance(node.Meshes[i], node);
                            }
                            break;

                        case CommandType.Unload:
                            for (int i = 0; i < node.Meshes.Count; i++)
                            {
                                await resourceManager.AsyncDestroyInstance(node.Meshes[i], node);
                            }
                            break;

                        case CommandType.Update:
                            if (cmd.Child is Mesh child)
                            {
                                switch (cmd.ChildCommand)
                                {
                                    case ChildCommandType.Added:
                                        await resourceManager.AsyncCreateInstance(child, node);
                                        break;

                                    case ChildCommandType.Removed:
                                        await resourceManager.AsyncDestroyInstance(child, node);
                                        break;
                                }
                            }
                            break;
                    }
                }

                if (cmd.Sender is Mesh mesh)
                {
                    await resourceManager.AsyncUpdateMesh(mesh);
                }

                if (cmd.Sender is Material material)
                {
                    await resourceManager.AsyncUpdateMaterial(material);
                }
            }
        }

        public void Render(IGraphicsContext context, SdlWindow window, Viewport viewport, Scene scene, Camera? camera)
        {
            if (camera == null) return;
            context.Write(cameraBuffer, new CBCamera(camera));
            context.Write(skyboxBuffer, new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far) * Matrix4x4.CreateTranslation(camera.Transform.Position)));
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            // Fill Geometry Buffer
            context.ClearRenderTargetViews(gbuffers.RTVs, Vector4.Zero);

            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                if (resourceManager.GetMesh(scene.Meshes[i], out var mesh))
                {
                    context.SetRenderTargets(gbuffers.RTVs, swapChain.BackbufferDSV);
                    mesh.DrawAuto(context, materialShader, gbuffers.Viewport);
                }
            }

            // Draw backface depth
            if (enableSSR)
            {
                depthbuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.Stencil | DepthStencilClearFlags.Depth);

                for (int i = 0; i < scene.Meshes.Count; i++)
                {
                    if (resourceManager.GetMesh(scene.Meshes[i], out var mesh))
                    {
                        depthbuffer.SetTarget(context);
                        mesh.DrawAuto(context, materialDepthBackface, depthbuffer.Viewport);
                    }
                }
            }

            var lights = new CBLight(scene.Lights);
            uint pointsd = 0;
            uint spotsd = 0;

            // Draw light depth
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];
                if (!light.CastShadows) continue;
                switch (light.Type)
                {
                    case LightType.Directional:
                        var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, light.Transform, lights.DirectionalLightSDs[0].Views, lights.DirectionalLightSDs[0].Cascades);
                        context.Write(csmMvpBuffer, mtxs);
                        csmDepthBuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);

                        for (int j = 0; j < scene.Meshes.Count; j++)
                        {
                            if (resourceManager.GetMesh(scene.Meshes[j], out var mesh))
                            {
                                context.SetRenderTarget(csmDepthBuffer.RenderTargetView, csmDepthBuffer.DepthStencilView);
                                mesh.DrawAuto(context, csmPipeline, csmDepthBuffer.Viewport);
                            }
                        }

                        break;

                    case LightType.Point:
                        var mt = OSMHelper.GetLightSpaceMatrices(light.Transform);
                        context.Write(osmBuffer, mt);
                        context.Write(osmParamBuffer, new Vector4(light.Transform.GlobalPosition, 25));
                        osmDepthBuffers[pointsd].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);

                        for (int j = 0; j < scene.Meshes.Count; j++)
                        {
                            if (resourceManager.GetMesh(scene.Meshes[j], out var mesh))
                            {
                                context.SetRenderTarget(osmDepthBuffers[pointsd].RenderTargetView, osmDepthBuffers[pointsd].DepthStencilView);
                                mesh.DrawAuto(context, osmPipeline, osmDepthBuffers[pointsd].Viewport);
                            }
                        }

                        pointsd++;
                        break;

                    case LightType.Spot:
                        var mts = PSMHelper.GetLightSpaceMatrices(light.Transform, ((Spotlight)light).ConeAngle.ToRad());
                        lights.SpotlightSDs[spotsd].View = mts[0];
                        context.Write(psmBuffer, mts);
                        psmDepthBuffers[spotsd].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);

                        for (int j = 0; j < scene.Meshes.Count; j++)
                        {
                            if (resourceManager.GetMesh(scene.Meshes[j], out var mesh))
                            {
                                context.SetRenderTarget(psmDepthBuffers[spotsd].RenderTargetView, psmDepthBuffers[spotsd].DepthStencilView);
                                mesh.DrawAuto(context, psmPipeline, psmDepthBuffers[spotsd].Viewport);
                            }
                        }

                        spotsd++;
                        break;
                }
            }

            // SSAO Pass
            ssaoBuffer.ClearTarget(context, Vector4.One);
            if (enableAO)
            {
                context.SetShaderResources(gbuffers.SRVs, ShaderStage.Pixel, 0);
                context.SetConstantBuffer(cameraBuffer, ShaderStage.Pixel, 1);
                ssaoEffect.Draw(context);
                context.ClearState();
            }

            // Light Pass
            lightMap.ClearTarget(context, Vector4.Zero);
            context.Write(lightBuffer, lights);
            context.SetShaderResources(gbuffers.SRVs, ShaderStage.Pixel, 0);
            context.SetShaderResource(envirr.ResourceView, ShaderStage.Pixel, 8);
            context.SetShaderResource(envfilter.ResourceView, ShaderStage.Pixel, 9);
            context.SetShaderResource(brdflut.ResourceView, ShaderStage.Pixel, 10);
            context.SetShaderResource(ssaoBuffer.ResourceView, ShaderStage.Pixel, 11);
            context.SetShaderResource(csmDepthBuffer.ResourceView, ShaderStage.Pixel, 12);
            context.SetShaderResources(osmSRVs, ShaderStage.Pixel, 13);
            context.SetShaderResources(psmSRVs, ShaderStage.Pixel, 21);
            lightMap.SetTarget(context);
            quad.DrawAuto(context, activeShader, lightMap.Viewport);
            context.ClearState();

            fxaaBuffer.ClearTarget(context, Vector4.Zero);
            context.SetShaderResource(lightMap.ResourceView, ShaderStage.Pixel, 0);
            blendEffect.Draw(context);
            context.ClearState();

            if (enableSSR)
            {
                context.SetShaderResources(gbuffers.SRVs, ShaderStage.Pixel, 0);
                context.SetShaderResource(lightMap.ResourceView, ShaderStage.Pixel, 0);
                context.SetShaderResource(depthbuffer.ResourceView, ShaderStage.Pixel, 3);
                context.SetConstantBuffer(cameraBuffer, ShaderStage.Pixel, 1);
                ssrEffect.Draw(context);
                context.ClearState();

                context.SetShaderResource(ssrBuffer.ResourceView, ShaderStage.Pixel, 0);
                ssrBlurEffect.Draw(context);
                context.ClearState();
            }

            {
                fxaaBuffer.SetTarget(context);
                context.SetShaderResource(env.ResourceView, ShaderStage.Pixel, 0);
                context.SetSampler(ansioSampler, ShaderStage.Pixel, 0);
                skycube.DrawAuto(context, skyboxShader, fxaaBuffer.Viewport);
                context.ClearState();
            }

            /*
            foreach (var item in scene.ForwardRenderers)
            {
                fxaaBuffer.SetTarget(context);
                item.Render(context, viewport, camera);
            }
            */

            fxaaEffect.Draw(context, viewport);
            context.ClearState();

            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            DebugDraw.Render(camera, viewport);
        }

        public void DrawSettings()
        {
            if (!ImGui.Begin("Renderer"))
            {
                ImGui.End();
                return;
            }
            if (ImGui.Button("Reload Skybox"))
            {
                env.Dispose();
                envirr.Dispose();
                envfilter.Dispose();
                env = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "env_o.dds", TextureDimension.TextureCube));
                envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
                envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));
            }

            if (ImGui.Combo("Shading Model", ref currentShadingModelIndex, availableShadingModelStrings, availableShadingModelStrings.Length))
            {
                currentShadingModel = availableShadingModels[currentShadingModelIndex];
                switch (currentShadingModel)
                {
                    case ShadingModel.Flat:
                        activeShader = flatlightShader;
                        break;

                    case ShadingModel.PbrBrdf:
                        activeShader = pbrlightShader;
                        break;
                }
            }

            ImGui.Separator();

            ImGui.Checkbox("Enable SSAO", ref enableAO);
            ImGui.Checkbox("Enable SSR", ref enableSSR);

            ssrEffect.DrawSettings();
            ssaoEffect.DrawSettings();
            ImGui.End();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                resourceManager.Dispose();
                lightBuffer.Dispose();
                cameraBuffer.Dispose();
                quad.Dispose();
                skycube.Dispose();
                materialShader.Dispose();
                materialDepthBackface.Dispose();
                materialDepthFrontface.Dispose();
                pbrlightShader.Dispose();
                flatlightShader.Dispose();
                skyboxShader.Dispose();
                gbuffers.Dispose();

                csmPipeline.Dispose();
                csmMvpBuffer.Dispose();
                csmDepthBuffer.Dispose();

                osmPipeline.Dispose();
                osmBuffer.Dispose();
                osmParamBuffer.Dispose();
                foreach (var buffer in osmDepthBuffers)
                    buffer.Dispose();

                psmPipeline.Dispose();
                psmBuffer.Dispose();
                foreach (var buffer in psmDepthBuffers)
                    buffer.Dispose();

                ssaoBuffer.Dispose();
                lightMap.Dispose();
                fxaaBuffer.Dispose();
                ssrBuffer.Dispose();
                depthbuffer.Dispose();

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
}