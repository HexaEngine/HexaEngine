namespace HexaEngine.Core.Debugging
{
    using Hexa.NET.ImGui;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a shell terminal.
    /// </summary>
    public class ShellTerminal : ITerminal
    {
        private readonly List<TerminalMessage> messages = new();
        private readonly List<string> history = new();
        private string inBuffer = string.Empty;

        private bool coloredOutput = true;
        private bool autoScroll = true;
        private readonly TerminalColorPalette colorPalette = new();

        private bool scrollToBottom;
        private int historyIndex;

        private readonly ConsoleAppManager consoleAppManager;

        private ImGuiInputTextCallback textCallback;

        /// <summary>
        /// Gets a value indicating whether the terminal is shown.
        /// </summary>
        public bool Shown { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellTerminal"/> class.
        /// </summary>
        /// <param name="exec">The command to execute in the terminal.</param>
        public unsafe ShellTerminal(string exec)
        {
            consoleAppManager = new(exec);
            consoleAppManager.ExecuteAsync();
            consoleAppManager.StandardTextReceived += StandardTextReceived;
            consoleAppManager.ErrorTextReceived += ErrorTextReceived;
            textCallback = InputCallback;
        }

        private void ErrorTextReceived(object? sender, string e)
        {
            lock (messages)
            {
                string[] lines = e.Split(Environment.NewLine);
                if (messages.Count != 0 && !messages[^1].Message.EndsWith(Environment.NewLine))
                {
                    var msg = messages[^1];
                    msg.Message += lines[0];
                    messages[^1] = msg;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            messages.Clear();
                        }
                        else if (i > 0)
                        {
                            AddMessage(lines[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            messages.Clear();
                        }
                        else
                        {
                            AddMessage(lines[i]);
                        }
                    }
                }
                scrollToBottom = true;
            }
        }

        private void StandardTextReceived(object? sender, string e)
        {
            lock (messages)
            {
                string[] lines = e.Split(Environment.NewLine);
                if (messages.Count != 0 && !messages[^1].Message.EndsWith(Environment.NewLine))
                {
                    var msg = messages[^1];
                    msg.Message += lines[0];
                    messages[^1] = msg;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            messages.Clear();
                        }
                        else if (i > 0)
                        {
                            AddMessage(lines[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            messages.Clear();
                        }
                        else
                        {
                            AddMessage(lines[i]);
                        }
                    }
                }

                scrollToBottom = true;
            }
        }

        private void AddMessage(string text)
        {
            lock (messages)
            {
                messages.Add(new TerminalMessage() { Message = text, Color = TerminalColor.White });
            }
        }

        /// <inheritdoc/>
        public void Draw()
        {
            DrawMenuBar();
            ImGui.Separator();
            DrawMessages();
            DrawInput();
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
                ImGui.BeginChild(1, new Vector2(0, -footerHeightToReserve), ImGuiWindowFlags.HorizontalScrollbar);

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

        private unsafe void DrawInput()
        {
            bool reclaimFocus = false;
            ImGui.PushItemWidth(-ImGui.GetStyle().ItemSpacing.X * 7);
            if (ImGui.InputText("Input", ref inBuffer, 1024, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory))
            {
                consoleAppManager.Write(inBuffer + Environment.NewLine);
                history.Add(inBuffer);
                historyIndex = history.Count;
                inBuffer = string.Empty;

                reclaimFocus = true;
            }

            ImGui.SetItemDefaultFocus();
            if (reclaimFocus)
            {
                ImGui.SetKeyboardFocusHere(-1);
            }
        }

        private unsafe int InputCallback(ImGuiInputTextCallbackData* data)
        {
            switch (data->EventFlag)
            {
                case ImGuiInputTextFlags.CallbackHistory:
                    {
                        // Clear buffer.
                        data->BufTextLen = 0;

                        // Traverse history.
                        if (data->EventKey == ImGuiKey.UpArrow)
                        {
                            if (historyIndex > 0)
                            {
                                historyIndex--;
                            }
                        }
                        else
                        {
                            if (historyIndex < history.Count)
                            {
                                historyIndex++;
                            }
                        }

                        if (historyIndex >= 0 && historyIndex < history.Count)
                        {
                            // Get history.
                            string prevCommand = history[historyIndex];

                            // Insert commands.
                            for (int i = 0; i < data->BufSize; i++)
                            {
                                if (i < prevCommand.Length)
                                {
                                    data->Buf[i] = (byte)prevCommand[i];
                                }
                                else
                                {
                                    data->Buf[i] = 0;
                                }
                            }

                            data->BufTextLen = prevCommand.Length;
                        }
                        else
                        {
                            for (int i = 0; i < data->BufSize; i++)
                            {
                                data->Buf[i] = 0;
                            }
                        }
                        data->BufDirty = 1;
                    }
                    break;
            }
            return 1;
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

        /// <inheritdoc/>
        public void Close()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Show()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Focus()
        {
            throw new NotImplementedException();
        }
    }
}