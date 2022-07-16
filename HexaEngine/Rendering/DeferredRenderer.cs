namespace HexaEngine.Rendering
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
    using HexaEngine.Pipelines;
    using HexaEngine.Pipelines.Deferred;
    using HexaEngine.Pipelines.Deferred.PrePass;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Pipelines.Forward;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public unsafe class DeferredRenderer : ISceneRenderer
    {
        public class ModelTexture
        {
            public string Name;
            public IShaderResourceView SRV;
            public int Instances;

            public ModelTexture(string name, IShaderResourceView sRV, int instances)
            {
                Name = name;
                SRV = sRV;
                Instances = instances;
            }
        }

        public class ModelMaterial
        {
            public IBuffer CB;
            public ModelTexture? AlbedoTexture;
            public ModelTexture? NormalTexture;
            public ModelTexture? DisplacementTexture;
            public ModelTexture? RoughnessTexture;
            public ModelTexture? MetalnessTexture;
            public ModelTexture? EmissiveTexture;
            public ModelTexture? AoTexture;
            public int Instances;

            public ModelMaterial(IBuffer cB)
            {
                CB = cB;
            }
        }

        public class ModelMesh
        {
            public IBuffer? VB;
            public IBuffer? IB;
            public IBuffer? ISB;
            public int VertexCount;
            public int IndexCount;
            public int InstanceCount;
            public ModelMaterial? Material;
            public bool Drawable;
            public BoundingBox Box;
        }

        private readonly Dictionary<Mesh, ModelMesh> meshes = new();
        private readonly Dictionary<string, ModelMaterial> materials = new();
        private readonly Dictionary<string, ModelTexture> textures = new();

#nullable disable
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;

        private Quad quad;
        private UVSphere skycube;

        private IBuffer cameraBuffer;
        private IBuffer lightBuffer;
        private IBuffer skyboxBuffer;
        private MTLShader materialShader;
        private MTLDepthShaderBack materialDepthBackface;
        private MTLDepthShaderFront materialDepthFrontface;
        private PBRBRDFPipeline pbrlightShader;
        private SkyboxPipeline skyboxShader;

        private RenderTextureArray gbuffers;

        private RenderTexture shadowMap;
        private RenderTexture ssaoBuffer;
        private RenderTexture lightMap;
        private RenderTexture fxaaBuffer;
        private RenderTexture ssrBuffer;
        private RenderTexture frontdepthbuffer;

        private ISamplerState pointSampler;

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
            ImGui.StyleColorsDark();
        }

        public void Initialize(IGraphicsDevice device, SdlWindow window)
        {
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

            cameraBuffer = device.CreateBuffer(new CBCamera(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            lightBuffer = device.CreateBuffer(new CBLight(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            skyboxBuffer = device.CreateBuffer(new CBWorld(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
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
            skyboxShader = new(device);
            skyboxShader.Constants.Add(new(skyboxBuffer, ShaderStage.Vertex, 0));
            skyboxShader.Constants.Add(new(cameraBuffer, ShaderStage.Vertex, 1));

            gbuffers = new RenderTextureArray(device, 1280, 720, 8, Format.RGBA32Float);
            gbuffers.RenderTargets.DepthStencil = swapChain.BackbufferDSV;
            gbuffers.Add(new(ShaderStage.Pixel, 0));

            frontdepthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(1280, 720, 1, Format.R32Float), DepthStencilDesc.Default);

            lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(1280, 720, 1));
            shadowMap = new(device, TextureDescription.CreateTexture2DArrayWithRTV(IResource.MaximumTexture2DSize, IResource.MaximumTexture2DSize, 1, 1, Format.R32Float));

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
            envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
            envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));

            envsmp = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

            brdflut = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RG32Float));

            brdfFilter = new(device);
            brdfFilter.Target = brdflut.RenderTargetView;
            brdfFilter.Draw(context);

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
#nullable disable
                lightMap.ResourceView,
                ssaoBuffer.ResourceView,
                ssrBuffer.ResourceView,
                fxaaBuffer.ResourceView,
#nullable enable
            });
        }

        private void OnResizeBegin(object? sender, EventArgs e)
        {
            FramebufferDebugger.Clear();
            gbuffers.Dispose();
            lightMap.Dispose();
            ssaoBuffer.Dispose();
            fxaaBuffer.Dispose();
            ssrBuffer.Dispose();
            frontdepthbuffer.Dispose();
        }

        private void OnResizeEnd(object? sender, ResizedEventArgs e)
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
                #nullable disable
                lightMap.ResourceView,
                ssaoBuffer.ResourceView,
                ssrBuffer.ResourceView,
                fxaaBuffer.ResourceView,
                #nullable enable
            });

            gbuffers.GetResourceView(0).DebugName = "Albedo";
            gbuffers.GetResourceView(1).DebugName = "Position Depth";
            gbuffers.GetResourceView(2).DebugName = "Normal Roughness";
            gbuffers.GetResourceView(3).DebugName = "Clearcoat Metallness";
            gbuffers.GetResourceView(4).DebugName = "Emission";
            gbuffers.GetResourceView(5).DebugName = "Misc 0";
            gbuffers.GetResourceView(6).DebugName = "Misc 1";
            gbuffers.GetResourceView(7).DebugName = "Misc 2";
#nullable disable
            lightMap.ResourceView.DebugName = "Light Buffer";
            ssaoBuffer.ResourceView.DebugName = "SSAO/HBAO Buffer";
            ssrBuffer.ResourceView.DebugName = "SSR/DDASSR Buffer";
            fxaaBuffer.ResourceView.DebugName = "FXAA Buffer";
#nullable enable
        }

        private void Update(Scene scene)
        {
            while (scene.CommandQueue.TryDequeue(out var cmd))
            {
                if (cmd.Sender is Mesh mesh)
                {
                    switch (cmd.Type)
                    {
                        case CommandType.Load:
                            {
                                ModelMesh model = new();
                                if (mesh.Vertices != null)
                                {
                                    IBuffer vb = device.CreateBuffer(mesh.Vertices, BindFlags.VertexBuffer);
                                    model.VB = vb;
                                    model.VertexCount = mesh.Vertices.Length;
                                }
                                if (mesh.Indices != null)
                                {
                                    IBuffer ib = device.CreateBuffer(mesh.Indices, BindFlags.IndexBuffer);
                                    model.IB = ib;
                                    model.IndexCount = mesh.Indices.Length;
                                }
                                if (!string.IsNullOrEmpty(mesh.MaterialName))
                                {
                                    model.Material = materials.GetValueOrDefault(mesh.MaterialName);
                                }

                                model.Drawable = model.VB != null && model.IB != null && model.ISB != null && model.Material != null;
                                meshes.Add(mesh, model);
                            }
                            break;

                        case CommandType.Unload:
                            {
                                var model = meshes[mesh];
                                model.VB?.Dispose();
                                model.IB?.Dispose();
                                model.ISB?.Dispose();
                                meshes.Remove(mesh);
                            }
                            break;

                        case CommandType.Update:
                            {
                                var model = meshes[mesh];
                                if (mesh.Vertices != null)
                                {
                                    model.VB?.Dispose();
                                    IBuffer vb = device.CreateBuffer(mesh.Vertices, BindFlags.VertexBuffer);
                                    model.VB = vb;
                                    model.VertexCount = mesh.Vertices.Length;
                                }
                                if (mesh.Indices != null)
                                {
                                    model.IB?.Dispose();
                                    IBuffer ib = device.CreateBuffer(mesh.Indices, BindFlags.IndexBuffer);
                                    model.IB = ib;
                                    model.IndexCount = mesh.Indices.Length;
                                }
                                if (string.IsNullOrEmpty(mesh.MaterialName))
                                {
                                    model.Material = materials.GetValueOrDefault(mesh.MaterialName);
                                }
                            }
                            break;
                    }
                }

                if (cmd.Sender is Material material)
                {
                    switch (cmd.Type)
                    {
                        case CommandType.Load:
                            {
                                ModelMaterial modelMaterial = new(device.CreateBuffer(new CBMaterial(material), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
                                modelMaterial.AlbedoTexture = LoadTexture(material.AlbedoTextureMap);
                                modelMaterial.NormalTexture = LoadTexture(material.NormalTextureMap);
                                modelMaterial.DisplacementTexture = LoadTexture(material.DisplacementTextureMap);
                                modelMaterial.RoughnessTexture = LoadTexture(material.RoughnessTextureMap);
                                modelMaterial.MetalnessTexture = LoadTexture(material.MetalnessTextureMap);
                                modelMaterial.EmissiveTexture = LoadTexture(material.EmissiveTextureMap);
                                modelMaterial.AoTexture = LoadTexture(material.AoTextureMap);
                                materials.Add(material.Name, modelMaterial);
                            }
                            break;

                        case CommandType.Unload:
                            {
                                ModelMaterial modelMaterial = new(device.CreateBuffer(new CBMaterial(material), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
                                materials.Remove(material.Name);
                                UnloadTexture(modelMaterial.AlbedoTexture);
                                UnloadTexture(modelMaterial.NormalTexture);
                                UnloadTexture(modelMaterial.DisplacementTexture);
                                UnloadTexture(modelMaterial.RoughnessTexture);
                                UnloadTexture(modelMaterial.MetalnessTexture);
                                UnloadTexture(modelMaterial.EmissiveTexture);
                                UnloadTexture(modelMaterial.AoTexture);
                            }
                            break;

                        case CommandType.Update:
                            {
                                ModelMaterial modelMaterial = materials[material.Name];
                                UpdateTexture(ref modelMaterial.AlbedoTexture, material.AlbedoTextureMap);
                                UpdateTexture(ref modelMaterial.NormalTexture, material.NormalTextureMap);
                                UpdateTexture(ref modelMaterial.DisplacementTexture, material.DisplacementTextureMap);
                                UpdateTexture(ref modelMaterial.RoughnessTexture, material.RoughnessTextureMap);
                                UpdateTexture(ref modelMaterial.MetalnessTexture, material.MetalnessTextureMap);
                                UpdateTexture(ref modelMaterial.EmissiveTexture, material.EmissiveTextureMap);
                                UpdateTexture(ref modelMaterial.AoTexture, material.AoTextureMap);
                                context.Write(modelMaterial.CB, new CBMaterial(material));
                            }
                            break;
                    }
                }
            }
        }

        public ModelTexture? LoadTexture(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (textures.TryGetValue(name, out var texture))
            {
                texture.Instances++;
                return texture;
            }
            var tex = device.LoadTexture2D(name);
            texture = new(name, device.CreateShaderResourceView(tex), 1);
            tex.Dispose();
            textures.Add(name, texture);
            return texture;
        }

        public void UnloadTexture(ModelTexture? texture)
        {
            if (texture == null) return;
            texture.Instances--;
            if (texture.Instances == 0)
            {
                textures.Remove(texture.Name);
                texture.SRV.Dispose();
            }
        }

        public void UpdateTexture(ref ModelTexture? texture, string name)
        {
            if (texture?.Name == name) return;
            UnloadTexture(texture);
            texture = LoadTexture(name);
        }

        public void Render(IGraphicsContext context, SdlWindow window, Viewport viewport, Scene scene, Camera camera)
        {
            Update(scene);
            context.Write(cameraBuffer, new CBCamera(camera));
            context.Write(skyboxBuffer, new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far) * Matrix4x4.CreateTranslation(camera.Transform.Position)));
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            // Fill Geometry Buffer
            gbuffers.RenderTargets.ClearTargets(context);
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                Mesh mesh = scene.Meshes[i];
                if (!mesh.Drawable) continue;
                gbuffers.RenderTargets.SetTargets(context);
                mesh.Material.Bind(context);
                mesh.DrawAuto(context, materialShader, gbuffers.Viewport);
            }

            // Draw backface depth
            frontdepthbuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.Stencil | DepthStencilClearFlags.Depth);
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                Mesh mesh = scene.Meshes[i];
                if (!mesh.Drawable) continue;
                frontdepthbuffer.SetTarget(context);
                mesh.Material.Bind(context);
                mesh.DrawAuto(context, materialDepthBackface, frontdepthbuffer.Viewport);
            }

            // Draw light depth
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];
                if (!light.CastShadows) continue;
                switch (light.Type)
                {
                    case LightType.Directional:
                        break;

                    case LightType.Point:
                        break;

                    case LightType.Spot:
                        break;
                }
            }

            gbuffers.Bind(context);
            context.SetConstantBuffer(cameraBuffer, ShaderStage.Pixel, 1);
            ssaoBuffer.ClearTarget(context, Vector4.One);
            ssaoEffect.Draw(context);

            lightMap.ClearTarget(context, Vector4.Zero);
            context.Write(lightBuffer, new CBLight(scene.Lights));
            gbuffers.Bind(context);
            envirr.Bind(context, 9);
            envfilter.Bind(context, 10);
            brdflut.Bind(context, 11);
            ssaoBuffer.Bind(context, 12);
            lightMap.SetTarget(context);
            quad.DrawAuto(context, pbrlightShader, lightMap.Viewport);

            fxaaBuffer.ClearTarget(context, Vector4.Zero);
            lightMap.Bind(context, 0);
            blendEffect.Draw(context);

            if (enableSSR)
            {
                gbuffers.Bind(context);
                context.SetConstantBuffer(cameraBuffer, ShaderStage.Pixel, 1);
                lightMap.Bind(context, 0);
                frontdepthbuffer.Bind(context, 3);
                ssrEffect.Draw(context);

                ssrBuffer.Bind(context, 0);
                ssrBlurEffect.Draw(context);
            }

            {
                fxaaBuffer.SetTarget(context);
                context.SetShaderResource(env.ResourceView, ShaderStage.Pixel, 0);
                context.SetSampler(envsmp, ShaderStage.Pixel, 0);
                skycube.DrawAuto(context, skyboxShader, fxaaBuffer.Viewport);
            }

            /*
            foreach (var item in scene.ForwardRenderers)
            {
                fxaaBuffer.SetTarget(context);
                item.Render(context, viewport, camera);
            }
            */

            fxaaBuffer.SetTarget(context);
            DebugDraw.Render(camera, fxaaBuffer.Viewport);
            fxaaEffect.Draw(context, viewport);
        }

        public void DrawSettings()
        {
            if (ImGui.Combo("Shading Model", ref currentShadingModelIndex, availableShadingModelStrings, availableShadingModelStrings.Length))
            {
                currentShadingModel = availableShadingModels[currentShadingModelIndex];
            }

            ImGui.Separator();

            ImGui.Checkbox("Enable SSAO", ref enableAO);
            ImGui.Checkbox("Enable SSR", ref enableSSR);

            ssrEffect.DrawSettings();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                shadowMap.Dispose();
                lightBuffer.Dispose();
                cameraBuffer.Dispose();
                quad.Dispose();
                skycube.Dispose();
                materialShader.Dispose();
                materialDepthBackface.Dispose();
                materialDepthFrontface.Dispose();
                pbrlightShader.Dispose();
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
}