namespace HexaEngine.Core.Debugging
{
    using ImGuiNET;
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Text;

    public class OutputTerminal : ITerminal
    {
        private readonly List<TerminalMessage> messages = new();

        private bool coloredOutput = true;
        private bool autoScroll = true;

        private readonly TerminalColorPalette colorPalette = new();

        private bool scrollToBottom;
        private readonly TerminalTraceListener traceListener;
        private readonly TerminalConsoleRedirect consoleRedirect;

        public OutputTerminal()
        {
            traceListener = new(this);
            Trace.Listeners.Add(traceListener);
            consoleRedirect = new(this);
            Console.SetOut(consoleRedirect);

            Console.WriteLine("Hello World");
        }

        private class TerminalTraceListener : TraceListener
        {
            private readonly OutputTerminal outputTerminal;

            public TerminalTraceListener(OutputTerminal outputTerminal)
            {
                this.outputTerminal = outputTerminal;
            }

            public override void Write(string? message)
            {
                if (message == null)
                {
                    return;
                }

                string[] lines = message.Split(Environment.NewLine);
                if (outputTerminal.messages.Count != 0 && !outputTerminal.messages[^1].Text.EndsWith(Environment.NewLine))
                {
                    var msg = outputTerminal.messages[^1];
                    msg.Text += lines[0];
                    outputTerminal.messages[^1] = msg;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            outputTerminal.messages.Clear();
                        }
                        else if (i > 0)
                        {
                            outputTerminal.AddMessage(lines[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            outputTerminal.messages.Clear();
                        }
                        else
                        {
                            outputTerminal.AddMessage(lines[i]);
                        }
                    }
                }
            }

            public override void WriteLine(string? message)
            {
                if (message == null)
                {
                    return;
                }

                outputTerminal.AddMessage(message);
            }
        }

        private class TerminalConsoleRedirect : TextWriter
        {
            private readonly OutputTerminal terminal;

            public TerminalConsoleRedirect(OutputTerminal terminal)
            {
                this.terminal = terminal;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(string? value)
            {
                if (value == null)
                {
                    base.Write(value);
                    return;
                }
                terminal.Write(value);
                base.Write(value);
            }
        }

        public void Write(string text)
        {
            AddMessage(text);
        }

        public void WriteLine(string text)
        {
            Write(text + Environment.NewLine);
        }

        private void AddMessage(string text)
        {
            var color = TerminalColor.Text;
            if (text.Contains("error", StringComparison.CurrentCultureIgnoreCase))
            {
                color = TerminalColor.Error;
            }

            if (text.Contains("err", StringComparison.CurrentCultureIgnoreCase))
            {
                color = TerminalColor.Error;
            }

            if (text.Contains("warn", StringComparison.CurrentCultureIgnoreCase))
            {
                color = TerminalColor.Warning;
            }

            if (text.Contains("wrn", StringComparison.CurrentCultureIgnoreCase))
            {
                color = TerminalColor.Warning;
            }

            if (text.Contains("warning", StringComparison.CurrentCultureIgnoreCase))
            {
                color = TerminalColor.Warning;
            }

            lock (messages)
            {
                messages.Add(new TerminalMessage() { Text = text, Color = color });
            }
            scrollToBottom = true;
        }

        public void Draw()
        {
            DrawMenuBar();
            DrawMessages();
        }

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                // Settings menu.
                if (ImGui.BeginMenu("Settings"))
                {
                    // Colored output
                    ImGui.Checkbox("Colored Output", ref coloredOutput);
                    ImGui.SameLine();
                    HelpMaker("Enable colored command output");

                    // Auto Scroll
                    ImGui.Checkbox("Auto Scroll", ref autoScroll);
                    ImGui.SameLine();
                    HelpMaker("Automatically scroll to bottom of console log");

                    ImGui.EndMenu();
                }

                // View settings.
                if (ImGui.BeginMenu("Appearance"))
                {
                    // Logging Colors
                    ImGuiColorEditFlags flags =
                            ImGuiColorEditFlags.Float | ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar;

                    ImGui.TextUnformatted("Color Palette");
                    ImGui.Indent();
                    ImGui.ColorEdit4("Text##", ref colorPalette[TerminalColor.Text], flags);
                    ImGui.ColorEdit4("Warning##", ref colorPalette[TerminalColor.Warning], flags);
                    ImGui.ColorEdit4("Error##", ref colorPalette[TerminalColor.Error], flags);

                    ImGui.Unindent();

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private void DrawMessages()
        {
            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            lock (messages)
            {
                ImGui.BeginChild(1, new Vector2(0, -footerHeightToReserve));
                for (int i = 0; i < messages.Count; i++)
                {
                    var msg = messages[i];
                    if (coloredOutput)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, colorPalette[msg.Color]);
                        ImGui.TextUnformatted(msg.Text);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextUnformatted(msg.Text);
                    }
                }
                if (scrollToBottom && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || autoScroll))
                {
                    ImGui.SetScrollHereY(1.0f);
                }

                scrollToBottom = false;
                ImGui.EndChild();
            }
        }

        private static void HelpMaker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
}