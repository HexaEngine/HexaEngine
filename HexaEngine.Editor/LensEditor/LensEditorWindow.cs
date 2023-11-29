namespace HexaEngine.Editor.LensEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.LensEditor.Generator;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx.BuildIn;
    using System.Numerics;
    using System.Reflection;
    using static HexaEngine.PostFx.BuildIn.ColorGrading;

    [EditorWindowCategory("Tools")]
    public class LensEditorWindow : EditorWindow
    {
        private List<ILensEffect> effects = [];

        private readonly DirectionalLight directionalLight = new();

        public struct LensFlareParams
        {
            public Vector4 SunPosition;
            public Vector4 Tint;

            public LensFlareParams(Vector4 sunPosition, Vector4 tint)
            {
                SunPosition = sunPosition;
                Tint = tint;
            }
        }

        private ConstantBuffer<ColorGradingParams> colorGradingParams;
        private ConstantBuffer<LensFlareParams> lensParams;
        private ConstantBuffer<CBCamera> view;
        private ConstantBuffer<CBWeather> weather;

        private DepthStencil depthStencil;
        private Texture2D textureTonemap;
        private Texture2D texturePreview;

        private IGraphicsDevice device;
        private IGraphicsPipeline? pipeline;
        private ISamplerState sampler;
        private IGraphicsPipeline tonemap;
        private IGraphicsPipeline fxaa;

        private Vector3 sc = new(2, 0, 0);
        private const float speed = 1;
        private static bool first = true;

        private readonly CameraTransform camera = new();
        private (string, Type)[] lensEffects;
        private LensGenerator generator = new();
        private bool autoGenerate = true;
        private volatile bool compiling;
        private readonly SemaphoreSlim semaphore = new(1);

        public LensEditorWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name { get; } = "Lens Editor";

        protected override void InitWindow(IGraphicsDevice device)
        {
            this.device = device;

            colorGradingParams = new(device, new ColorGradingParams(), CpuAccessFlags.Write);
            lensParams = new(device, CpuAccessFlags.Write);
            view = new(device, CpuAccessFlags.Write);
            weather = new(device, CpuAccessFlags.Write);
            directionalLight.Transform.Rotation = new(45, 165, 0);
            directionalLight.Transform.Recalculate();

            depthStencil = new(device, Format.D32Float, 1024, 1024);
            textureTonemap = new(device, Format.R16G16B16A16Float, 1024, 1024, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            texturePreview = new(device, Format.R16G16B16A16Float, 1024, 1024, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            tonemap = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/colorgrading/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen
            });

            fxaa = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
            });

            camera.Far = 1000;
            camera.Fov = 90;
            camera.Width = 1;
            camera.Height = 1;

            lensEffects = Assembly.GetExecutingAssembly().GetTypes().AsParallel().Where(x => x.BaseType == typeof(LensEffectBase)).Select(x => (x.Name.Replace("Lens", string.Empty), x)).ToArray();
        }

        protected override void DisposeCore()
        {
            colorGradingParams.Dispose();
            lensParams.Dispose();
            view.Dispose();
            weather.Dispose();

            depthStencil.Dispose();
            textureTonemap.Dispose();
            texturePreview.Dispose();

            pipeline?.Dispose();
            sampler.Dispose();
            tonemap.Dispose();
            fxaa.Dispose();
        }

        private void CreateNew()
        {
            effects = [];
        }

        private void Save()
        {
        }

        private void Unload()
        {
            effects.Clear();
        }

        private void SaveAs(string path)
        {
        }

        public override void DrawContent(IGraphicsContext context)
        {
            DrawPreviewWindow(context);
            DrawEffectsWindow();

            DrawMenuBar();
            DrawEditor();
        }

        private void DrawEditor()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (ImGui.CollapsingHeader($"{effect.Name}##{i}"))
                {
                    ImGui.BeginChild($"{effect.Name}##{i}Child", new Vector2(0, 300));
                    ImGui.BeginDisabled(!effect.Enabled);
                    if (effect.Draw())
                    {
                        if (autoGenerate)
                        {
                            Generate();
                        }
                    }
                    ImGui.EndDisabled();
                    ImGui.EndChild();

                    DrawEffectContextMenu(effect, ref i);
                }
            }
        }

        private void DrawEffectContextMenu(ILensEffect effect, ref int index)
        {
            if (ImGui.BeginPopupContextWindow($"{index}"))
            {
                var enabled = effect.Enabled;
                ImGui.Checkbox("Enabled", ref enabled);
                effect.Enabled = enabled;
                if (ImGui.MenuItem("Remove"))
                {
                    effects.Remove(effect);
                    index--;
                }
                if (ImGui.MenuItem("Move Up", false, index > 0))
                {
                    effects.Remove(effect);
                    effects.Insert(index - 1, effect);
                }
                if (ImGui.MenuItem("Move Down", false, index < effects.Count - 1))
                {
                    effects.Remove(effect);
                    effects.Insert(index + 1, effect);
                }

                ImGui.EndPopup();
            }
        }

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        CreateNew();
                    }

                    if (ImGui.MenuItem("Load"))
                    {
                        //openFileDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save"))
                    {
                        Save();
                    }

                    if (ImGui.MenuItem("Save as"))
                    {
                        Save();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Close"))
                    {
                        Unload();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Generate"))
                {
                    Generate();
                }

                ImGui.Checkbox("Auto", ref autoGenerate);

                ImGui.EndMenuBar();
            }
        }

        private void DrawEffectsWindow()
        {
            if (!ImGui.Begin("Add Effects"))
            {
                ImGui.End();
                return;
            }

            if (ImGui.CollapsingHeader("Effects"))
            {
                for (int i = 0; i < lensEffects.Length; i++)
                {
                    var eff = lensEffects[i];

                    if (ImGui.MenuItem(eff.Item1))
                    {
                        ILensEffect node = (ILensEffect)Activator.CreateInstance(eff.Item2);
                        effects.Add(node);
                    }
                }
            }

            ImGui.End();
        }

        private void DrawPreviewWindow(IGraphicsContext context)
        {
            if (!ImGui.Begin("Lens Preview"))
            {
                ImGui.End();
                return;
            }

            if (pipeline != null && pipeline.IsValid && pipeline.IsInitialized && !compiling)
            {
                Vector3 sunPosition = Vector3.Transform(directionalLight.Transform.Backward * camera.ClipRange + camera.GlobalPosition, camera.View);

                // skip render if the light is behind the camera.
                if (sunPosition.Z >= 0.0f)
                {
                    Vector4 posH = Vector4.Transform(new Vector4(sunPosition, 1.0f), camera.Projection);
                    posH = new Vector4(new Vector3(posH.X, posH.Y, posH.Z) / posH.W, 1.0f);
                    Vector2 screenPos = new Vector2(posH.X, posH.Y) / posH.Z;
                    screenPos = screenPos * new Vector2(0.5f, -0.5f) + new Vector2(0.5f);
                    var pos = new Vector4(screenPos, posH.Z, posH.W);

                    lensParams.Update(context, new(new(pos.X, pos.Y, 1 - 0.001f, 1), new Vector4(1.4f, 1.2f, 1.0f, 1)));
                    view.Update(context, new(camera, new(1024, 1024)));
                    weather.Update(context, new() { LightDir = new Vector4(directionalLight.Transform.Backward, 1) });

                    context.ClearRenderTargetView(texturePreview.RTV, new(0, 0, 0, 1));
                    context.ClearDepthStencilView(depthStencil.DSV, DepthStencilClearFlags.All, 1, 0);
                    context.SetRenderTarget(texturePreview.RTV, null);
                    context.PSSetShaderResource(0, depthStencil);
                    context.PSSetSampler(0, sampler);

                    context.PSSetConstantBuffer(0, lensParams);
                    context.PSSetConstantBuffer(1, view);
                    context.PSSetConstantBuffer(2, weather);
                    context.SetViewport(new(1024, 1024));
                    context.SetGraphicsPipeline(pipeline);

                    context.DrawInstanced(4, 1, 0, 0);

                    context.SetGraphicsPipeline(null);
                    context.VSSetConstantBuffer(0, null);
                    context.VSSetConstantBuffer(1, null);
                    context.PSSetConstantBuffer(0, null);
                    context.PSSetConstantBuffer(1, null);
                    context.PSSetConstantBuffer(2, null);

                    context.PSSetSampler(0, null);
                    context.PSSetShaderResource(0, null);
                    context.PSSetShaderResource(1, null);
                    context.SetRenderTarget(null, null);

                    context.ClearRenderTargetView(textureTonemap.RTV, new(0, 0, 0, 1));
                    context.SetRenderTarget(textureTonemap.RTV, null);
                    context.SetGraphicsPipeline(tonemap);
                    context.PSSetConstantBuffer(0, colorGradingParams);
                    context.PSSetShaderResource(0, texturePreview.SRV);
                    context.PSSetSampler(0, sampler);
                    context.DrawInstanced(4, 1, 0, 0);
                    context.PSSetSampler(0, null);
                    context.PSSetShaderResource(0, null);
                    context.PSSetConstantBuffer(0, null);
                    context.SetGraphicsPipeline(null);
                    context.SetRenderTarget(null, null);

                    context.SetRenderTarget(texturePreview.RTV, null);
                    context.SetGraphicsPipeline(fxaa);
                    context.PSSetShaderResource(0, textureTonemap.SRV);
                    context.PSSetSampler(0, sampler);
                    context.DrawInstanced(4, 1, 0, 0);
                    context.PSSetSampler(0, null);
                    context.PSSetShaderResource(0, null);
                    context.SetGraphicsPipeline(null);
                    context.SetRenderTarget(null, null);
                }
            }

            var size = ImGui.GetContentRegionAvail();
            size.Y = size.X;
            ImGui.Image(texturePreview.SRV?.NativePointer ?? 0, size);

            if (ImGui.IsItemHovered() || first)
            {
                Vector2 delta = Vector2.Zero;
                if (Mouse.IsDown(MouseButton.Middle))
                {
                    delta = Mouse.Delta;
                }

                float wheel = 0;
                if (Keyboard.IsDown(Key.LCtrl))
                {
                    wheel = Mouse.DeltaWheel.Y * Time.Delta;
                }

                // Only update the camera's position if the mouse got moved in either direction
                if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
                {
                    sc.X += sc.X / 2 * -wheel;

                    // Rotate the camera left and right
                    sc.Y += -delta.X * Time.Delta * speed;

                    // Rotate the camera up and down
                    // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                    sc.Z = Math.Clamp(sc.Z + delta.Y * Time.Delta * speed, -MathF.PI / 2, MathF.PI / 2);

                    first = false;

                    // Calculate the cartesian coordinates
                    Vector3 pos = SphereHelper.GetCartesianCoordinates(sc);
                    var orientation = Quaternion.CreateFromYawPitchRoll(-sc.Y, sc.Z, 0);
                    camera.PositionRotation = (pos, orientation);
                    camera.Recalculate();
                }
            }

            ImGui.End();
        }

        private void Generate()
        {
            semaphore.Wait();
            compiling = true;
            Directory.CreateDirectory("generated/" + "shaders/");

            string result = generator.Generate(effects);

            File.WriteAllText("assets/generated/shaders/generated/usercodeLens.hlsl", result);
            pipeline ??= device.CreateGraphicsPipeline(new()
            {
                PixelShader = "generated/usercodeLens.hlsl",
                VertexShader = "quad.hlsl",
                State = new GraphicsPipelineState()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = default,
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    SampleMask = 0,
                    StencilRef = 0,
                    Topology = PrimitiveTopology.TriangleStrip
                }
            });

            pipeline.Recompile();
            first = true;

            compiling = false;
            semaphore.Release();
        }
    }
}