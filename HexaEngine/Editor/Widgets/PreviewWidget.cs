namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Objects;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Pipelines.Deferred;
    using HexaEngine.Pipelines.Deferred.Lighting;
    using HexaEngine.Pipelines.Deferred.PrePass;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Pipelines.Forward;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.ConstantBuffers;
    using ImGuiNET;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class PreviewWidget : ImGuiWindow, IDisposable
    {
#nullable disable
        private bool isdrawing;
        private DepthBuffer depth;
        private RenderTexture target;
        private IntPtr id;
        private IRenderTargetView rtv;
        private RenderTextureArray gbuffer;
        private RenderTexture aoBuffer;
        private RenderTexture brdflut;
        private RenderTexture env;
        private RenderTexture irr;
        private RenderTexture pre;

        private ISamplerState ansioSampler;
        private ISamplerState pointSampler;

        private UVSphere sphere;
        private Quad quad;

        private PrepassShader prepass;
        private BRDFPipeline pbrlightShader;
        private BRDFLUT brdfFilter;
        private SkyboxPipeline skyboxShader;

        private Material material = new() { Albedo = new(1, 0, 0), Ao = 1 };

        private IBuffer vb;
        private IBuffer ib;
        private int indexCount;
        private IBuffer cbMaterial;
        private IBuffer cbWorld;
        private IBuffer cameraBuffer;
        private IBuffer lightBuffer;
        private IBuffer skyboxBuffer;

        private string pathEnvironment = string.Empty;
        private bool searchPathEnvironment;
        private FilePicker pickerEnv = new() { CurrentFolder = Environment.CurrentDirectory };

        private string pathIrradiance = string.Empty;
        private bool searchPathIrradiance;
        private FilePicker pickerIrr = new() { CurrentFolder = Environment.CurrentDirectory };

        private string pathPrefilter = string.Empty;
        private bool searchPathPrefilter;
        private FilePicker pickerPre = new() { CurrentFolder = Environment.CurrentDirectory };

        private Camera camera;

        protected override string Name => "PBR Preview";

        public override void Init(IGraphicsDevice device)
        {
            camera = new() { Height = 512, Width = 512, AutoSize = false };
            camera.Transform.Position = new(0, 0, -2);
            var context = device.Context;
            depth = new(device, new(512, 512, 1, Format.Depth24UNormStencil8, BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default));
            target = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1));
            id = ImGuiRenderer.RegisterTexture(target.ResourceView);
            rtv = target.RenderTargetView ?? throw new PlatformNotSupportedException();
            gbuffer = new(device, 512, 512, 8);
            aoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.R32Float));
            brdflut = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RG32Float));

#nullable disable
            device.Context.ClearRenderTargetView(aoBuffer.RenderTargetView, Vector4.One);
#nullable enable

            brdfFilter = new(device);
            brdfFilter.Target = brdflut.RenderTargetView;
            brdfFilter.Draw(context);
            brdfFilter.Target = null;
            brdfFilter.Dispose();

            cameraBuffer = device.CreateBuffer(new CBCamera(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            lightBuffer = device.CreateBuffer(new CBLight(Array.Empty<Light>()), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            skyboxBuffer = device.CreateBuffer(new CBWorld(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            ansioSampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            pointSampler = device.CreateSamplerState(SamplerDescription.PointClamp);

            prepass = new(device);
            prepass.Constants.Add(new(cameraBuffer, ShaderStage.Domain, 1));
            prepass.Samplers.Add(new(ansioSampler, ShaderStage.Domain, 0));
            prepass.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 0));

            pbrlightShader = new(device);
            pbrlightShader.Constants.Add(new(lightBuffer, ShaderStage.Pixel, 0));
            pbrlightShader.Constants.Add(new(cameraBuffer, ShaderStage.Pixel, 1));
            pbrlightShader.Samplers.Add(new(pointSampler, ShaderStage.Pixel, 0));
            pbrlightShader.Samplers.Add(new(ansioSampler, ShaderStage.Pixel, 1));

            skyboxShader = new(device);
            skyboxShader.Constants.Add(new(skyboxBuffer, ShaderStage.Vertex, 0));
            skyboxShader.Constants.Add(new(cameraBuffer, ShaderStage.Vertex, 1));

            sphere = new(device);
            quad = new(device);
            cbMaterial = device.CreateBuffer(new CBMaterial(material), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Immutable);

            UVSphere.CreateSphere(out MeshVertex[] vertices, out int[] indices, 64, 64);

            vb = device.CreateBuffer(vertices, BindFlags.VertexBuffer, Usage.Immutable);
            ib = device.CreateBuffer(indices, BindFlags.IndexBuffer, Usage.Immutable);
            indexCount = indices.Length;
        }

        public override void Dispose()
        {
            depth.DSV.Dispose();
            depth.Resource.Dispose();
            depth.SRV?.Dispose();
            ImGuiRenderer.UnregisterTexture(target.ResourceView);
            target.Dispose();
            gbuffer.Dispose();
            aoBuffer.Dispose();
            brdflut.Dispose();

            cameraBuffer.Dispose();
            lightBuffer.Dispose();
            skyboxBuffer.Dispose();

            ansioSampler.Dispose();
            pointSampler.Dispose();

            prepass.Dispose();

            pbrlightShader.Dispose();
            skyboxShader.Dispose();

            sphere.Dispose();
            quad.Dispose();
            cbMaterial.Dispose();
            cbWorld.Dispose();

            vb.Dispose();
            ib.Dispose();

            env?.Dispose();
            irr?.Dispose();
            pre?.Dispose();
        }

        private void TryLoadEnv(IGraphicsContext context)
        {
            env?.Dispose();
            try
            {
                env = new(context.Device, new TextureFileDescription(pathEnvironment, TextureDimension.TextureCube));
                isdrawing = env != null && irr != null && pre != null;
            }
            catch (Exception e)
            {
                ImGuiConsole.Log(e);
            }
        }

        private void TryLoadIrr(IGraphicsContext context)
        {
            irr?.Dispose();
            try
            {
                irr = new(context.Device, new TextureFileDescription(pathIrradiance, TextureDimension.TextureCube));
                isdrawing = env != null && irr != null && pre != null;
            }
            catch (Exception e)
            {
                ImGuiConsole.Log(e);
            }
        }

        private void TryLoadPre(IGraphicsContext context)
        {
            pre?.Dispose();
            try
            {
                pre = new(context.Device, new TextureFileDescription(pathPrefilter, TextureDimension.TextureCube));
                isdrawing = env != null && irr != null && pre != null;
            }
            catch (Exception e)
            {
                ImGuiConsole.Log(e);
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            Flags = ImGuiWindowFlags.None;

            IsDocked = ImGui.IsWindowDocked();

            if (ImGui.Button("Env"))
            {
                searchPathEnvironment = true;
            }
            ImGui.SameLine();
            if (ImGui.InputText("Environment", ref pathEnvironment, 1000))
            {
                TryLoadEnv(context);
            }

            if (ImGui.Button("Irr"))
            {
                searchPathIrradiance = true;
            }
            ImGui.SameLine();
            if (ImGui.InputText("Irradiation", ref pathIrradiance, 1000))
            {
                TryLoadIrr(context);
            }

            if (ImGui.Button("Pre"))
            {
                searchPathPrefilter = true;
            }
            ImGui.SameLine();
            if (ImGui.InputText("Prefilter", ref pathPrefilter, 1000))
            {
                TryLoadPre(context);
            }

            ImGui.Image(id, new(512, 512));

            ImGui.SameLine();

            ImGui.BeginChild("Settings");

            var color = material.Albedo;
            if (ImGui.ColorEdit3("Color", ref color, ImGuiColorEditFlags.Float))
                material.Albedo = color;

            var roughness = material.Roughness;
            if (ImGui.SliderFloat("Roughness", ref roughness, 0, 1))
                material.Roughness = roughness;

            var Metalness = material.Metalness;
            if (ImGui.SliderFloat("Metalness", ref Metalness, 0, 1))
                material.Metalness = Metalness;

            var Ao = material.Ao;
            if (ImGui.SliderFloat("Ao", ref Ao, 0, 1))
                material.Ao = Ao;

            ImGui.EndChild();

            EndWindow();

            if (searchPathEnvironment)
            {
                if (pickerEnv.Draw())
                {
                    pathEnvironment = pickerEnv.SelectedFile;
                    searchPathEnvironment = false;
                    TryLoadEnv(context);
                }
            }

            if (searchPathIrradiance)
            {
                if (pickerIrr.Draw())
                {
                    pathIrradiance = pickerIrr.SelectedFile;
                    searchPathIrradiance = false;
                    TryLoadIrr(context);
                }
            }

            if (searchPathPrefilter)
            {
                if (pickerPre.Draw())
                {
                    pathPrefilter = pickerPre.SelectedFile;
                    searchPathPrefilter = false;
                    TryLoadPre(context);
                }
            }

            if (isdrawing)
            {
#nullable disable
                context.Write(cameraBuffer, new CBCamera(camera));
                context.Write(skyboxBuffer, new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far) * Matrix4x4.CreateTranslation(camera.Transform.Position)));
                context.ClearDepthStencilView(depth.DSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

                // Fill Geometry Buffer
                context.ClearRenderTargetViews(gbuffer.RTVs, Vector4.Zero);
                context.SetRenderTargets(gbuffer.RTVs, depth.DSV);
                context.Write(cbMaterial, new CBMaterial(material));
                context.SetConstantBuffer(cbWorld, ShaderStage.Domain, 0);
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbMaterial, ShaderStage.Domain, 2);
                context.SetConstantBuffer(cbMaterial, ShaderStage.Pixel, 2);
                context.SetVertexBuffer(vb, Marshal.SizeOf<MeshVertex>());
                context.SetIndexBuffer(ib, Format.R32UInt, 0);
                prepass.DrawIndexed(context, gbuffer.Viewport, indexCount, 0, 0);
                context.ClearState();

                // Light pass
                context.ClearRenderTargetView(rtv, Vector4.Zero);
                context.SetShaderResources(gbuffer.SRVs, ShaderStage.Pixel, 0);
                context.SetShaderResource(irr.ResourceView, ShaderStage.Pixel, 8);
                context.SetShaderResource(pre.ResourceView, ShaderStage.Pixel, 9);
                context.SetShaderResource(brdflut.ResourceView, ShaderStage.Pixel, 10);
                context.SetShaderResource(aoBuffer.ResourceView, ShaderStage.Pixel, 11);
                context.SetRenderTarget(rtv, null);
                quad.DrawAuto(context, pbrlightShader, target.Viewport);
                context.ClearState();

                // Skybox pass
                context.SetShaderResource(env.ResourceView, ShaderStage.Pixel, 0);
                context.SetSampler(ansioSampler, ShaderStage.Pixel, 0);
                context.SetRenderTarget(rtv, depth.DSV);
                sphere.DrawAuto(context, skyboxShader, target.Viewport);
                context.ClearState();
#nullable enable
            }
        }
    }
}