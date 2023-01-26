namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using ImPlotNET;
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
                value = 0;
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
                head += length;
        }
    }

    public class ProfilerWindow : ImGuiWindow
    {
        protected override string Name => "SceneProfiler";

        public RingBuffer<double> Frame = new(512);
        public RingBuffer<double> Graphics = new(512);
        public RingBuffer<double> Updates = new(512);
        public RingBuffer<double> Systems = new(512);

        public RingBuffer<double> Simulation = new(512);
        public RingBuffer<double> PoseIntegrator = new(512);
        public RingBuffer<double> Sleeper = new(512);
        public RingBuffer<double> BroadPhaseUpdate = new(512);
        public RingBuffer<double> CollisionTesting = new(512);
        public RingBuffer<double> NarrowPhaseFlush = new(512);
        public RingBuffer<double> Solver = new(512);
        public RingBuffer<double> BatchCompressor = new(512);

        public override void DrawContent(IGraphicsContext context)
        {
            const int shade_mode = 2;
            const float fill_ref = 0;
            double fill = shade_mode == 0 ? -double.PositiveInfinity : shade_mode == 1 ? double.PositiveInfinity : fill_ref;

            Scene? scene = SceneManager.Current;
            if (scene == null) return;

            var renderer = Application.MainWindow.Renderer;
            var simulation = scene.Simulation;

            Frame.Add(Time.Delta * 1000);
            Graphics.Add(renderer.Profiler[renderer] * 1000);
            Updates.Add(scene.Profiler[scene.UpdateCallbacks] * 1000);
            Systems.Add(scene.Profiler[scene.Systems] * 1000);

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
                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.8f);
                ImPlot.PlotShaded("Total", ref Frame.Values[0], Frame.Length, fill, 1, 0, ImPlotShadedFlags.None, Frame.Head);
                ImPlot.PlotShaded("Graphics", ref Graphics.Values[0], Graphics.Length, fill, 1, 0, ImPlotShadedFlags.None, Graphics.Head);
                ImPlot.PlotShaded("Physics", ref Simulation.Values[0], Simulation.Length, fill, 1, 0, ImPlotShadedFlags.None, Simulation.Head);
                ImPlot.PlotShaded("Updates", ref Updates.Values[0], Updates.Length, fill, 1, 0, ImPlotShadedFlags.None, Updates.Head);
                ImPlot.PlotShaded("Systems", ref Systems.Values[0], Systems.Length, fill, 1, 0, ImPlotShadedFlags.None, Systems.Head);
                ImPlot.PopStyleVar();
                ImPlot.EndPlot();
            }

            ImPlot.SetNextAxesToFit();
            if (ImPlot.BeginPlot("Physics", new Vector2(-1, 0), ImPlotFlags.NoInputs))
            {
                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.8f);
                ImPlot.PlotShaded("Total", ref Simulation.Values[0], Simulation.Length, fill, 1, 0, ImPlotShadedFlags.None, Simulation.Head);
                ImPlot.PlotShaded("PoseIntegrator", ref PoseIntegrator.Values[0], PoseIntegrator.Length, fill, 1, 0, ImPlotShadedFlags.None, PoseIntegrator.Head);
                ImPlot.PlotShaded("Sleeper", ref Sleeper.Values[0], Sleeper.Length, fill, 1, 0, ImPlotShadedFlags.None, Sleeper.Head);
                ImPlot.PlotShaded("BroadPhaseUpdate", ref BroadPhaseUpdate.Values[0], BroadPhaseUpdate.Length, fill, 1, 0, ImPlotShadedFlags.None, BroadPhaseUpdate.Head);
                ImPlot.PlotShaded("CollisionTesting", ref CollisionTesting.Values[0], CollisionTesting.Length, fill, 1, 0, ImPlotShadedFlags.None, CollisionTesting.Head);
                ImPlot.PlotShaded("NarrowPhaseFlush", ref NarrowPhaseFlush.Values[0], NarrowPhaseFlush.Length, fill, 1, 0, ImPlotShadedFlags.None, NarrowPhaseFlush.Head);
                ImPlot.PlotShaded("Solver", ref Solver.Values[0], Solver.Length, fill, 1, 0, ImPlotShadedFlags.None, Solver.Head);
                ImPlot.PlotShaded("BatchCompressor", ref BatchCompressor.Values[0], BatchCompressor.Length, fill, 1, 0, ImPlotShadedFlags.None, BatchCompressor.Head);

                ImPlot.PopStyleVar();
                ImPlot.EndPlot();
            }
        }
    }
}