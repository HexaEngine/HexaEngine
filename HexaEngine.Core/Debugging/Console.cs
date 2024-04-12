namespace HexaEngine.Core.Debugging
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Collections;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a console-like interface for logging, displaying messages, and accepting commands within an ImGui window.
    /// </summary>
    public static class ImGuiConsole
    {
        private static readonly List<ConsoleMessage> messages = new(maxMessages + 1);
        private static readonly List<string> history = new();
        private static readonly ConsoleLogWriter logListener = new();
        private static readonly Dictionary<string, Action<string[]>> commands = new();
        private static bool timeStamps;
        private static bool coloredOutput;
        private static bool scrollToBottom;
        private static bool autoScroll;
        private static string textFilter = string.Empty;
        private static string buffer = "";
        private static readonly uint bufferSize = 256;
        private static readonly List<string> cmdSuggestions = new();
        private static int historyIndex;
        private static readonly TernarySearchTreeDictionary<Action<string[]>> cmdAutocomplete = new();
        private static readonly TernarySearchTreeDictionary<string> varAutocomplete = new();
        private static bool filterBar;
        private static bool resetModal;
        private static float windowAlpha = 1;
        private static readonly ConsoleColorPalette consoleColorPalette = new();
        private static readonly string consoleName = "Console";
        private static bool consoleOpen;
        private static ConsoleColor foregroundColor = ConsoleColor.White;
        private static ConsoleColor backgroundColor = ConsoleColor.Black;
        private static readonly SemaphoreSlim semaphore = new(1);
        private static readonly unsafe ImGuiInputTextCallback inputTextCallback = InputCallback;
        private const int maxMessages = 4096;

        /// <summary>
        /// Initializes the ImGuiConsole, registers default commands, and sets default settings.
        /// </summary>
        public static void Initialize()
        {
            LoggerFactory.AddGlobalWriter(logListener);

            DefaultSettings();

            RegisterCommand("clear", _ =>
            {
                Clear();
            });
            RegisterCommand("help", _ =>
            {
                foreach (var cmd in commands)
                {
                    WriteLine(cmd.Key);
                }
            });
            RegisterCommand("info", _ =>
            {
                WriteLine($"HexaEngine: v{Assembly.GetExecutingAssembly().GetName().Version}");
            });
            RegisterCommand("gc", _ =>
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForFullGCComplete();
                GC.RefreshMemoryLimit();
            });
#if DEBUG
            RegisterCommand("qqq", _ =>
            {
                throw new Exception("Command qqq was triggered!");
            });
#endif
        }

        /// <summary>
        /// Gets or sets the foreground color used for console text.
        /// </summary>
        public static ConsoleColor ForegroundColor { get => foregroundColor; set => foregroundColor = value; }

        /// <summary>
        /// Gets or sets the background color of the console window.
        /// </summary>
        public static ConsoleColor BackgroundColor { get => backgroundColor; set => backgroundColor = value; }

        private class ConsoleLogWriter : ILogWriter
        {
            public void Clear()
            {
                semaphore.Wait();
                messages.Clear();
                semaphore.Release();
            }

            public void Dispose()
            {
            }

            public void Flush()
            {
            }

            public void Log(LogMessage message)
            {
                if (message.Logger != LoggerFactory.General)
                {
                    return;
                }

                semaphore.Wait();
                messages.Add(message);
                if (messages.Count > maxMessages)
                {
                    messages.Remove(messages[0]);
                }
                semaphore.Release();
                scrollToBottom = true;
            }

            public async Task LogAsync(LogMessage message)
            {
                if (message.Logger != LoggerFactory.General)
                {
                    return;
                }

                await semaphore.WaitAsync();
                messages.Add(message);
                if (messages.Count > maxMessages)
                {
                    messages.Remove(messages[0]);
                }
                semaphore.Release();
                scrollToBottom = true;
            }

            public void Write(string message)
            {
                if (message == null)
                {
                    return;
                }

                semaphore.Wait();
                WriteToConsole(message);
                semaphore.Release();
            }

            public async Task WriteAsync(string message)
            {
                if (message == null)
                {
                    return;
                }

                await semaphore.WaitAsync();
                WriteToConsole(message);
                semaphore.Release();
            }

            private static void WriteToConsole(string message)
            {
                if (messages.Count > 0)
                {
                    if (messages[^1].Message.EndsWith(Environment.NewLine))
                    {
                        messages.Add(new(foregroundColor, backgroundColor, message));
                        scrollToBottom = true;
                    }
                    else
                    {
                        var msg = messages[^1];
                        msg.Message += message;
                        messages[^1] = msg;
                    }
                }
                else
                {
                    messages.Add(new(foregroundColor, backgroundColor, message));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the ImGuiConsole window is displayed.
        /// </summary>
        public static bool IsDisplayed { get => consoleOpen; set => consoleOpen = value; }

        /// <summary>
        /// Restores the default settings for ImGuiConsole, including auto-scrolling, colored output, filtering bar, and timestamps.
        /// </summary>
        public static void DefaultSettings()
        {
            // Settings
            autoScroll = true;
            scrollToBottom = false;
            coloredOutput = true;
            filterBar = true;
            timeStamps = true;

            // Style
            windowAlpha = 1;
        }

        /// <summary>
        /// Registers a custom command with its corresponding callback.
        /// </summary>
        /// <param name="command">The name of the command.</param>
        /// <param name="callback">The callback action to execute when the command is invoked.</param>
        public static void RegisterCommand(string command, Action<string[]> callback)
        {
            commands.Add(command, callback);
            cmdAutocomplete.Add(command, callback);
        }

        /// <summary>
        /// Writes a message to the ImGuiConsole.
        /// </summary>
        /// <param name="message">The message to write to the console.</param>
        public static void Write(string? message)
        {
            semaphore.Wait();
            if (messages.Count > 0)
            {
                if (messages[^1].Message.EndsWith(Environment.NewLine))
                {
                    messages.Add(new(foregroundColor, backgroundColor, message ?? "<null>"));
                    scrollToBottom = true;
                }
                else
                {
                    var msg = messages[^1];
                    msg.Message += message;
                    messages[^1] = msg;
                }
            }
            else
            {
                messages.Add(new(foregroundColor, backgroundColor, message ?? "<null>"));
            }

            if (messages.Count > maxMessages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
            scrollToBottom = true;
        }

        /// <summary>
        /// Writes an object's string representation to the ImGuiConsole.
        /// </summary>
        /// <param name="value">The object to write to the console as a string.</param>
        public static void Write(object? value) => Write(value?.ToString());

        /// <summary>
        /// Writes a message followed by a new line to the ImGuiConsole.
        /// </summary>
        /// <param name="msg">The message to write to the console.</param>
        public static void WriteLine(string? msg)
        {
            semaphore.Wait();
            messages.Add(new(foregroundColor, backgroundColor, $"{msg}{Environment.NewLine}"));
            if (messages.Count > maxMessages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
            scrollToBottom = true;
        }

        /// <summary>
        /// Writes an object's string representation followed by a new line to the ImGuiConsole.
        /// </summary>
        /// <param name="value">The object to write to the console as a string.</param>
        public static void WriteLine(object? value) => WriteLine(value?.ToString());

        /// <summary>
        /// Clears all messages from the ImGuiConsole.
        /// </summary>
        public static void Clear()
        {
            semaphore.Wait();
            messages.Clear();
            semaphore.Release();
        }

        /// <summary>
        /// Draws the ImGuiConsole window, allowing for logging, message display, and command input.
        /// </summary>
        public static void Draw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, windowAlpha);
            if (!ImGui.Begin(consoleName, ref consoleOpen, ImGuiWindowFlags.MenuBar))
            {
                ImGui.PopStyleVar();
                ImGui.End();
                return;
            }
            ImGui.PopStyleVar();

            MenuBar();

            if (filterBar)
            { FilterBar(); }

            LogWindow();

            ImGui.Separator();

            InputBar();

            ImGui.End();
        }

        private static void FilterBar()
        {
            ImGui.InputText("Filter", ref textFilter, (uint)(ImGui.GetWindowWidth() * 0.25f));
            ImGui.Separator();
        }

        private static unsafe void LogWindow()
        {
            float suggestionHeight = 0;

            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();

            if (cmdSuggestions.Count > 0)
            {
                suggestionHeight = ImGui.GetStyle().ChildBorderSize + ImGui.GetStyle().ItemSpacing.Y * 2;
                for (int i = 0; i < cmdSuggestions.Count; i++)
                {
                    suggestionHeight += ImGui.GetStyle().ItemSpacing.Y + ImGui.CalcTextSize(cmdSuggestions[i]).Y;
                }
                footerHeightToReserve += suggestionHeight;
                footerHeightToReserve += ImGui.GetStyle().ItemSpacing.Y;
            }

            if (ImGui.BeginChild("ScrollRegion##", new Vector2(0, -footerHeightToReserve), 0, 0))
            {
                float scrollPos = ImGui.GetScrollY();
                float lineHeight = ImGui.GetTextLineHeightWithSpacing();
                int startLine = (int)(scrollPos / lineHeight);

                float windowHeight = ImGui.GetWindowHeight();
                int visibleLines = (int)MathF.Ceiling(windowHeight / lineHeight);
                int endLine = startLine + visibleLines;

                endLine = Math.Min(endLine, messages.Count);

                float dummyHeight = messages.Count * lineHeight;

                Vector2 cursor = ImGui.GetCursorPos();
                ImGui.Dummy(new(0, dummyHeight));
                ImGui.SetCursorPos(cursor + new Vector2(0, startLine * lineHeight));

                // Display colored command output.
                Vector2 size = default;
                ImGui.CalcTextSize(ref size, "00:00:00:0000");    // Timestamp.
                float timestamp_width = size.X;

                // Display items.
                for (int i = startLine; i < endLine; i++)
                {
                    var item = messages[i];

                    // Exit if word is filtered.
                    if (textFilter.Length != 0 && !item.Message.Contains(textFilter))
                    {
                        continue;
                    }

                    if (timeStamps)
                    {
                        ImGui.PushTextWrapPos(ImGui.GetColumnWidth() - timestamp_width);
                    }

                    // Items.
                    if (coloredOutput)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, consoleColorPalette[item.ForegroundColor]);
                        ImGui.TextUnformatted(item.Message);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextUnformatted(item.Message);
                    }

                    // Time stamp.
                    if (timeStamps)
                    {
                        // No wrap for timestamps
                        ImGui.PopTextWrapPos();

                        // Right align.
                        ImGui.SameLine(ImGui.GetColumnWidth(-1) - timestamp_width);

                        // Draw time stamp.
                        ImGui.PushStyleColor(ImGuiCol.Text, consoleColorPalette[ConsoleColor.Gray]);
                        ImGui.Text(item.Timestamp ?? string.Empty);
                        ImGui.PopStyleColor();
                    }
                }

                // Stop wrapping since we are done displaying console items.
                if (!timeStamps)
                {
                    ImGui.PopTextWrapPos();
                }

                // Auto-scroll logs.
                if (scrollToBottom && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || autoScroll))
                {
                    ImGui.SetScrollHereY(1.0f);

                    scrollToBottom = false;
                }

                // Loop through command string vector.
            }
            ImGui.EndChild();

            if (cmdSuggestions.Count > 0)
            {
                if (ImGui.BeginChild("Suggestions", new(0, suggestionHeight), ImGuiChildFlags.Border))
                {
                    for (int i = 0; i < cmdSuggestions.Count; i++)
                    {
                        ImGui.Text(cmdSuggestions[i]);
                    }

                    ImGui.EndChild();
                }
            }
        }

        private static unsafe void InputBar()
        {
            // Variables.
            ImGuiInputTextFlags inputTextFlags =
                    ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackCharFilter | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.CallbackAlways |
                    ImGuiInputTextFlags.EnterReturnsTrue;

            // Only reclaim after enter key is pressed!
            bool reclaimFocus = false;

            // Input widget. (Width an always fixed width)
            ImGui.PushItemWidth(-ImGui.GetStyle().ItemSpacing.X * 7);
            if (ImGui.InputText("Input", ref buffer, bufferSize, inputTextFlags, inputTextCallback))
            {
                // Validate.
                if (!string.IsNullOrWhiteSpace(buffer))
                {
                    WriteLine("> " + buffer);

                    string[] args = buffer.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    // Run command line input.
                    if (commands.TryGetValue(args[0], out var command))
                    {
                        command(args.Skip(1).ToArray());
                    }
                    else
                    {
                        var old = foregroundColor;
                        foregroundColor = ConsoleColor.Red;
                        WriteLine("command not found");
                        foregroundColor = old;
                    }

                    // Scroll to bottom after its ran.
                    scrollToBottom = true;
                }

                // Keep focus.
                reclaimFocus = true;

                if (history.Contains(buffer))
                {
                    history.Remove(buffer);
                }

                history.Add(buffer);
                historyIndex = history.Count;

                // Clear command line.
                buffer = new(new char[buffer.Length]);

                cmdSuggestions.Clear();
            }
            ImGui.PopItemWidth();

            // Auto-focus on window apparition
            ImGui.SetItemDefaultFocus();
            if (reclaimFocus)
            {
                ImGui.SetKeyboardFocusHere(-1); // Focus on command line after clearing.
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

        private static void MenuBar()
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

                    // Filter bar
                    ImGui.Checkbox("Filter Bar", ref filterBar);
                    ImGui.SameLine();
                    HelpMaker("Enable console filter bar");

                    // Time stamp
                    ImGui.Checkbox("Time Stamps", ref timeStamps);
                    ImGui.SameLine();
                    HelpMaker("Display command execution timestamps");

                    // Reset to default settings
                    if (ImGui.Button("Reset settings", new(ImGui.GetColumnWidth(), 0)))
                    {
                        ImGui.OpenPopup("Reset Settings?");
                    }

                    // Confirmation
                    if (ImGui.BeginPopupModal("Reset Settings?", ref resetModal, ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.Text("All settings will be reset to default.\nThis operation cannot be undone!\n\n");
                        ImGui.Separator();

                        if (ImGui.Button("Reset", new(120, 0)))
                        {
                            DefaultSettings();
                            ImGui.CloseCurrentPopup();
                        }

                        if (ImGui.Button("Clear", new(120, 0)))
                        {
                            messages.Clear();
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel", new(120, 0)))
                        { ImGui.CloseCurrentPopup(); }
                        ImGui.EndPopup();
                    }

                    ImGui.EndMenu();
                }

                // View settings.
                if (ImGui.BeginMenu("Appearance"))
                {
                    // Window transparency.
                    ImGui.TextUnformatted("Background");
                    ImGui.SliderFloat("Transparency##", ref windowAlpha, 0.1f, 1.0f);

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private static unsafe int InputCallback(ImGuiInputTextCallbackData* data)
        {
            // Exit if no buffer.
            if (data->BufTextLen == 0 && data->EventFlag != ImGuiInputTextFlags.CallbackHistory && data->EventFlag != ImGuiInputTextFlags.CallbackAlways)
            {
                return 0;
            }

            // Get input string and console.
            Span<byte> input_str = new(data->Buf, data->BufTextLen);

            int startPos = input_str.IndexOf((byte)' ');
            startPos = startPos == -1 ? 0 : startPos;
            int endPos = input_str.LastIndexOf((byte)' ');
            endPos = endPos == -1 ? input_str.Length : endPos;

            Span<byte> trim_str = new(data->Buf + startPos, endPos);

            Span<byte> buffer = new(data->Buf, data->BufSize);

            switch (data->EventFlag)
            {
                case ImGuiInputTextFlags.CallbackCompletion:
                    {
                        // Find last word.
                        int startSubtrPos = trim_str.LastIndexOf((byte)' ');
                        startSubtrPos = startSubtrPos == -1 ? 0 : startSubtrPos;

                        // Validate str
                        if (trim_str.Length != 0 && cmdSuggestions.Count != 0)
                        {
                            // Get partial completion and suggestions.
                            Span<byte> partial = trim_str[startSubtrPos..];

                            // Autocomplete only when one work is available.
                            if (cmdSuggestions.Count == 1)
                            {
                                buffer[startSubtrPos..data->BufTextLen].Clear();
                                string ne = cmdSuggestions[0];
                                cmdSuggestions.Clear();

                                data->Buf = (byte*)ImGui.MemAlloc((nuint)ne.Length);
                                Encoding.UTF8.GetBytes(ne, new Span<byte>(data->Buf, ne.Length));
                                data->BufTextLen = ne.Length;
                                data->CursorPos = ne.Length;
                                data->BufDirty = 1;
                            }
                            else
                            {
                                int st = 0;
                                int len = cmdSuggestions.Min(x => x.Length);
                                bool dirty = false;
                                for (int i = 0; i < len; i++, st++)
                                {
                                    char c = cmdSuggestions[0][i];
                                    bool exit = false;
                                    for (int j = 1; j < cmdSuggestions.Count; j++)
                                    {
                                        if (c != cmdSuggestions[j][i])
                                        {
                                            exit = true;
                                            break;
                                        }
                                    }

                                    if (exit)
                                    {
                                        break;
                                    }

                                    buffer[i] = (byte)c;
                                    dirty = true;
                                }

                                if (dirty)
                                {
                                    data->BufTextLen = st;
                                    data->CursorPos = st;
                                    data->BufDirty = 1;
                                    trim_str = new(data->Buf + startPos, st);
                                    UpdateSuggestions(trim_str);
                                }
                            }
                        }
                    }
                    break;

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
                            if (historyIndex + 1 < history.Count)
                            {
                                historyIndex++;
                            }
                        }

                        // Get history.
                        string prevCommand = history[historyIndex];

                        // Insert commands.
                        for (int i = 0; i < prevCommand.Length; i++)
                        {
                            buffer[i] = (byte)prevCommand[i];
                        }

                        data->BufTextLen = prevCommand.Length;
                        data->CursorPos = prevCommand.Length;
                        data->BufDirty = 1;
                    }
                    break;

                case ImGuiInputTextFlags.CallbackAlways:
                    {
                        UpdateSuggestions(trim_str);
                    }

                    break;

                case ImGuiInputTextFlags.CallbackCharFilter:

                default:
                    break;
            }
            return 1;
        }

        private static void UpdateSuggestions(Span<byte> trim_str)
        {
            // Find last word.
            int startSubtrPos = trim_str.LastIndexOf((byte)' ');
            startSubtrPos = startSubtrPos == -1 ? 0 : startSubtrPos;

            cmdSuggestions.Clear();

            // Validate str
            if (trim_str.Length != 0)
            {
                // Get partial completion and suggestions.
                string partial = Encoding.UTF8.GetString(trim_str[startSubtrPos..]);
                cmdSuggestions.AddRange(cmdAutocomplete.StartingWith(partial).Select(x => x.Key));
            }
        }
    }
}