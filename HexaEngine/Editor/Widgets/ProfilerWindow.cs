namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Rendering.Renderers;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Systems;
    using ImPlotNET;
    using System.Diagnostics;
    using System.Numerics;

    public class ProfilerWindow : EditorWindow
    {
        private const int SampleBufferSize = 1000;
        private bool full = false;
        private bool memory = false;
        private bool cpu = false;
        private bool gpu = false;
        private bool scene = false;
        private bool physics = false;
        private bool flame = false;

        public ProfilerWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name => "SceneProfiler";

        public RingBuffer<double> Frame = new(SampleBufferSize);

        public RingBuffer<double> Systems = new(SampleBufferSize);
        private readonly Dictionary<object, RingBuffer<double>> systems = new();

        public RingBuffer<double> Update = new(SampleBufferSize);
        public RingBuffer<double> Prepass = new(SampleBufferSize);
        public RingBuffer<double> ObjectCulling = new(SampleBufferSize);
        public RingBuffer<double> LightCulling = new(SampleBufferSize);
        public RingBuffer<double> ShadowMaps = new(SampleBufferSize);
        public RingBuffer<double> Geometry = new(SampleBufferSize);
        public RingBuffer<double> AO = new(SampleBufferSize);
        public RingBuffer<double> LightsDeferred = new(SampleBufferSize);
        public RingBuffer<double> LightsForward = new(SampleBufferSize);
        public RingBuffer<double> PostProcessing = new(SampleBufferSize);
        public RingBuffer<double> DebugDraw = new(SampleBufferSize);
        public RingBuffer<double> ImGuiDraw = new(SampleBufferSize);

        public RingBuffer<double> GpuTotal = new(SampleBufferSize);
        public RingBuffer<double> GpuUpdate = new(SampleBufferSize);
        public RingBuffer<double> GpuPrepass = new(SampleBufferSize);
        public RingBuffer<double> GpuObjectCulling = new(SampleBufferSize);
        public RingBuffer<double> GpuLightCulling = new(SampleBufferSize);
        public RingBuffer<double> GpuShadowMaps = new(SampleBufferSize);
        public RingBuffer<double> GpuGeometry = new(SampleBufferSize);
        public RingBuffer<double> GpuAO = new(SampleBufferSize);
        public RingBuffer<double> GpuLightsDeferred = new(SampleBufferSize);
        public RingBuffer<double> GpuLightsForward = new(SampleBufferSize);
        public RingBuffer<double> GpuPostProcessing = new(SampleBufferSize);
        public RingBuffer<double> GpuDebugDraw = new(SampleBufferSize);
        public RingBuffer<double> GpuImGuiDraw = new(SampleBufferSize);

        public RingBuffer<double> Simulation = new(SampleBufferSize);
        public RingBuffer<double> PoseIntegrator = new(SampleBufferSize);
        public RingBuffer<double> Sleeper = new(SampleBufferSize);
        public RingBuffer<double> BroadPhaseUpdate = new(SampleBufferSize);
        public RingBuffer<double> CollisionTesting = new(SampleBufferSize);
        public RingBuffer<double> NarrowPhaseFlush = new(SampleBufferSize);
        public RingBuffer<double> Solver = new(SampleBufferSize);
        public RingBuffer<double> BatchCompressor = new(SampleBufferSize);

        public RingBuffer<double> MemoryUsage = new(SampleBufferSize);
        public RingBuffer<double> VideoMemoryUsage = new(SampleBufferSize);

        public int SamplesPerSecond { get => samplesPerSecond; set => samplesPerSecond = value; }

        public float SampleLatency => 1f / SamplesPerSecond;

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Options"))
                {
                    ImGui.Checkbox("Profile (low~extreme performance impact)", ref full);
                    ImGui.Checkbox("Memory Profile (extreme performance impact 4.5ms)", ref memory);
                    ImGui.Checkbox("CPU Profile (low~med performance impact)", ref cpu);
                    ImGui.Checkbox("GPU Profile (heavy performance impact)", ref gpu);
                    ImGui.Checkbox("Scene Profile (low~med performance impact)", ref scene);
                    ImGui.Checkbox("Physics Profile (low~med performance impact)", ref physics);
                    ImGui.Checkbox("Flame Graph", ref flame);
                    ImGui.Separator();
                    if (ImGui.RadioButton("Samples Low (100)", samplesPerSecond == 100))
                    {
                        samplesPerSecond = 100;
                    }
                    if (ImGui.RadioButton("Samples Medium (1000)", samplesPerSecond == 1000))
                    {
                        samplesPerSecond = 1000;
                    }
                    if (ImGui.RadioButton("Samples High (4000)", samplesPerSecond == 4000))
                    {
                        samplesPerSecond = 4000;
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            SampleInterpolated(context);
            if (flame)
            {
                var renderer = SceneRenderer.Current;

                if (renderer == null)
                    return;
                var profiler = renderer.Profiler;
                ImGuiWidgetFlameGraph.PlotFlame("Flame", profiler.Getter, profiler.Current, profiler.StageCount, ref selected);
            }
            if (full)
            {
                DrawFull(context);
            }
            else
            {
                DrawSlim();
            }
        }

        public void DrawSlim()
        {
            var fps = 1 / Time.Delta;
            var ms = Time.Delta * 1000;
            ImGui.Text($"FPS: {fps}");
            ImGui.Text($"Latency: {ms}ms");
        }

        public void DrawFull(IGraphicsContext context)
        {
            const int shade_mode = 2;
            const float fill_ref = 0;
            double fill = shade_mode == 0 ? -double.PositiveInfinity : shade_mode == 1 ? double.PositiveInfinity : fill_ref;

            ImPlot.SetNextAxesToFit();
            if (ImPlot.BeginPlot("Latency", new Vector2(-1, 0), ImPlotFlags.NoInputs))
            {
                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                ImPlot.PlotShaded("Latency", ref Frame.Values[0], Frame.Length, fill, 1, 0, ImPlotShadedFlags.None, Frame.Head);
                ImPlot.PopStyleVar();

                ImPlot.PlotLine("Latency", ref Frame.Values[0], Frame.Length, 1, 0, ImPlotLineFlags.None, Frame.Head);
                ImPlot.EndPlot();
            }

            if (memory)
            {
                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Memory", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded("Memory", ref MemoryUsage.Values[0], MemoryUsage.Length, fill, 1, 0, ImPlotShadedFlags.None, MemoryUsage.Head);
                    ImPlot.PlotShaded("Video Memory", ref VideoMemoryUsage.Values[0], VideoMemoryUsage.Length, fill, 1, 0, ImPlotShadedFlags.None, VideoMemoryUsage.Head);
                    ImPlot.PopStyleVar();

                    ImPlot.PlotLine("Memory", ref MemoryUsage.Values[0], MemoryUsage.Length, 1, 0, ImPlotLineFlags.None, MemoryUsage.Head);
                    ImPlot.PlotLine("Video Memory", ref VideoMemoryUsage.Values[0], VideoMemoryUsage.Length, 1, 0, ImPlotLineFlags.None, VideoMemoryUsage.Head);
                    ImPlot.EndPlot();
                }
            }

            if (cpu)
            {
                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Graphics (CPU Latency)", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded("Total", ref Frame.Values[0], Frame.Length, fill, 1, 0, ImPlotShadedFlags.None, Frame.Head);
                    ImPlot.PlotShaded("Update", ref Update.Values[0], Update.Length, fill, 1, 0, ImPlotShadedFlags.None, Update.Head);
                    ImPlot.PlotShaded("Prepass", ref Prepass.Values[0], Prepass.Length, fill, 1, 0, ImPlotShadedFlags.None, Prepass.Head);
                    ImPlot.PlotShaded("Object Culling", ref ObjectCulling.Values[0], ObjectCulling.Length, fill, 1, 0, ImPlotShadedFlags.None, ObjectCulling.Head);
                    ImPlot.PlotShaded("Light Culling", ref LightCulling.Values[0], LightCulling.Length, fill, 1, 0, ImPlotShadedFlags.None, LightCulling.Head);
                    ImPlot.PlotShaded("Shadow Maps", ref ShadowMaps.Values[0], ShadowMaps.Length, fill, 1, 0, ImPlotShadedFlags.None, ShadowMaps.Head);
                    ImPlot.PlotShaded("Geometry", ref Geometry.Values[0], Geometry.Length, fill, 1, 0, ImPlotShadedFlags.None, Geometry.Head);
                    ImPlot.PlotShaded("AO", ref AO.Values[0], AO.Length, fill, 1, 0, ImPlotShadedFlags.None, AO.Head);
                    ImPlot.PlotShaded("Lights Deferred", ref LightsDeferred.Values[0], LightsDeferred.Length, fill, 1, 0, ImPlotShadedFlags.None, LightsDeferred.Head);
                    ImPlot.PlotShaded("Lights Forward", ref LightsForward.Values[0], LightsForward.Length, fill, 1, 0, ImPlotShadedFlags.None, LightsForward.Head);
                    ImPlot.PlotShaded("PostProcess", ref PostProcessing.Values[0], PostProcessing.Length, fill, 1, 0, ImPlotShadedFlags.None, PostProcessing.Head);
                    ImPlot.PlotShaded("DebugDraw", ref DebugDraw.Values[0], DebugDraw.Length, fill, 1, 0, ImPlotShadedFlags.None, DebugDraw.Head);
                    ImPlot.PlotShaded("ImGui", ref ImGuiDraw.Values[0], ImGuiDraw.Length, fill, 1, 0, ImPlotShadedFlags.None, ImGuiDraw.Head);
                    ImPlot.PopStyleVar();

                    ImPlot.PlotLine("Total", ref Frame.Values[0], Frame.Length, 1, 0, ImPlotLineFlags.None, Frame.Head);
                    ImPlot.PlotLine("Update", ref Update.Values[0], Update.Length, 1, 0, ImPlotLineFlags.None, Update.Head);
                    ImPlot.PlotLine("Prepass", ref Prepass.Values[0], Prepass.Length, 1, 0, ImPlotLineFlags.None, Prepass.Head);
                    ImPlot.PlotLine("Object Culling", ref ObjectCulling.Values[0], ObjectCulling.Length, 1, 0, ImPlotLineFlags.None, ObjectCulling.Head);
                    ImPlot.PlotLine("Light Culling", ref LightCulling.Values[0], LightCulling.Length, 1, 0, ImPlotLineFlags.None, LightCulling.Head);
                    ImPlot.PlotLine("Shadow Maps", ref ShadowMaps.Values[0], ShadowMaps.Length, 1, 0, ImPlotLineFlags.None, ShadowMaps.Head);
                    ImPlot.PlotLine("Geometry", ref Geometry.Values[0], Geometry.Length, 1, 0, ImPlotLineFlags.None, Geometry.Head);
                    ImPlot.PlotLine("AO", ref AO.Values[0], AO.Length, 1, 0, ImPlotLineFlags.None, AO.Head);
                    ImPlot.PlotLine("Lights Deferred", ref LightsDeferred.Values[0], LightsDeferred.Length, 1, 0, ImPlotLineFlags.None, LightsDeferred.Head);
                    ImPlot.PlotLine("Lights Forward", ref LightsForward.Values[0], LightsForward.Length, 1, 0, ImPlotLineFlags.None, LightsForward.Head);
                    ImPlot.PlotLine("PostProcess", ref PostProcessing.Values[0], PostProcessing.Length, 1, 0, ImPlotLineFlags.None, PostProcessing.Head);
                    ImPlot.PlotLine("DebugDraw", ref DebugDraw.Values[0], DebugDraw.Length, 1, 0, ImPlotLineFlags.None, DebugDraw.Head);
                    ImPlot.PlotLine("ImGui", ref ImGuiDraw.Values[0], ImGuiDraw.Length, 1, 0, ImPlotLineFlags.None, ImGuiDraw.Head);
                    ImPlot.EndPlot();
                }
            }

            if (gpu)
            {
                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Graphics (GPU Latency)", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded("Total", ref GpuTotal.Values[0], GpuTotal.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuTotal.Head);
                    ImPlot.PlotShaded("Update", ref GpuUpdate.Values[0], GpuUpdate.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuUpdate.Head);
                    ImPlot.PlotShaded("Prepass", ref GpuPrepass.Values[0], GpuPrepass.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuPrepass.Head);
                    ImPlot.PlotShaded("Object Culling", ref GpuObjectCulling.Values[0], GpuObjectCulling.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuObjectCulling.Head);
                    ImPlot.PlotShaded("Light Culling", ref GpuLightCulling.Values[0], GpuLightCulling.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuLightCulling.Head);
                    ImPlot.PlotShaded("Shadow Maps", ref GpuShadowMaps.Values[0], GpuShadowMaps.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuShadowMaps.Head);
                    ImPlot.PlotShaded("Geometry", ref GpuGeometry.Values[0], GpuGeometry.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuGeometry.Head);
                    ImPlot.PlotShaded("AO", ref GpuAO.Values[0], GpuAO.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuAO.Head);
                    ImPlot.PlotShaded("Lights Deferred", ref GpuLightsDeferred.Values[0], GpuLightsDeferred.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuLightsDeferred.Head);
                    ImPlot.PlotShaded("Lights Forward", ref GpuLightsForward.Values[0], GpuLightsForward.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuLightsForward.Head);
                    ImPlot.PlotShaded("PostProcess", ref GpuPostProcessing.Values[0], GpuPostProcessing.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuPostProcessing.Head);
                    ImPlot.PlotShaded("DebugDraw", ref GpuDebugDraw.Values[0], GpuDebugDraw.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuDebugDraw.Head);
                    ImPlot.PlotShaded("ImGui", ref GpuImGuiDraw.Values[0], GpuImGuiDraw.Length, fill, 1, 0, ImPlotShadedFlags.None, GpuImGuiDraw.Head);
                    ImPlot.PopStyleVar();

                    ImPlot.PlotLine("Total", ref GpuTotal.Values[0], GpuTotal.Length, 1, 0, ImPlotLineFlags.None, GpuTotal.Head);
                    ImPlot.PlotLine("Update", ref GpuUpdate.Values[0], GpuUpdate.Length, 1, 0, ImPlotLineFlags.None, GpuUpdate.Head);
                    ImPlot.PlotLine("Prepass", ref GpuPrepass.Values[0], GpuPrepass.Length, 1, 0, ImPlotLineFlags.None, GpuPrepass.Head);
                    ImPlot.PlotLine("Object Culling", ref GpuObjectCulling.Values[0], GpuObjectCulling.Length, 1, 0, ImPlotLineFlags.None, GpuObjectCulling.Head);
                    ImPlot.PlotLine("Light Culling", ref GpuLightCulling.Values[0], GpuLightCulling.Length, 1, 0, ImPlotLineFlags.None, GpuLightCulling.Head);
                    ImPlot.PlotLine("Shadow Maps", ref GpuShadowMaps.Values[0], GpuShadowMaps.Length, 1, 0, ImPlotLineFlags.None, GpuShadowMaps.Head);
                    ImPlot.PlotLine("Geometry", ref GpuGeometry.Values[0], GpuGeometry.Length, 1, 0, ImPlotLineFlags.None, GpuGeometry.Head);
                    ImPlot.PlotLine("AO", ref GpuAO.Values[0], GpuAO.Length, 1, 0, ImPlotLineFlags.None, GpuAO.Head);
                    ImPlot.PlotLine("Lights Deferred", ref GpuLightsDeferred.Values[0], GpuLightsDeferred.Length, 1, 0, ImPlotLineFlags.None, GpuLightsDeferred.Head);
                    ImPlot.PlotLine("Lights Forward", ref GpuLightsForward.Values[0], GpuLightsForward.Length, 1, 0, ImPlotLineFlags.None, GpuLightsForward.Head);
                    ImPlot.PlotLine("PostProcess", ref GpuPostProcessing.Values[0], GpuPostProcessing.Length, 1, 0, ImPlotLineFlags.None, GpuPostProcessing.Head);
                    ImPlot.PlotLine("DebugDraw", ref GpuDebugDraw.Values[0], GpuDebugDraw.Length, 1, 0, ImPlotLineFlags.None, GpuDebugDraw.Head);
                    ImPlot.PlotLine("ImGui", ref GpuImGuiDraw.Values[0], GpuImGuiDraw.Length, 1, 0, ImPlotLineFlags.None, GpuImGuiDraw.Head);
                    ImPlot.EndPlot();
                }
            }

            if (scene)
            {
                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Systems", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded("Total", ref Systems.Values[0], Systems.Length, fill, 1, 0, ImPlotShadedFlags.None, Systems.Head);
                    ImPlot.PlotShaded("Physics", ref Simulation.Values[0], Simulation.Length, fill, 1, 0, ImPlotShadedFlags.None, Simulation.Head);
                    ImPlot.PopStyleVar();

                    ImPlot.PlotLine("Total", ref Systems.Values[0], Systems.Length, 1, 0, ImPlotLineFlags.None, Systems.Head);
                    ImPlot.PlotLine("Physics", ref Simulation.Values[0], Simulation.Length, 1, 0, ImPlotLineFlags.None, Simulation.Head);
                    ImPlot.EndPlot();
                }

                Scene? scene = SceneManager.Current;
                if (scene == null)
                {
                    return;
                }

                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Systems", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded("Total", ref Systems.Values[0], Systems.Length, fill, 1, 0, ImPlotShadedFlags.None, Systems.Head);
                    ImPlot.PopStyleVar();
                    ImPlot.PlotLine("Total", ref Systems.Values[0], Systems.Length, 1, 0, ImPlotLineFlags.None, Systems.Head);
                    for (int i = 0; i < scene.Systems.Count; i++)
                    {
                        var system = scene.Systems[i];
                        if (!systems.TryGetValue(system, out var buffer))
                        {
                            continue;
                        }

                        ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                        ImPlot.PlotShaded(system.Name, ref buffer.Values[0], buffer.Length, fill, 1, 0, ImPlotShadedFlags.None, buffer.Head);
                        ImPlot.PopStyleVar();

                        ImPlot.PlotLine(system.Name, ref buffer.Values[0], buffer.Length, 1, 0, ImPlotLineFlags.None, buffer.Head);
                    }

                    ImPlot.EndPlot();
                }
            }

            if (physics)
            {
                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Physics", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded("Total", ref Simulation.Values[0], Simulation.Length, fill, 1, 0, ImPlotShadedFlags.None, Simulation.Head);
                    ImPlot.PlotShaded("PoseIntegrator", ref PoseIntegrator.Values[0], PoseIntegrator.Length, fill, 1, 0, ImPlotShadedFlags.None, PoseIntegrator.Head);
                    ImPlot.PlotShaded("Sleeper", ref Sleeper.Values[0], Sleeper.Length, fill, 1, 0, ImPlotShadedFlags.None, Sleeper.Head);
                    ImPlot.PlotShaded("BroadPhaseUpdate", ref BroadPhaseUpdate.Values[0], BroadPhaseUpdate.Length, fill, 1, 0, ImPlotShadedFlags.None, BroadPhaseUpdate.Head);
                    ImPlot.PlotShaded("CollisionTesting", ref CollisionTesting.Values[0], CollisionTesting.Length, fill, 1, 0, ImPlotShadedFlags.None, CollisionTesting.Head);
                    ImPlot.PlotShaded("NarrowPhaseFlush", ref NarrowPhaseFlush.Values[0], NarrowPhaseFlush.Length, fill, 1, 0, ImPlotShadedFlags.None, NarrowPhaseFlush.Head);
                    ImPlot.PlotShaded("Solver", ref Solver.Values[0], Solver.Length, fill, 1, 0, ImPlotShadedFlags.None, Solver.Head);
                    ImPlot.PlotShaded("BatchCompressor", ref BatchCompressor.Values[0], BatchCompressor.Length, fill, 1, 0, ImPlotShadedFlags.None, BatchCompressor.Head);
                    ImPlot.PopStyleVar();

                    ImPlot.PlotLine("Total", ref Simulation.Values[0], Simulation.Length, 1, 0, ImPlotLineFlags.None, Simulation.Head);
                    ImPlot.PlotLine("PoseIntegrator", ref PoseIntegrator.Values[0], PoseIntegrator.Length, 1, 0, ImPlotLineFlags.None, PoseIntegrator.Head);
                    ImPlot.PlotLine("Sleeper", ref Sleeper.Values[0], Sleeper.Length, 1, 0, ImPlotLineFlags.None, Sleeper.Head);
                    ImPlot.PlotLine("BroadPhaseUpdate", ref BroadPhaseUpdate.Values[0], BroadPhaseUpdate.Length, 1, 0, ImPlotLineFlags.None, BroadPhaseUpdate.Head);
                    ImPlot.PlotLine("CollisionTesting", ref CollisionTesting.Values[0], CollisionTesting.Length, 1, 0, ImPlotLineFlags.None, CollisionTesting.Head);
                    ImPlot.PlotLine("NarrowPhaseFlush", ref NarrowPhaseFlush.Values[0], NarrowPhaseFlush.Length, 1, 0, ImPlotLineFlags.None, NarrowPhaseFlush.Head);
                    ImPlot.PlotLine("Solver", ref Solver.Values[0], Solver.Length, 1, 0, ImPlotLineFlags.None, Solver.Head);
                    ImPlot.PlotLine("BatchCompressor", ref BatchCompressor.Values[0], BatchCompressor.Length, 1, 0, ImPlotLineFlags.None, BatchCompressor.Head);

                    ImPlot.EndPlot();
                }
            }
        }

        private float accum = 0;
        private int samplesPerSecond = 1000;
        private int selected;

        public void SampleInterpolated(IGraphicsContext context)
        {
            accum += Time.Delta;

            while (accum > SampleLatency)
            {
                Sample(context);
                accum -= SampleLatency;
            }
        }

        public void Sample(IGraphicsContext context)
        {
            Frame.Add(Time.Delta * 1000);

            if (memory)
            {
                MemoryUsage.Add(Process.GetCurrentProcess().PrivateMemorySize64 / 1000d / 1000d);
                VideoMemoryUsage.Add(GraphicsAdapter.Current.GetMemoryCurrentUsage() / 1000d / 1000d);
            }

            var renderer = SceneRenderer.Current;

            if (renderer == null)
                return;

            if (cpu)
            {
                Update.Add(renderer.Profiler["Update"] * 1000);
                ObjectCulling.Add(renderer.Profiler["ObjectCulling"] * 1000);
                LightCulling.Add(renderer.Profiler["LightCulling"] * 1000);
                ShadowMaps.Add(renderer.Profiler["ShadowMaps"] * 1000);
                Prepass.Add(renderer.Profiler["PrePass"] * 1000);
                Geometry.Add(renderer.Profiler["Geometry"] * 1000);
                AO.Add(renderer.Profiler["AO"] * 1000);
                LightsDeferred.Add(renderer.Profiler["LightsDeferred"] * 1000);
                LightsForward.Add(renderer.Profiler["LightsForward"] * 1000);
                PostProcessing.Add(renderer.Profiler["PostProcessing"] * 1000);
                DebugDraw.Add(renderer.Profiler["DebugDraw"] * 1000);
                ImGuiDraw.Add(renderer.Profiler["ImGui"] * 1000);
            }

            if (gpu)
            {
                GpuTotal.Add(context.Device.Profiler["Total"] * 1000);
                GpuUpdate.Add(context.Device.Profiler["Update"] * 1000);
                GpuObjectCulling.Add(context.Device.Profiler["ObjectCulling"] * 1000);
                GpuLightCulling.Add(context.Device.Profiler["LightCulling"] * 1000);
                GpuShadowMaps.Add(context.Device.Profiler["ShadowMaps"] * 1000);
                GpuPrepass.Add(context.Device.Profiler["PrePass"] * 1000);
                GpuGeometry.Add(context.Device.Profiler["Geometry"] * 1000);
                GpuAO.Add(context.Device.Profiler["AO"] * 1000);
                GpuLightsDeferred.Add(context.Device.Profiler["LightsDeferred"] * 1000);
                GpuLightsForward.Add(context.Device.Profiler["LightsForward"] * 1000);
                GpuPostProcessing.Add(context.Device.Profiler["PostProcessing"] * 1000);
                GpuDebugDraw.Add(context.Device.Profiler["DebugDraw"] * 1000);
                GpuImGuiDraw.Add(context.Device.Profiler["ImGui"] * 1000);
            }

            Scene? scene = SceneManager.Current;

            if (scene == null)
            {
                return;
            }

            if (this.scene)
            {
                Systems.Add(scene.Profiler[scene.Systems] * 1000);

                for (int i = 0; i < scene.Systems.Count; i++)
                {
                    var system = scene.Systems[i];
                    var value = scene.Profiler[system];
                    if (!systems.TryGetValue(system, out var buffer))
                    {
                        buffer = new(SampleBufferSize);
                        systems.Add(system, buffer);
                    }
                    buffer.Add(value * 1000);
                    systems[system] = buffer;
                }
            }

            var simulation = scene.GetRequiredSystem<PhysicsSystem>().Simulation;

            if (physics)
            {
                Simulation.Add(simulation.Profiler[simulation] * 1000);
                PoseIntegrator.Add(simulation.Profiler[simulation.PoseIntegrator] * 1000);
                Sleeper.Add(simulation.Profiler[simulation.Sleeper] * 1000);
                BroadPhaseUpdate.Add(simulation.Profiler[simulation.BroadPhase] * 1000);
                CollisionTesting.Add(simulation.Profiler[simulation.BroadPhaseOverlapFinder] * 1000);
                NarrowPhaseFlush.Add(simulation.Profiler[simulation.NarrowPhase] * 1000);
                Solver.Add(simulation.Profiler[simulation.Solver]);
                BatchCompressor.Add(simulation.Profiler[simulation.SolverBatchCompressor]);
            }
        }
    }
}