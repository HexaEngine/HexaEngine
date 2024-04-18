﻿namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImPlot;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Scenes;
    using System.Diagnostics;
    using System.Numerics;

    public class MemorySnapshot
    {
        public long Timestamp;
        public long TotalMemory;
        public string TotalMemText;
        public List<MemoryManager.MemoryEntry> GPUMemory = [];

        public MemorySnapshot(long timestamp, long totalMemory)
        {
            Timestamp = timestamp;
            TotalMemory = totalMemory;
            TotalMemText = totalMemory.FormatDataSize();
        }

        public void Collect()
        {
            GPUMemory.AddRange(MemoryManager.Entries);
        }
    }

    public enum ProfilerTab
    {
        Scene,
        Memory,
        Graphics,
    }

    [EditorWindowCategory("Debug")]
    public class ProfilerWindow : EditorWindow
    {
        private const int SampleBufferSize = 1000;
        private bool full = false;
        private bool memory = false;
        private bool graphics = false;
        private bool gpu = false;
        private bool scene = false;
        private bool physics = false;
        private bool flame = false;

        private ProfilerTab tab;

        public ProfilerWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name => "Scene Profiler";

        public RingBuffer<double> Frame = new(SampleBufferSize);

        public RingBuffer<double> TotalUpdate = new(SampleBufferSize);
        public RingBuffer<double> SceneTick = new(SampleBufferSize);
        public RingBuffer<double> FixedUpdate = new(SampleBufferSize);
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

        public RingBuffer<double> MemoryUsage = new(SampleBufferSize) { AverageValues = false };
        public RingBuffer<double> VideoMemoryUsage = new(SampleBufferSize) { AverageValues = false };

        public int SamplesPerSecond { get => samplesPerSecond; set => samplesPerSecond = value; }

        public int SamplesPerSecondMem { get => samplesPerSecondMem; set => samplesPerSecondMem = value; }

        public float SampleLatency => 1f / SamplesPerSecond;

        public float MemSampleLatency => 1f / SamplesPerSecondMem;

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Options"))
                {
                    ImGui.Checkbox("Profile (low~extreme performance impact)", ref full);
                    ImGui.Checkbox("Memory Profile (extreme performance impact 4.5ms)", ref memory);
                    ImGui.Checkbox("CPU Profile (low~med performance impact)", ref graphics);
                    if (ImGui.Checkbox("GPU Profile (heavy performance impact)", ref gpu))
                    {
                        context.Device.Profiler.Enabled = gpu;
                    }
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
                {
                    return;
                }

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

            ImGui.BeginTabBar("Profiler", ImGuiTabBarFlags.None);

            if (ImGui.BeginTabItem("Frame"))
            {
                Vector2 avail = ImGui.GetContentRegionAvail();

                if (gpu)
                {
                    avail.X *= 0.5f;
                }

                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Frame (CPU Latency)", new Vector2(avail.X, 0), ImPlotFlags.NoInputs))
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

                if (gpu)
                {
                    ImGui.SameLine();
                    ImPlot.SetNextAxesToFit();
                    if (ImPlot.BeginPlot("Frame (GPU Latency)", new Vector2(avail.X, 0), ImPlotFlags.NoInputs))
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

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Memory"))
            {
                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Memory", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded("Memory (RAM)", ref MemoryUsage.Values[0], MemoryUsage.Length, fill, 1, 0, ImPlotShadedFlags.None, MemoryUsage.Head);
                    ImPlot.PlotShaded("Video Memory (VRAM)", ref VideoMemoryUsage.Values[0], VideoMemoryUsage.Length, fill, 1, 0, ImPlotShadedFlags.None, VideoMemoryUsage.Head);
                    ImPlot.PopStyleVar();

                    ImPlot.PlotLine("Memory (RAM)", ref MemoryUsage.Values[0], MemoryUsage.Length, 1, 0, ImPlotLineFlags.None, MemoryUsage.Head);
                    ImPlot.PlotLine("Video Memory (VRAM)", ref VideoMemoryUsage.Values[0], VideoMemoryUsage.Length, 1, 0, ImPlotLineFlags.None, VideoMemoryUsage.Head);
                    ImPlot.EndPlot();
                }

                if (ImGui.BeginListBox("Snapshots"))
                {
                    if (memorySnapshots.Count > 0)
                    {
                        var baseline = memorySnapshots[0];
                        var last = baseline;
                        ImGui.Text($"Snapshot #0 {last.TotalMemText}");
                        for (int i = 1; i < memorySnapshots.Count; i++)
                        {
                            var memorySnapshot = memorySnapshots[i];
                            var toBaseline = memorySnapshot.TotalMemory - baseline.TotalMemory;
                            var deltaLast = memorySnapshot.TotalMemory - last.TotalMemory;
                            ImGui.Text($"Snapshot #{i} {memorySnapshot.TotalMemText}, {deltaLast.FormatDataSize()}, {toBaseline.FormatDataSize()}");
                            last = memorySnapshot;
                        }
                    }

                    ImGui.EndListBox();
                }

                if (ImGui.Button("Snapshot"))
                {
                    GC.Collect();
                    GC.WaitForFullGCComplete();
                    Thread.MemoryBarrier();
                    MemorySnapshot snapshot = new(Stopwatch.GetTimestamp(), Process.GetCurrentProcess().PrivateMemorySize64);
                    snapshot.Collect();
                    memorySnapshots.Add(snapshot);
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Scene"))
            {
                Scene? scene = SceneManager.Current;
                if (scene != null)
                {
                    ImPlot.SetNextAxesToFit();
                    if (ImPlot.BeginPlot("Scene Update", new Vector2(-1, 0)))
                    {
                        ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                        ImPlot.PlotShaded("Total", ref TotalUpdate.Values[0], TotalUpdate.Length, fill, 1, 0, ImPlotShadedFlags.None, TotalUpdate.Head);
                        ImPlot.PlotShaded("Scene Tick", ref SceneTick.Values[0], SceneTick.Length, fill, 1, 0, ImPlotShadedFlags.None, SceneTick.Head);
                        ImPlot.PlotShaded("FixedUpdate", ref FixedUpdate.Values[0], FixedUpdate.Length, fill, 1, 0, ImPlotShadedFlags.None, FixedUpdate.Head);
                        ImPlot.PopStyleVar();

                        ImPlot.PlotLine("Total", ref TotalUpdate.Values[0], TotalUpdate.Length, 1, 0, ImPlotLineFlags.None, TotalUpdate.Head);
                        ImPlot.PlotLine("Scene Tick", ref SceneTick.Values[0], SceneTick.Length, 1, 0, ImPlotLineFlags.None, SceneTick.Head);
                        ImPlot.PlotLine("FixedUpdate", ref FixedUpdate.Values[0], FixedUpdate.Length, 1, 0, ImPlotLineFlags.None, FixedUpdate.Head);

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

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Physics"))
            {
                ImPlot.SetNextAxesToFit();
                if (ImPlot.BeginPlot("Physics", new Vector2(-1, 0), ImPlotFlags.NoInputs))
                {
                    // TODO: PhysX Profiler.
                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);

                    ImPlot.PopStyleVar();

                    ImPlot.EndPlot();
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        private float accum = 0;
        private int samplesPerSecond = 1000;
        private int samplesPerSecondMem = 50;
        private int selected;

        private float accumMem = 0;
        private readonly List<MemorySnapshot> memorySnapshots = new();

        public void SampleInterpolated(IGraphicsContext context)
        {
            accum += Time.Delta;

            while (accum > SampleLatency)
            {
                Sample(context);
                accum -= SampleLatency;
            }

            if (memory)
            {
                accumMem += Time.Delta;
                while (accumMem > MemSampleLatency)
                {
                    SampleMem(context);
                    accumMem = 0;
                }
            }
        }

        public void Sample(IGraphicsContext context)
        {
            Frame.Add(Time.Delta * 1000);

            var renderer = SceneRenderer.Current;

            if (renderer == null)
            {
                return;
            }

            if (graphics)
            {
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
                var sceneTick = scene.Profiler[Scene.ProfileObject] * 1000;
                var fixedUpdate = scene.Profiler[Time.ProfileObject] * 1000;
                SceneTick.Add(sceneTick);
                FixedUpdate.Add(fixedUpdate);
                TotalUpdate.Add(sceneTick + fixedUpdate);

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
        }

        public void SampleMem(IGraphicsContext context)
        {
            MemoryUsage.Add(Process.GetCurrentProcess().PrivateMemorySize64 / 1000d / 1000d);
            VideoMemoryUsage.Add(GraphicsAdapter.Current.GetMemoryCurrentUsage() / 1000d / 1000d);
        }
    }
}