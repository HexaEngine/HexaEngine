﻿namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Physics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.ImGuiNET;
    using ImPlotNET;
    using System.Diagnostics;
    using System.Numerics;

    public unsafe struct RingBuffer
    {
        private readonly double[] values;
        private readonly int length;
        private int tail = 0;
        private int head;

        public RingBuffer(int length)
        {
            values = new double[length];
            this.length = length;
        }

        public double[] Values => values;

        public int Length => length;

        public int Tail => tail;

        public int Head => head;

        public void Add(double value)
        {
            if (value < 0)
            {
                value = 0;
            }

            values[tail] = value;

            tail++;

            if (tail == length)
            {
                tail = 0;
            }
            if (tail < 0)
            {
                tail = length - 1;
            }

            head = (tail - length) % length;

            if (head < 0)
            {
                head += length;
            }
        }
    }

    public class ProfilerWindow : EditorWindow
    {
        protected override string Name => "SceneProfiler";

        public RingBuffer Frame = new(512);

        public RingBuffer Updates = new(512);

        public RingBuffer Systems = new(512);
        private readonly Dictionary<object, RingBuffer> systems = new();

        public RingBuffer Graphics = new(512);
        public RingBuffer Update = new(512);
        public RingBuffer Prepass = new(512);
        public RingBuffer ObjectCulling = new(512);
        public RingBuffer LightCulling = new(512);
        public RingBuffer ShadowMaps = new(512);
        public RingBuffer Geometry = new(512);
        public RingBuffer AO = new(512);
        public RingBuffer LightsDeferred = new(512);
        public RingBuffer LightsForward = new(512);
        public RingBuffer PostProcessing = new(512);
        public RingBuffer DebugDraw = new(512);
        public RingBuffer ImGuiDraw = new(512);

        public RingBuffer GpuTotal = new(512);
        public RingBuffer GpuUpdate = new(512);
        public RingBuffer GpuPrepass = new(512);
        public RingBuffer GpuObjectCulling = new(512);
        public RingBuffer GpuLightCulling = new(512);
        public RingBuffer GpuShadowMaps = new(512);
        public RingBuffer GpuGeometry = new(512);
        public RingBuffer GpuAO = new(512);
        public RingBuffer GpuLightsDeferred = new(512);
        public RingBuffer GpuLightsForward = new(512);
        public RingBuffer GpuPostProcessing = new(512);
        public RingBuffer GpuDebugDraw = new(512);
        public RingBuffer GpuImGuiDraw = new(512);

        public RingBuffer Simulation = new(512);
        public RingBuffer PoseIntegrator = new(512);
        public RingBuffer Sleeper = new(512);
        public RingBuffer BroadPhaseUpdate = new(512);
        public RingBuffer CollisionTesting = new(512);
        public RingBuffer NarrowPhaseFlush = new(512);
        public RingBuffer Solver = new(512);
        public RingBuffer BatchCompressor = new(512);

        public RingBuffer MemoryUsage = new(512);
        public RingBuffer VideoMemoryUsage = new(512);

        public override void DrawContent(IGraphicsContext context)
        {
            const int shade_mode = 2;
            const float fill_ref = 0;
            double fill = shade_mode == 0 ? -double.PositiveInfinity : shade_mode == 1 ? double.PositiveInfinity : fill_ref;

            Scene? scene = SceneManager.Current;

            Frame.Add(Time.Delta * 1000);

            MemoryUsage.Add(Process.GetCurrentProcess().PrivateMemorySize64 / 1000d / 1000d);
            VideoMemoryUsage.Add(GraphicsAdapter.Current.GetMemoryCurrentUsage() / 1000d / 1000d);

            bool enableGPUProfiler = context.Device.Profiler.Enabled;
            if (ImGui.Checkbox("Enable GPU Profile", ref enableGPUProfiler))
            {
                context.Device.Profiler.Enabled = enableGPUProfiler;
            }

            ImPlot.SetNextAxesToFit();
            if (ImPlot.BeginPlot("Latency", new Vector2(-1, 0), ImPlotFlags.NoInputs))
            {
                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                ImPlot.PlotShaded("Latency", ref Frame.Values[0], Frame.Length, fill, 1, 0, ImPlotShadedFlags.None, Frame.Head);
                ImPlot.PopStyleVar();

                ImPlot.PlotLine("Latency", ref Frame.Values[0], Frame.Length, 1, 0, ImPlotLineFlags.None, Frame.Head);
                ImPlot.EndPlot();
            }

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

            var renderer = Application.MainWindow.Renderer;

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

            ImPlot.SetNextAxesToFit();
            if (ImPlot.BeginPlot("Graphics (CPU Latency)", new Vector2(-1, 0), ImPlotFlags.NoInputs))
            {
                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                ImPlot.PlotShaded("Total", ref Graphics.Values[0], Graphics.Length, fill, 1, 0, ImPlotShadedFlags.None, Graphics.Head);
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

                ImPlot.PlotLine("Total", ref Graphics.Values[0], Graphics.Length, 1, 0, ImPlotLineFlags.None, Graphics.Head);
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

            if (scene == null)
            {
                return;
            }

            Graphics.Add(renderer.Profiler["Total"] * 1000);
            Systems.Add(scene.Profiler[scene.Systems] * 1000);

            var simulation = scene.GetRequiredSystem<PhysicsSystem>().Simulation;

            Simulation.Add(simulation.Profiler[simulation] * 1000);
            PoseIntegrator.Add(simulation.Profiler[simulation.PoseIntegrator] * 1000);
            Sleeper.Add(simulation.Profiler[simulation.Sleeper] * 1000);
            BroadPhaseUpdate.Add(simulation.Profiler[simulation.BroadPhase] * 1000);
            CollisionTesting.Add(simulation.Profiler[simulation.BroadPhaseOverlapFinder] * 1000);
            NarrowPhaseFlush.Add(simulation.Profiler[simulation.NarrowPhase] * 1000);
            Solver.Add(simulation.Profiler[simulation.Solver]);
            BatchCompressor.Add(simulation.Profiler[simulation.SolverBatchCompressor]);

            ImPlot.SetNextAxesToFit();
            if (ImPlot.BeginPlot("Frame", new Vector2(-1, 0), ImPlotFlags.NoInputs))
            {
                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                ImPlot.PlotShaded("Total", ref Frame.Values[0], Frame.Length, fill, 1, 0, ImPlotShadedFlags.None, Frame.Head);
                ImPlot.PlotShaded("Graphics", ref Graphics.Values[0], Graphics.Length, fill, 1, 0, ImPlotShadedFlags.None, Graphics.Head);
                ImPlot.PlotShaded("Physics", ref Simulation.Values[0], Simulation.Length, fill, 1, 0, ImPlotShadedFlags.None, Simulation.Head);
                ImPlot.PlotShaded("Updates", ref Updates.Values[0], Updates.Length, fill, 1, 0, ImPlotShadedFlags.None, Updates.Head);
                ImPlot.PlotShaded("Systems", ref Systems.Values[0], Systems.Length, fill, 1, 0, ImPlotShadedFlags.None, Systems.Head);
                ImPlot.PopStyleVar();

                ImPlot.PlotLine("Total", ref Frame.Values[0], Frame.Length, 1, 0, ImPlotLineFlags.None, Frame.Head);
                ImPlot.PlotLine("Graphics", ref Graphics.Values[0], Graphics.Length, 1, 0, ImPlotLineFlags.None, Graphics.Head);
                ImPlot.PlotLine("Physics", ref Simulation.Values[0], Simulation.Length, 1, 0, ImPlotLineFlags.None, Simulation.Head);
                ImPlot.PlotLine("Updates", ref Updates.Values[0], Updates.Length, 1, 0, ImPlotLineFlags.None, Updates.Head);
                ImPlot.PlotLine("Systems", ref Systems.Values[0], Systems.Length, 1, 0, ImPlotLineFlags.None, Systems.Head);
                ImPlot.EndPlot();
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
                    var value = scene.Profiler[system];
                    if (!systems.TryGetValue(system, out var buffer))
                    {
                        buffer = new RingBuffer(512);
                        systems.Add(system, buffer);
                    }
                    buffer.Add(value * 1000);
                    systems[system] = buffer;

                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                    ImPlot.PlotShaded(system.Name, ref buffer.Values[0], buffer.Length, fill, 1, 0, ImPlotShadedFlags.None, buffer.Head);
                    ImPlot.PopStyleVar();

                    ImPlot.PlotLine(system.Name, ref buffer.Values[0], buffer.Length, 1, 0, ImPlotLineFlags.None, buffer.Head);
                }

                ImPlot.EndPlot();
            }

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
}