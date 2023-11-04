namespace HexaEngine.Core.Debugging
{
    using Hexa.NET.ImGui;
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

        public bool Shown { get; }

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
                if (outputTerminal.messages.Count != 0 && !outputTerminal.messages[^1].Message.EndsWith(Environment.NewLine))
                {
                    var msg = outputTerminal.messages[^1];
                    msg.Message += lines[0];
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
            lock (messages)
            {
                messages.Add(new TerminalMessage() { Message = text, Color = TerminalColor.Black });
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

                    ImGui.Unindent();

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private unsafe void DrawMessages()
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
                        ImGui.TextUnformatted(msg.Message);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextUnformatted(msg.Message);
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

        public void Focus()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Show()
        {
            throw new NotImplementedException();
        }
    }
}