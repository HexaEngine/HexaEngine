namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Physics;
    using HexaEngine.Core.Scenes;
    using ImPlotNET;
    using System.Diagnostics;
    using System.Numerics;

    public unsafe struct RingBuffer<T> where T : unmanaged
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

        public RingBuffer<double> Frame = new(512);
        public RingBuffer<double> Graphics = new(512);
        public RingBuffer<double> Updates = new(512);

        public RingBuffer<double> Systems = new(512);
        private readonly Dictionary<object, RingBuffer<double>> systems = new();

        public RingBuffer<double> Update = new(512);
        public RingBuffer<double> Prepass = new(512);
        public RingBuffer<double> ObjectCulling = new(512);
        public RingBuffer<double> LightCulling = new(512);
        public RingBuffer<double> Geometry = new(512);
        public RingBuffer<double> SSAO = new(512);
        public RingBuffer<double> Lights = new(512);
        public RingBuffer<double> PostProcess = new(512);

        public RingBuffer<double> Simulation = new(512);
        public RingBuffer<double> PoseIntegrator = new(512);
        public RingBuffer<double> Sleeper = new(512);
        public RingBuffer<double> BroadPhaseUpdate = new(512);
        public RingBuffer<double> CollisionTesting = new(512);
        public RingBuffer<double> NarrowPhaseFlush = new(512);
        public RingBuffer<double> Solver = new(512);
        public RingBuffer<double> BatchCompressor = new(512);

        public RingBuffer<double> MemoryUsage = new(512);
        public RingBuffer<double> VideoMemoryUsage = new(512);

        public override void DrawContent(IGraphicsContext context)
        {
            const int shade_mode = 2;
            const float fill_ref = 0;
            double fill = shade_mode == 0 ? -double.PositiveInfinity : shade_mode == 1 ? double.PositiveInfinity : fill_ref;

            Scene? scene = SceneManager.Current;

            Frame.Add(Time.Delta * 1000);

            MemoryUsage.Add(Process.GetCurrentProcess().PrivateMemorySize64 / 1000d / 1000d);
            VideoMemoryUsage.Add(GraphicsAdapter.Current.GetMemoryCurrentUsage() / 1000d / 1000d);

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

            if (scene == null)
            {
                return;
            }

            var renderer = Application.MainWindow.Renderer;
            var simulation = scene.GetRequiredSystem<PhysicsSystem>().Simulation;

            Graphics.Add(renderer.Profiler[renderer] * 1000);
            Systems.Add(scene.Profiler[scene.Systems] * 1000);

            Update.Add(renderer.Profiler[renderer.Update] * 1000);
            ObjectCulling.Add(renderer.Profiler[renderer.ObjectCulling] * 1000);
            LightCulling.Add(renderer.Profiler[renderer.LightCulling] * 1000);
            Prepass.Add(renderer.Profiler[renderer.Prepass] * 1000);
            Geometry.Add(renderer.Profiler[renderer.Geometry] * 1000);
            SSAO.Add(renderer.Profiler[renderer.SSAO] * 1000);
            Lights.Add(renderer.Profiler[renderer.Lights] * 1000);
            PostProcess.Add(renderer.Profiler[renderer.PostProcess] * 1000);

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
                        buffer = new RingBuffer<double>(512);
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
            if (ImPlot.BeginPlot("Graphics (CPU Latency)", new Vector2(-1, 0), ImPlotFlags.NoInputs))
            {
                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                ImPlot.PlotShaded("Total", ref Graphics.Values[0], Graphics.Length, fill, 1, 0, ImPlotShadedFlags.None, Graphics.Head);
                ImPlot.PlotShaded("Update", ref Update.Values[0], Update.Length, fill, 1, 0, ImPlotShadedFlags.None, Update.Head);
                ImPlot.PlotShaded("Prepass", ref Prepass.Values[0], Prepass.Length, fill, 1, 0, ImPlotShadedFlags.None, Prepass.Head);
                ImPlot.PlotShaded("Object Culling", ref ObjectCulling.Values[0], ObjectCulling.Length, fill, 1, 0, ImPlotShadedFlags.None, ObjectCulling.Head);
                ImPlot.PlotShaded("Light Culling", ref LightCulling.Values[0], LightCulling.Length, fill, 1, 0, ImPlotShadedFlags.None, LightCulling.Head);
                ImPlot.PlotShaded("Geometry", ref Geometry.Values[0], Geometry.Length, fill, 1, 0, ImPlotShadedFlags.None, Geometry.Head);
                ImPlot.PlotShaded("SSAO", ref SSAO.Values[0], SSAO.Length, fill, 1, 0, ImPlotShadedFlags.None, SSAO.Head);
                ImPlot.PlotShaded("Lights", ref Lights.Values[0], Lights.Length, fill, 1, 0, ImPlotShadedFlags.None, Lights.Head);
                ImPlot.PlotShaded("PostProcess", ref PostProcess.Values[0], PostProcess.Length, fill, 1, 0, ImPlotShadedFlags.None, PostProcess.Head);
                ImPlot.PopStyleVar();

                ImPlot.PlotLine("Total", ref Graphics.Values[0], Graphics.Length, 1, 0, ImPlotLineFlags.None, Graphics.Head);
                ImPlot.PlotLine("Update", ref Update.Values[0], Update.Length, 1, 0, ImPlotLineFlags.None, Update.Head);
                ImPlot.PlotLine("Prepass", ref Prepass.Values[0], Prepass.Length, 1, 0, ImPlotLineFlags.None, Prepass.Head);
                ImPlot.PlotLine("Object Culling", ref ObjectCulling.Values[0], ObjectCulling.Length, 1, 0, ImPlotLineFlags.None, ObjectCulling.Head);
                ImPlot.PlotLine("Light Culling", ref LightCulling.Values[0], LightCulling.Length, 1, 0, ImPlotLineFlags.None, LightCulling.Head);
                ImPlot.PlotLine("Geometry", ref Geometry.Values[0], Geometry.Length, 1, 0, ImPlotLineFlags.None, Geometry.Head);
                ImPlot.PlotLine("SSAO", ref SSAO.Values[0], SSAO.Length, 1, 0, ImPlotLineFlags.None, SSAO.Head);
                ImPlot.PlotLine("Lights", ref Lights.Values[0], Lights.Length, 1, 0, ImPlotLineFlags.None, Lights.Head);
                ImPlot.PlotLine("PostProcess", ref PostProcess.Values[0], PostProcess.Length, 1, 0, ImPlotLineFlags.None, PostProcess.Head);
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