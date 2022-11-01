﻿namespace HexaEngine.Rendering
{
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Nodes;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Objects;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Pipelines.Deferred;
    using HexaEngine.Pipelines.Deferred.Lighting;
    using HexaEngine.Pipelines.Deferred.PrePass;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Pipelines.Forward;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Rendering.Passes;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class DeferredRenderer : ISceneRenderer
    {
#nullable disable
        private bool dirty = true;
        private ICommandList commandList;
        private RenderPassCollection passes = new();
        private NodeEditor graph = new();
        private Node root;
        private ResourceManager resourceManager;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private IGraphicsContext deferredContext;
        private ISwapChain swapChain;

        private Quad quad;
        private UVSphere skycube;

        private IBuffer cameraBuffer;
        private IBuffer lightBuffer;
        private IBuffer skyboxBuffer;
        private IBuffer tesselationBuffer;
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
        private RenderTexture tonemapBuffer;
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

        private HBAO ssao;
        private DDASSREffect ssr;
        private BlendBoxBlurEffect ssrBlurEffect;
        private BlendEffect blendEffect;
        private Tonemap tonemap;
        private FXAA fxaa;
        private Bloom bloom;

        private BRDFLUT brdfLUT;

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
        private bool enableBloom;

        private IBuffer gridVB;
        private IBuffer gridIB;
        private IBuffer gridISB;
        private int gridIndexCount;
        private GridShader gridShader;

        private IBuffer terrVB;
        private IBuffer terrIB;
        private IBuffer terrISB;
        private int terrIndexCount;
        private TerrainShader terrShader;
        private RenderTexture terrTexture;
        private RenderTexture terrHeightTexture;
        private RenderTexture terrMaskTexture;
#nullable enable

        public DeferredRenderer()
        {
        }

        public unsafe void Initialize(IGraphicsDevice device, SdlWindow window)
        {
            SceneManager.SceneChanged += SceneManager_SceneChanged;
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
            tesselationBuffer = device.CreateBuffer(new CBTessellation(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            passes = new();
            passes.AddSharedDepthView("SWAPCHAIN", swapChain.BackbufferDSV);
            passes.AddSharedRenderTarget("SWAPCHAIN", swapChain.BackbufferRTV);
            passes.AddSharedBuffer<CBCamera>(cameraBuffer);
            passes.Add(new PrePass(resourceManager));

            csmDepthBuffer = new(device, TextureDescription.CreateTexture2DArrayWithRTV(4096, 4096, 3, 1, Format.R32Float), DepthStencilDesc.Default);
            csmMvpBuffer = device.CreateBuffer(new Matrix4x4[16], BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            csmPipeline = new(device);
            csmPipeline.Constants.Add(new(csmMvpBuffer, ShaderStage.Geometry, 0));

            osmDepthBuffers = new RenderTexture[8];
            osmSRVs = new IShaderResourceView[8];
            for (int i = 0; i < 8; i++)
            {
                osmDepthBuffers[i] = new(device, TextureDescription.CreateTextureCubeWithRTV(4096, 1, Format.R32Float), DepthStencilDesc.Default);
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
                psmDepthBuffers[i] = new(device, TextureDescription.CreateTexture2DWithRTV(4096, 4096, 1, Format.R32Float), DepthStencilDesc.Default);
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
            ssao = new(device);
            ssao.Target = ssaoBuffer.RenderTargetView;
            ssao.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));

            fxaaBuffer = new(device, null, TextureDescription.CreateTexture2DWithRTV(window.Width, window.Height, 1));
            fxaa = new(device);
            fxaa.Target = swapChain.BackbufferRTV;
            fxaa.Resources.Add(new(fxaaBuffer.ResourceView, ShaderStage.Pixel, 0));
            fxaa.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 0));

            tonemapBuffer = new(device, swapChain.BackbufferDSV, TextureDescription.CreateTexture2DWithRTV(window.Width, window.Height, 1));
            tonemap = new(device);
            tonemap.Target = fxaaBuffer.RenderTargetView;
            tonemap.Samplers.Add(new(linearSampler, ShaderStage.Pixel, 0));
            tonemap.Resources.Add(new(tonemapBuffer.ResourceView, ShaderStage.Pixel, 0));

            bloom = new(device);
            bloom.Source = tonemapBuffer.ResourceView;
            bloom.Resize(device, window.Width, window.Height);
            tonemap.Resources.Add(new(bloom.Output, ShaderStage.Pixel, 1));

            ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(window.Width, window.Height, 1));
            ssr = new(device);
            ssr.Target = ssrBuffer.RenderTargetView;
            ssr.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 0));

            ssrBlurEffect = new(device);
            ssrBlurEffect.Target = tonemapBuffer.RenderTargetView;
            ssrBlurEffect.Samplers.Add(new(linearSampler, ShaderStage.Pixel, 0));
            ssrBlurEffect.Resources.Add(new(ssrBuffer.ResourceView, ShaderStage.Pixel, 0));

            blendEffect = new(device);
            blendEffect.Target = tonemapBuffer.RenderTargetView;
            blendEffect.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 0));

            env = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "env_o.dds", TextureDimension.TextureCube));
            envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
            envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));

            envsmp = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

            brdflut = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RG32Float));

            brdfLUT = new(device);
            brdfLUT.Target = brdflut.RenderTargetView;
            brdfLUT.Draw(context);

            context.ClearState();
            deferredContext = device.CreateDeferredContext();

            root = graph.CreateNode("Swapchain", false);
            root.CreatePin("Output", PinKind.Input, PinType.Texture2D, ImNodesNET.PinShape.Quad);
            graph.CreateFromPipeline(pbrlightShader, "BRDF");
            graph.CreateFromPipeline(materialShader, "Prepass");
            graph.CreateFromPipeline(osmPipeline, "Omni Depthpass");
            graph.CreateFromPipeline(psmPipeline, "Spot Depthpass");
            graph.CreateFromPipeline(csmPipeline, "Cascade Depthpass");
            graph.CreateFromEffect(fxaa, "FXAA");
            graph.CreateFromEffect(tonemap, "Tonemap");
            graph.CreateFromEffect(ssao, "HBAO");
            var bleeem = graph.CreateNode("Bloom");
            bleeem.CreatePin("in Image", PinKind.Input, PinType.Texture2D, ImNodesNET.PinShape.Quad);
            bleeem.CreatePin("out Image", PinKind.Output, PinType.Texture2D, ImNodesNET.PinShape.Quad);
            graph.AddNode(new ImageNode(graph, "BRDF LUT", false, true) { Image = brdflut.ResourceView });
            graph.AddNode(new ImageCubeNode(graph, "Irradiance", false, true) { Image = envirr.ResourceView });
            graph.AddNode(new ImageCubeNode(graph, "Prefilter", false, true) { Image = envfilter.ResourceView });

            passes.Initialize(device, window.Width, window.Height);

            {
                var res = Grid.GenerateGrid();
                gridVB = device.CreateBuffer(res.Item1, BindFlags.VertexBuffer, Usage.Immutable);
                gridIB = device.CreateBuffer(res.Item2, BindFlags.IndexBuffer, Usage.Immutable);

                gridShader = new(device);
                var inst = new Matrix4x4[4];
                int a = 0;
                for (int i = -1; i < 1; i++)
                {
                    for (int j = -1; j < 1; j++)
                    {
                        Matrix4x4 translation = Matrix4x4.CreateTranslation(i * 255, 0, j * 255);
                        inst[a++] = translation;
                    }
                }
                gridIndexCount = res.Item2.Length;
                gridISB = device.CreateBuffer(inst, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            }

            {
                Terrain map = new(256, 256, 1);

                terrVB = device.CreateBuffer(map.Vertices, BindFlags.VertexBuffer, Usage.Immutable);
                terrIB = device.CreateBuffer(map.Indices, BindFlags.IndexBuffer, Usage.Immutable);

                terrShader = new(device);
                terrTexture = RenderTexture.Combine2D(device, Paths.CurrentTexturePath + "layer0.dds", Paths.CurrentTexturePath + "layer1.dds", Paths.CurrentTexturePath + "layer2.dds");
                terrHeightTexture = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "terrain-height.png"));
                terrMaskTexture = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "terrain-mask.png"));
                terrShader.Resources.Add(new(terrHeightTexture.ResourceView, ShaderStage.Domain, 0));
                terrShader.Resources.Add(new(terrTexture.ResourceView, ShaderStage.Pixel, 0));
                terrShader.Resources.Add(new(terrMaskTexture.ResourceView, ShaderStage.Pixel, 1));
                terrShader.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 0));
                terrShader.Samplers.Add(new(linearSampler, ShaderStage.Domain, 0));
                var inst = new Matrix4x4[4];
                int a = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Matrix4x4 translation = Matrix4x4.CreateTranslation(i * 255, 0, j * 255);
                        inst[a++] = translation;
                    }
                }
                terrIndexCount = map.Indices.Length;
                terrISB = device.CreateBuffer(inst, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            }
        }

        private void SceneManager_SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            if (e.Old != null && e.Old != e.New)
            {
                Update(e.Old).Wait();
            }
        }

        private void OnResizeBegin(object? sender, EventArgs e)
        {
            gbuffers.Dispose();
            lightMap.Dispose();
            ssaoBuffer.Dispose();
            fxaaBuffer.Dispose();
            tonemapBuffer.Dispose();
            ssrBuffer.Dispose();
            depthbuffer.Dispose();
        }

        private void OnResizeEnd(object? sender, ResizedEventArgs e)
        {
            dirty = true;
            gbuffers = new RenderTextureArray(device, e.NewWidth, e.NewHeight, 8, Format.RGBA32Float);
            depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1), DepthStencilDesc.Default);

            lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));

            ssaoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth / 2, e.NewHeight / 2, 1, Format.R32Float));
            ssao.Target = ssaoBuffer.RenderTargetView;

            fxaaBuffer = new(device, null, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));
            fxaa.Target = swapChain.BackbufferRTV;
            fxaa.Resources.Clear();
            fxaa.Resources.Add(new(fxaaBuffer.ResourceView, ShaderStage.Pixel, 0));

            tonemapBuffer = new(device, swapChain.BackbufferDSV, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));
            tonemap.Target = fxaaBuffer.RenderTargetView;
            tonemap.Resources.Clear();
            tonemap.Resources.Add(new(tonemapBuffer.ResourceView, ShaderStage.Pixel, 0));

            bloom.Source = tonemapBuffer.ResourceView;
            bloom.Resize(device, e.NewWidth, e.NewHeight);
            tonemap.Resources.Add(new(bloom.Output, ShaderStage.Pixel, 1));

            ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(e.NewWidth, e.NewHeight, 1));
            ssr.Target = ssrBuffer.RenderTargetView;

            ssrBlurEffect.Target = tonemapBuffer.RenderTargetView;
            ssrBlurEffect.Resources.Clear();
            ssrBlurEffect.Resources.Add(new(ssrBuffer.ResourceView, ShaderStage.Pixel, 0));

            blendEffect.Target = tonemapBuffer.RenderTargetView;
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
                                await resourceManager.AsyncCreateInstance(scene.Meshes[node.Meshes[i]], node);
                                dirty = true;
                            }
                            break;

                        case CommandType.Unload:
                            for (int i = 0; i < node.Meshes.Count; i++)
                            {
                                await resourceManager.AsyncDestroyInstance(scene.Meshes[node.Meshes[i]], node);
                                dirty = true;
                            }
                            break;

                        case CommandType.Update:
                            if (cmd.Child is int child)
                            {
                                switch (cmd.ChildCommand)
                                {
                                    case ChildCommandType.Added:
                                        await resourceManager.AsyncCreateInstance(scene.Meshes[child], node);
                                        dirty = true;
                                        break;

                                    case ChildCommandType.Removed:
                                        await resourceManager.AsyncDestroyInstance(scene.Meshes[child], node);
                                        dirty = true;
                                        break;
                                }
                            }
                            break;
                    }
                }

                if (cmd.Sender is Mesh mesh)
                {
                    await resourceManager.AsyncUpdateMesh(mesh);
                    dirty = true;
                }

                if (cmd.Sender is Material material)
                {
                    await resourceManager.AsyncUpdateMaterial(material);
                    dirty = true;
                }

                if (cmd.Sender is Light)
                {
                    switch (cmd.Type)
                    {
                        case CommandType.Update:
                            dirty = true;
                            break;
                    }
                }
            }
        }

        private unsafe void RenderTerrain(Pipeline pipeline, Viewport viewport)
        {
            context.SetConstantBuffer(tesselationBuffer, ShaderStage.Vertex, 2);
            context.SetConstantBuffer(cameraBuffer, ShaderStage.Vertex, 1);
            context.SetConstantBuffer(cameraBuffer, ShaderStage.Domain, 1);
            context.SetVertexBuffer(terrVB, sizeof(TerrainVertex));
            context.SetIndexBuffer(terrIB, Format.R32UInt, 0);
            context.SetVertexBuffer(1, terrISB, sizeof(Matrix4x4));
            pipeline.DrawIndexedInstanced(context, viewport, terrIndexCount, 1, 0, 0, 0);
            context.ClearState();
        }

        public unsafe void Render(IGraphicsContext context, SdlWindow window, Viewport viewport, Scene scene, Camera? camera)
        {
            if (camera == null) return;
            context.Write(cameraBuffer, new CBCamera(camera));
            context.Write(skyboxBuffer, new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far) * Matrix4x4.CreateTranslation(camera.Transform.Position)));
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            /*
             passes.Update(context, scene);
             if (dirty)
             {
                 commandList?.Dispose();
                 passes.Draw(deferredContext, scene, viewport);
                 commandList = deferredContext.FinishCommandList(0);
                 dirty = false;
             }

             context.ExecuteCommandList(commandList, 0);

             return;*/

            // Fill Geometry Buffer
            //context.ClearRenderTargetViews(gbuffers.RTVs, Vector4.Zero);

            context.SetRenderTargets(gbuffers.RTVs, swapChain.BackbufferDSV);
            RenderTerrain(terrShader, gbuffers.Viewport);

            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                if (resourceManager.GetMesh(scene.Meshes[i], out var mesh))
                {
                    mesh.UpdateInstanceBuffer(context);
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
            context.Write(lightBuffer, lights);

            uint pointsd = 0;
            uint spotsd = 0;

            // Draw light depth
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];
                if (!light.CastShadows || !light.Updated) continue;
                light.Updated = false;
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
                ssao.Draw(context);
                context.ClearState();
            }

            // Light Pass
            lightMap.ClearTarget(context, Vector4.Zero);
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

            context.SetShaderResource(lightMap.ResourceView, ShaderStage.Pixel, 0);
            blendEffect.Draw(context);
            context.ClearState();

            if (enableSSR)
            {
                context.SetShaderResources(gbuffers.SRVs, ShaderStage.Pixel, 0);
                context.SetShaderResource(lightMap.ResourceView, ShaderStage.Pixel, 0);
                context.SetShaderResource(depthbuffer.ResourceView, ShaderStage.Pixel, 3);
                context.SetConstantBuffer(cameraBuffer, ShaderStage.Pixel, 1);
                ssr.Draw(context);
                context.ClearState();

                context.SetShaderResource(ssrBuffer.ResourceView, ShaderStage.Pixel, 0);
                ssrBlurEffect.Draw(context);
                context.ClearState();
            }

            {
                tonemapBuffer.SetTarget(context);
                context.SetShaderResource(env.ResourceView, ShaderStage.Pixel, 0);
                context.SetSampler(ansioSampler, ShaderStage.Pixel, 0);
                skycube.DrawAuto(context, skyboxShader, tonemapBuffer.Viewport);
                context.ClearState();
            }

            /*
            foreach (var item in scene.ForwardRenderers)
            {
                fxaaBuffer.SetTarget(deferredContext);
                item.Render(deferredContext, viewport, camera);
            }
            */

            if (enableBloom)
            {
                bloom.Draw(context);
                context.ClearState();
            }

            tonemap.Draw(context);
            context.ClearState();

            fxaa.Draw(context, viewport);
            context.ClearState();

            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            DebugDraw.Render(camera, viewport);
            /*
            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            context.SetConstantBuffer(cameraBuffer, ShaderStage.Vertex, 1);

            context.SetVertexBuffer(gridVB, sizeof(VertexPositionColor));
            context.SetIndexBuffer(gridIB, Format.R32UInt, 0);
            context.SetVertexBuffer(1, gridISB, sizeof(Matrix4x4));
            gridShader.DrawIndexedInstanced(context, fxaaBuffer.Viewport, gridIndexCount, 4, 0, 0, 0);*/
        }

        private Node[][] ExecutionOrder;

        public void DrawSettings()
        {
            ImGui.Begin("Graph Editor");
            if (ImGui.Button("Traverse"))
            {
                ExecutionOrder = NodeEditor.TreeTraversal2(root, false);
            }

            if (ExecutionOrder != null)
            {
                if (ImGui.CollapsingHeader("ExecutionOrder"))
                {
                    for (int i = 0; i < ExecutionOrder.Length; i++)
                    {
                        ImGui.TreePush();
                        for (int j = 0; j < ExecutionOrder[i].Length; j++)
                        {
                            ImGui.TreeNode(ExecutionOrder[i][j].Name);
                        }
                    }
                    for (int i = 0; i < ExecutionOrder.Length; i++)
                    {
                        ImGui.TreePop();
                    }
                }
            }

            graph.Draw();

            ImGui.End();

            if (!ImGui.Begin("Renderer"))
            {
                ImGui.End();
                return;
            }

            {
                bool vsync = swapChain.VSync;
                if (ImGui.Checkbox("VSync", ref vsync))
                {
                    swapChain.VSync = vsync;
                }

                ImGui.BeginDisabled(vsync);

                bool limitFPS = swapChain.LimitFPS;
                if (ImGui.Checkbox("Limit FPS", ref limitFPS))
                {
                    swapChain.LimitFPS = limitFPS;
                }

                int target = swapChain.TargetFPS;
                if (ImGui.InputInt("FPS Target", ref target, 1, 2, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    swapChain.TargetFPS = target;
                }

                ImGui.EndDisabled();
            }

            if (ImGui.Button("Reload Skybox"))
            {
                env.Dispose();
                envirr.Dispose();
                envfilter.Dispose();
                env = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "env_o.dds", TextureDimension.TextureCube));
                envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
                envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));
                dirty = true;
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
                dirty = true;
            }

            ImGui.Separator();

            if (ImGui.Checkbox("Enable SSAO", ref enableAO))
                dirty = true;
            if (ImGui.Checkbox("Enable SSR", ref enableSSR))
                dirty = true;
            if (ImGui.Checkbox("Enable Bloom", ref enableBloom))
                dirty = true;

            ssr.DrawSettings();
            ssao.DrawSettings();
            bloom.DrawSettings();
            tonemap.DrawSettings();
            ImGui.End();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                commandList?.Dispose();
                deferredContext.Dispose();
                resourceManager.Dispose();
                lightBuffer.Dispose();
                cameraBuffer.Dispose();
                tesselationBuffer.Dispose();
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
                tonemapBuffer.Dispose();
                ssrBuffer.Dispose();
                depthbuffer.Dispose();

                pointSampler.Dispose();

                ssao.Dispose();
                ssr.Dispose();
                ssrBlurEffect.Dispose();
                blendEffect.Dispose();
                fxaa.Dispose();
                tonemap.Dispose();
                bloom.Dispose();

                brdfLUT.Dispose();

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

    public class UniversalDeferredRenderer
    {
    }
}