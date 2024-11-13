namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using System;
    using System.Numerics;

    public sealed class ImportProgressModal : Modal, IDisposable, IImportProgress
    {
        private readonly string title;
        private readonly string message;
        private readonly ProgressFlags progressFlags;
        private readonly Vector2 offsetCenter;
        private float progress;
        private readonly List<Step> steps = [];
        private int current;
        private Stack<int> stepStack = [];
        private DateTime started;
        private DateTime ended;

        private bool blockClose = false;
        private bool wantsClosing = false;

        public ImportProgressModal(string title, string message, ProgressFlags progressFlags = ProgressFlags.None)
        {
            this.title = title;
            this.message = message;
            this.progressFlags = progressFlags;
        }

        private struct Step
        {
            public string Title;
            public List<LogMessage> Messages;
            public bool Done;
            public bool Error;
            public bool Warn;

            public DateTime Started;
            public DateTime Ended;

            public Step(string title)
            {
                Title = title;
                Messages = [];
            }
        }

        public override string Name => title;

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse;

        public override unsafe void Draw()
        {
            if (!shown || signalClose)
            {
                if ((progressFlags & ProgressFlags.NoModal) != 0)
                {
                    shown = false;
                    return;
                }
                base.Draw();
                return;
            }

            if ((progressFlags & ProgressFlags.NoOverlay) == 0)
            {
                Vector2 main_viewport_pos = ImGui.GetMainViewport().Pos;
                Vector2 main_viewport_size = ImGui.GetMainViewport().Size;
                ImGui.SetNextWindowPos(main_viewport_pos);
                ImGui.SetNextWindowSize(main_viewport_size);
                ImGui.SetNextWindowBgAlpha(0.9f);
                ImGui.Begin("Overlay", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
                ImGui.End();
            }
            if ((progressFlags & ProgressFlags.NoModal) != 0)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 30);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 8);
                ImGui.PushStyleColor(ImGuiCol.Border, 0xff4c4c4c);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xff1c1c1c);
                if (ImGui.Begin(Name, ref shown, Flags))
                {
                    DrawContent();
                    ImGui.End();
                }
                ImGui.PopStyleColor(2);
                ImGui.PopStyleVar(2);
            }
            else
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 30);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 8);
                ImGui.PushStyleColor(ImGuiCol.Border, 0xff4c4c4c);

                base.Draw();

                ImGui.PopStyleColor();
                ImGui.PopStyleVar(2);
            }
        }

        protected override unsafe void DrawContent()
        {
            var windowSize = ImGui.GetWindowSize();
            var mainViewport = ImGui.GetMainViewport();
            float offsetX = mainViewport.Size.X * offsetCenter.X;
            float offsetY = mainViewport.Size.Y * offsetCenter.Y;
            Vector2 winPos;
            if ((progressFlags & ProgressFlags.TopLeft) != 0)
            {
                Vector2 pos = mainViewport.Pos;
                Vector2 size = mainViewport.Size;
                pos.Y += offsetY;
                pos.X += offsetX;
                ImGuiP.SetWindowPos(winPos = pos);
            }
            else if ((progressFlags & ProgressFlags.BottomLeft) != 0)
            {
                Vector2 pos = mainViewport.Pos;
                Vector2 size = mainViewport.Size;
                pos.Y += size.Y - windowSize.Y - offsetY;
                pos.X += offsetX;
                ImGuiP.SetWindowPos(winPos = pos);
            }
            else
            {
                Vector2 mainViewportPos = mainViewport.Pos;
                var s = ImGui.GetPlatformIO().Monitors.Data[0].MainSize;
                ImGuiP.SetWindowPos(winPos = mainViewportPos + (s / 2 - windowSize / 2));
            }

            {
                ProgressBar(message, progress);
            }

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                DateTime stepEnd = step.Ended;
                if (step.Done)
                {
                    if (step.Error)
                    {
                        ImGui.TextColored(Colors.Crimson, $"{UwU.Xmark}");
                    }
                    else if (step.Warn)
                    {
                        ImGui.TextColored(Colors.Yellow, $"{UwU.Warning}");
                    }
                    else
                    {
                        ImGui.TextColored(Colors.Green, $"{UwU.Check}");
                    }
                }
                else
                {
                    stepEnd = DateTime.Now;
                    var height = ImGui.GetTextLineHeight();
                    Spinner($"##{step.Title}", height * 0.5f, 2, 0xffcf7334);
                }

                ImGui.SameLine();
                bool open = ImGui.TreeNodeEx(step.Title, ImGuiTreeNodeFlags.NoTreePushOnOpen);

                ImGui.SameLine();

                var stepElapsed = stepEnd - step.Started;

                ImGui.Text($"Elapsed: {stepElapsed.Minutes}m {stepElapsed.Seconds}s");

                if (open)
                {
                    RenderMessages(step.Title, step.Messages);

                    blockClose = true;
                }
            }

            DateTime end = ended;
            if (!wantsClosing)
            {
                end = DateTime.Now;
            }

            var elapsed = end - started;

            ImGui.Text($"Elapsed: {elapsed.Minutes}m {elapsed.Seconds}s");

            if (wantsClosing && ImGui.Button("Close"))
            {
                blockClose = false;
                Close();
            }
        }

        public static unsafe void Spinner(string label, float radius, float thickness, uint color)
        {
            var window = ImGuiP.GetCurrentWindow();
            if (window.SkipItems)
            {
                return;
            }

            uint id = ImGui.GetID(label);

            var pos = ImGui.GetCursorScreenPos();

            Vector2 size = new(radius * 2, radius * 2);

            ImRect bb = new(pos, pos + size);

            ImGuiP.ItemSize(bb, -1);
            if (!ImGuiP.ItemAdd(bb, id, null, ImGuiItemFlags.None))
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            var g = ImGui.GetCurrentContext();

            // Render
            ImGui.PathClear(drawList);

            const int num_segments = 24;

            int start = (int)Math.Abs(MathF.Sin((float)(g.Time * 1.8f)) * (num_segments - 5));

            float a_min = float.Pi * 2.0f * start / num_segments;
            float a_max = float.Pi * 2.0f * ((float)num_segments - 3) / num_segments;

            Vector2 center = pos + new Vector2(radius, radius);

            radius -= thickness;

            for (var i = 0; i < num_segments; i++)
            {
                float a = a_min + i / (float)num_segments * (a_max - a_min);
                var time = (float)g.Time;
                Vector2 pp = new(center.X + MathF.Cos(a + time * 8) * radius, center.Y + MathF.Sin(a + time * 8) * radius);
                drawList.PathLineTo(pp);
            }

            drawList.PathStroke(color, 0, thickness);
        }

        private static void RenderMessages(string title, List<LogMessage> messages)
        {
            var style = ImGui.GetStyle();
            const float maxHeight = 400;
            var lineHeight = ImGui.GetTextLineHeightWithSpacing();
            var heightComputed = messages.Count * lineHeight + style.FramePadding.Y * 2 + style.ChildBorderSize * 2;
            var avail = ImGui.GetContentRegionAvail();
            ImGui.PushStyleColor(ImGuiCol.ChildBg, 0xFF3c3c3c);
            if (ImGui.BeginChild(title, new Vector2(Math.Max(avail.X, 300), Math.Min(heightComputed, maxHeight)), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar))
            {
                for (int j = 0; j < messages.Count; j++)
                {
                    var msg = messages[j];
                    ImGui.TextColored(GetColor(msg.Severity), msg.Message);
                }
            }
            ImGui.PopStyleColor();

            ImGui.EndChild();
        }

        private static Vector4 GetColor(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Trace => new Vector4(0.25f, 0.25f, 0.25f, 1.0f),      // DarkGray
                LogSeverity.Debug => new Vector4(0.0f, 0.5f, 0.5f, 1.0f),        // DarkCyan
                LogSeverity.Info => new Vector4(1.0f, 1.0f, 1.0f, 1.0f),         // White
                LogSeverity.Warning => new Vector4(1.0f, 1.0f, 0.0f, 1.0f),      // Yellow
                LogSeverity.Error => new Vector4(1.0f, 0.0f, 0.0f, 1.0f),        // Red
                LogSeverity.Critical => new Vector4(0.5f, 0.0f, 0.5f, 1.0f),     // Magenta
                _ => new Vector4(1.0f, 1.0f, 1.0f, 1.0f),                        // Default: White
            };
        }

        private static unsafe void ProgressBar(string title, float progress)
        {
            ImGuiWindowPtr win = ImGuiP.GetCurrentWindow();

            if (win.SkipItems)
            {
                return;
            }

            uint id = ImGui.GetID(title);

            Vector2 textSize = ImGui.CalcTextSize(title);

            const float padding = 5;
            float height = padding * 2 + textSize.Y;

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 avail = ImGui.GetContentRegionAvail();

            Vector2 size = new(Math.Max(avail.X, textSize.X + padding * 2), height);
            ImRect bb = new(pos, pos + size);

            ImGuiP.ItemSize(bb, 0);
            if (!ImGuiP.ItemAdd(bb, id, ref bb, ImGuiItemFlags.None))
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            Vector2 innerRectMax = bb.Min + new Vector2(avail.X * progress, height);
            drawList.AddRectFilled(bb.Min, innerRectMax, 0xffcf7334, 30, ImDrawFlags.RoundCornersAll);
            drawList.AddRect(bb.Min, bb.Max, 0xff3c3c3c, 30, ImDrawFlags.RoundCornersAll);

            Vector2 textPos = bb.Min + new Vector2(padding, (height - textSize.Y) * 0.5f); // center to progress rect.
            drawList.AddText(textPos, 0xffffffff, title);
        }

        public override void Show()
        {
            started = DateTime.Now;
            base.Show();
        }

        public void Dispose()
        {
            Close();
        }

        public override void Close()
        {
            ended = DateTime.Now;
            wantsClosing = true;
            if (blockClose)
            {
                return;
            }

            base.Close();
        }

        public override void Reset()
        {
            progress = 0f;
            steps.Clear();
            blockClose = false;
            wantsClosing = false;
        }

        public void Report(float value)
        {
            progress = MathUtil.Clamp01(value);
        }

        public void BeginStep(string title)
        {
            if (current != -1)
            {
                stepStack.Push(current);
            }

            current = steps.Count;
            steps.Add(new Step { Title = title, Messages = [], Started = DateTime.Now });
        }

        public void LogMessage(LogMessage message)
        {
            if (message.Severity == LogSeverity.Critical || message.Severity == LogSeverity.Error || message.Severity == LogSeverity.Warning)
            {
                blockClose = true;
            }

            if (current == -1)
            {
                return;
            }

            var step = steps[current];
            step.Messages.Add(message);
            if (message.Severity == LogSeverity.Critical || message.Severity == LogSeverity.Error)
            {
                step.Error = true;
            }
            if (message.Severity == LogSeverity.Warning)
            {
                step.Warn = true;
            }
            steps[current] = step;
        }

        public void EndStep()
        {
            if (current == -1)
            {
                return;
            }

            var step = steps[current];
            step.Done = true;
            step.Ended = DateTime.Now;
            steps[current] = step;

            if (stepStack.TryPop(out var last))
            {
                current = last;
            }
        }
    }
}