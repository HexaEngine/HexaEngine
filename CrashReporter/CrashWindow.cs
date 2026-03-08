namespace CrashReporter
{
    using Hexa.NET.ImGui;
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Text;

    public sealed class CrashWindow
    {
        private readonly string? reportFile;
        private readonly string reportMessage;

        public CrashWindow()
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1 && File.Exists(args[1]))
            {
                reportFile = args[1];
                reportMessage = File.ReadAllText(args[1]);
            }
            else
            {
                reportMessage = "Couldn't load crash report!";
            }
        }

        public event Action<CrashWindow>? Closing;

        public void Draw()
        {
            bool shown = true;

            var vp = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(vp.Pos);
            ImGui.SetNextWindowSize(vp.Size);

            if (!ImGui.Begin("Crash report", ref shown, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse))
            {
                ImGui.End();
            }

            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            if (ImGui.BeginChild(1, new Vector2(0, -footerHeightToReserve), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.TextColored(new(1, 0, 0, 1), "Oops something went wrong!");
                ImGui.Text(reportFile ?? string.Empty);
                ImGui.Separator();
                var msg = reportMessage;
                var avail = ImGui.GetContentRegionAvail();
                int count = Encoding.UTF8.GetByteCount(msg) + 64;
                ImGui.InputTextMultiline("##Msg"u8, ref msg, (nuint)count, avail, ImGuiInputTextFlags.ReadOnly);
            }
            ImGui.EndChild();

            ImGui.SetNextItemWidth(-100);
            if (ImGui.Button("Ok"))
            {
                Close();
            }
            ImGui.SameLine();
            if (reportFile != null && ImGui.Button("Open Log"))
            {
                Process.Start(reportFile);
            }

            if (!shown)
            {
                Close();
            }

            ImGui.End();
        }

        public void Close()
        {
            Closing?.Invoke(this);
        }
    }
}