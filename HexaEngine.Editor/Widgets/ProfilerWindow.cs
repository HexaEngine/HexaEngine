namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImPlot;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Profiling;
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

    public class ProfilerWindow : EditorWindow
    {
        private const int SampleBufferSize = 500;
        private double[] downsampleBuffer = new double[100];
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

        protected override string Name => $"{UwU.ChartPie} Profiler";

        public RingBuffer<double> Frame = new(SampleBufferSize);

        public RingBuffer<double> TotalUpdate = new(SampleBufferSize);
        public RingBuffer<double> SceneTick = new(SampleBufferSize);
        public RingBuffer<double> FixedUpdate = new(SampleBufferSize);
        private readonly Dictionary<object, RingBuffer<double>> systems = new();
        private readonly Dictionary<string, RingBuffer<double>> cpuBlocks = new();
        private readonly Dictionary<string, RingBuffer<double>> gpuBlocks = new();

        public RingBuffer<double> MemoryUsage = new(SampleBufferSize) { AverageValues = false };
        public RingBuffer<double> VideoMemoryUsage = new(SampleBufferSize) { AverageValues = false };

        public int SamplesPerSecond { get => samplesPerSecond; set => samplesPerSecond = value; }

        public int SamplesPerSecondMem { get => samplesPerSecondMem; set => samplesPerSecondMem = value; }

        public float SampleLatency => 1f / SamplesPerSecond;

        public float MemSampleLatency => 1f / SamplesPerSecondMem;

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            ImGuiWindow* window = ImGui.GetCurrentWindow();
            ImDrawList* list = ImGui.GetWindowDrawList();

            Vector2 pos = window->DC.CursorPos;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Options"))
                {
                    if (ImGui.Checkbox("Profile", ref cpu))
                    {
                        full = cpu;
                        CPUProfiler.Global.Enabled = cpu;
                    }

                    if (ImGui.Checkbox("GPU Profile (heavy performance impact)", ref gpu))
                    {
                        context.Device.Profiler.Enabled = gpu;
                    }
                    ImGui.Checkbox("Memory Profile (extreme performance impact 4.5ms)", ref memory);
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
                var profiler = CPUProfiler.Global;
                ImGuiWidgetFlameGraph.PlotFlame("Flame", profiler.Getter, profiler.Current, profiler.StageCount, ref selected);
            }
            if (full)
            {
                DrawFull(context, list, pos);
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

        public unsafe void DrawFull(IGraphicsContext context, ImDrawList* list, Vector2 pos)
        {
            const int shade_mode = 2;
            const float fill_ref = 0;
            double fill = shade_mode == 0 ? -double.PositiveInfinity : shade_mode == 1 ? double.PositiveInfinity : fill_ref;

            ImGui.BeginTabBar("Profiler", ImGuiTabBarFlags.None);

            if (ImGui.BeginTabItem("Frame"))
            {
                {
                    Vector2 avail = ImGui.GetContentRegionAvail();

                    if (cpu && gpu)
                    {
                        avail.X *= 0.5f;
                    }

                    ImPlot.SetNextAxesToFit();

                    if (cpu)
                    {
                        /*
                        if (ImPlot.BeginPlot("Frame (CPU Latency)", new Vector2(avail.X, 0), ImPlotFlags.NoInputs))
                        {
                            var profiler = CPUProfiler2.Global;
                            var blockNames = profiler.BlockNames;
                            lock (blockNames)
                            {
                                for (int i = 0; i < blockNames.Count; i++)
                                {
                                    var name = blockNames[i];
                                    if (!cpuBlocks.TryGetValue(name, out var buffer))
                                    {
                                        continue;
                                    }

                                    int count = Math.Min(100, buffer.Count);
                                    LTTBAlgorithm.Downsample(buffer, count, downsampleBuffer);

                                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                                    ImPlot.PlotShaded(name, ref downsampleBuffer[0], count, fill, 1, 0, ImPlotShadedFlags.None);
                                    ImPlot.PopStyleVar();

                                    ImPlot.PlotLine(name, ref downsampleBuffer[0], count, 1, 0, ImPlotLineFlags.None);
                                }
                            }

                            ImPlot.EndPlot();
                        }
                        */
                    }

                    if (gpu)
                    {
                        ImGui.SameLine();
                        ImPlot.SetNextAxesToFit();
                        if (ImPlot.BeginPlot("Frame (GPU Latency)", new Vector2(avail.X, 0), ImPlotFlags.NoInputs))
                        {
                            var profiler = context.Device.Profiler;
                            var blockNames = profiler.BlockNames;
                            lock (blockNames)
                            {
                                for (int i = 0; i < blockNames.Count; i++)
                                {
                                    var name = blockNames[i];
                                    if (!gpuBlocks.TryGetValue(name, out var buffer))
                                    {
                                        continue;
                                    }

                                    ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                                    ImPlot.PlotShaded(name, ref buffer.Values[0], buffer.Length, fill, 1, 0, ImPlotShadedFlags.None, buffer.Head);
                                    ImPlot.PopStyleVar();

                                    ImPlot.PlotLine(name, ref buffer.Values[0], buffer.Length, 1, 0, ImPlotLineFlags.None, buffer.Head);
                                }
                            }

                            ImPlot.EndPlot();
                        }
                    }
                }

                unsafe
                {
                    var profiler = CPUProfiler.Global;
                    var entry = profiler.Current;

                    if (entry->Stages.Count > 0)
                    {
                        float textHeight = ImGui.GetTextLineHeightWithSpacing();
                        Vector2 avail = ImGui.GetContentRegionAvail();

                        double duration = (entry->End - entry->Start) / (double)Stopwatch.Frequency * 1000;

                        Vector2 cursorPos = ImGui.GetCursorPos();
                        cursorPos.Y -= textHeight * 3;
                        cursorPos.X = 0;

                        int i = 0;
                        int level = 0;
                        foreach (ProfilerScope stage in entry->Stages.OrderBy(x => x.Start))
                        {
                            if (stage.Level > level)
                            {
                                continue;
                            }

                            if (stage.Level < level)
                            {
                                int delta = level - stage.Level;
                                for (int j = 0; j < delta; j++)
                                {
                                    ImGui.TreePop();
                                    level--;
                                }
                            }
                            double ratio = (stage.LastEndSample - stage.LastStartSample) / duration;
                            if (i != 0)
                            {
                                Vector2 min = pos + cursorPos;
                                min.Y += i * textHeight;
                                Vector2 max = min + new Vector2(avail.X, textHeight);
                                var color = 0x000000ff | (uint)((byte)(ratio * byte.MaxValue) << 24);
                                list->AddRectFilled(min, max, color);
                            }

                            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;
                            if (i == 0)
                            {
                                flags |= ImGuiTreeNodeFlags.DefaultOpen;
                            }
                            if (stage.Leaf)
                            {
                                flags |= ImGuiTreeNodeFlags.Leaf;
                            }

                            var currPos = ImGui.GetCursorPos();
                            bool open = ImGui.TreeNodeEx(stage.Name.Data, flags);
                            if (open)
                            {
                                level++;
                            }
                            ImGui.SetCursorPos(new(avail.X - 200, currPos.Y));
                            ImGui.Text($"{stage.LastEndSample - stage.LastStartSample:n2}ms");
                            ImGui.SetCursorPos(new(avail.X - 100, currPos.Y));
                            ImGui.Text($"{ratio * 100:n2}%");
                            i++;
                        }

                        for (int j = 0; j < level; j++)
                        {
                            ImGui.TreePop();
                        }
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

            if (cpu)
            {
                var profiler = CPUProfiler.Global;
                var blockNames = profiler.BlockNames;
                lock (blockNames)
                {
                    for (int i = 0; i < blockNames.Count; i++)
                    {
                        var name = blockNames[i];
                        var value = profiler[name];
                        if (!cpuBlocks.TryGetValue(name, out var buffer))
                        {
                            buffer = new(SampleBufferSize);
                            cpuBlocks.Add(name, buffer);
                        }
                        buffer.Add(value.Duration * 1000);
                    }
                }
            }

            if (gpu)
            {
                var profiler = context.Device.Profiler;
                var blockNames = profiler.BlockNames;
                lock (blockNames)
                {
                    for (int i = 0; i < blockNames.Count; i++)
                    {
                        var name = blockNames[i];
                        var value = profiler[name];
                        if (!gpuBlocks.TryGetValue(name, out var buffer))
                        {
                            buffer = new(SampleBufferSize);
                            gpuBlocks.Add(name, buffer);
                        }
                        buffer.Add(value * 1000);
                    }
                }
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