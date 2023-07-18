namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Collections;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class ImGuiConsole
    {
        private static readonly List<ConsoleMessage> messages = new();
        private static readonly List<string> history = new();
        private static readonly ConsoleTraceListener traceListener;
        private static readonly Dictionary<string, Action<string[]>> commands = new();
        private static bool m_TimeStamps;
        private static bool m_ColoredOutput;
        private static bool m_ScrollToBottom;
        private static bool m_AutoScroll;
        private static bool m_WasPrevFrameTabCompletion;
        private static string m_TextFilter = string.Empty;
        private static string m_Buffer = "";
        private static readonly uint m_Buffer_size = 256;
        private static readonly List<string> m_CmdSuggestions = new();
        private static int m_HistoryIndex;
        private static readonly TernarySearchTreeDictionary<Action<string[]>> cmdAutocomplete = new();
        private static readonly TernarySearchTreeDictionary<string> varAutocomplete = new();
        private static bool m_FilterBar;
        private static bool m_resetModal;
        private static float m_WindowAlpha = 1;
        private static readonly ConsoleColorPalette consoleColorPalette = new();
        private static readonly string m_ConsoleName = "Console";
        private static bool m_consoleOpen;
        private static readonly SemaphoreSlim semaphore = new(1);
        private const int max_messages = 4096;

        static ImGuiConsole()
        {
            traceListener = new();
            Trace.Listeners.Add(traceListener);
            DefaultSettings();

            RegisterCommand("clear", _ =>
            {
                messages.Clear();
            });
        }

        private class ConsoleTraceListener : TraceListener
        {
            public override void Write(string? message)
            {
                if (message == null)
                {
                    return;
                }

                semaphore.Wait();
                if (messages.Count > 0)
                {
                    if (messages[^1].Message.EndsWith(Environment.NewLine))
                    {
                        messages.Add(new() { Severity = ConsoleColor.Trace, Message = message, Timestamp = DateTime.Now.ToShortTimeString() });
                        m_ScrollToBottom = true;
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
                    messages.Add(new() { Severity = ConsoleColor.Trace, Message = message, Timestamp = DateTime.Now.ToShortTimeString() });
                }
                semaphore.Release();
            }

            public override void WriteLine(string? message)
            {
                if (message == null)
                {
                    return;
                }

                semaphore.Wait();
                messages.Add(new() { Severity = ConsoleColor.Trace, Message = message, Timestamp = DateTime.Now.ToShortTimeString() });
                semaphore.Release();
                m_ScrollToBottom = true;
            }
        }

        public static bool IsDisplayed { get => m_consoleOpen; set => m_consoleOpen = value; }
        public static bool Redirect { get; set; }

        public static void DefaultSettings()
        {
            // Settings
            m_AutoScroll = true;
            m_ScrollToBottom = false;
            m_ColoredOutput = true;
            m_FilterBar = true;
            m_TimeStamps = true;

            // Style
            m_WindowAlpha = 1;
            consoleColorPalette[ConsoleColor.Command] = new(1.0f, 1.0f, 1.0f, 1.0f);
            consoleColorPalette[ConsoleColor.Timestamp] = new(1.0f, 1.0f, 1.0f, 0.5f);
            consoleColorPalette[ConsoleColor.Trace] = new(0.46f, 0.96f, 0.46f, 1.0f);
            consoleColorPalette[ConsoleColor.Debug] = new(0.46f, 0.96f, 0.46f, 1.0f);
            consoleColorPalette[ConsoleColor.Information] = new(1.0f, 1.0f, 1.0f, 1.0f);
            consoleColorPalette[ConsoleColor.Warning] = new(1.0f, 0.87f, 0.37f, 1.0f);
            consoleColorPalette[ConsoleColor.Error] = new(1.0f, 0.365f, 0.365f, 1.0f);
            consoleColorPalette[ConsoleColor.Critical] = new(1.0f, 0.0f, 0.0f, 1.0f);
        }

        public static void RegisterCommand(string command, Action<string[]> callback)
        {
            commands.Add(command, callback);
            cmdAutocomplete.Add(command, callback);
        }

        public static Task HandleError(Task task)
        {
            if (!task.IsCompletedSuccessfully && task.Exception != null)
            {
                Log(task.Exception);
            }
            task.Dispose();
            m_ScrollToBottom = true;
            return Task.CompletedTask;
        }

        public static void Log(LogSeverity type, string message)
        {
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            var msg = new LogMessage(type, message);
            semaphore.Wait();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static void Log(Exception? e)
        {
            var message = e?.ToString() ?? "null";
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            var msg = new LogMessage(LogSeverity.Error, message);
            semaphore.Wait();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        private static LogSeverity Evaluate(string message)
        {
            LogSeverity type = LogSeverity.Information;
            if (message.Contains("critical", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Critical;
            }
            else if (message.Contains("error", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Error;
            }
            else if (message.Contains("warn", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Warning;
            }
            else if (message.Contains("warning", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Warning;
            }
            else if (message.Contains("info", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Information;
            }
            else if (message.Contains("information", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Information;
            }
            else if (message.Contains("debug", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Debug;
            }
            else if (message.Contains("dbg", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Debug;
            }
            else if (message.Contains("trace", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Trace;
            }

            return type;
        }

        public static void Log(object? value)
        {
            var message = value?.ToString() ?? "null";
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            LogSeverity type = Evaluate(message);

            var msg = new LogMessage(type, message);
            semaphore.Wait();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static void Log(string message)
        {
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            LogSeverity type = Evaluate(message);
            var msg = new LogMessage(type, message);
            semaphore.Wait();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static async Task LogAsync(LogSeverity type, string message)
        {
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            var msg = new LogMessage(type, message);
            await semaphore.WaitAsync();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static async Task LogAsync(Exception? e)
        {
            var message = e?.ToString() ?? "null";
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            var msg = new LogMessage(LogSeverity.Error, message);
            await semaphore.WaitAsync();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static async Task LogAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            LogSeverity type = Evaluate(message);

            var msg = new LogMessage(type, message);
            await semaphore.WaitAsync();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static async Task LogAsync(string message)
        {
            if (Redirect)
            {
                Debug.WriteLine(message);
            }

            LogSeverity type = Evaluate(message);

            var msg = new LogMessage(type, message);
            await semaphore.WaitAsync();
            messages.Add(msg);
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            Logger.DebugListener.WriteLine(message);
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static void WriteLine(string msg)
        {
            if (Redirect)
            {
                Debug.WriteLine(msg);
            }

            semaphore.Wait();
            messages.Add(new LogMessage() { Severity = LogSeverity.Information, Message = $"{msg}{Environment.NewLine}", Timestamp = DateTime.Now.ToShortTimeString() });
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static void WriteLine(object msg)
        {
            if (Redirect)
            {
                Debug.WriteLine(msg);
            }

            semaphore.Wait();
            messages.Add(new LogMessage() { Severity = LogSeverity.Information, Message = $"{msg}{Environment.NewLine}", Timestamp = DateTime.Now.ToShortTimeString() });
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
            m_ScrollToBottom = true;
        }

        public static void Draw()
        {
            //semaphore.Wait();
            ///////////////////////////////////////////////////////////////////////////
            // Window and Settings ////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////

            // Begin Console Window.
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, m_WindowAlpha);
            if (!ImGui.Begin(m_ConsoleName, ref m_consoleOpen, ImGuiWindowFlags.MenuBar))
            {
                ImGui.PopStyleVar();
                ImGui.End();
                return;
            }
            ImGui.PopStyleVar();

            ///////////////
            // Menu bar  //
            ///////////////
            MenuBar();

            ////////////////
            // Filter bar //
            ////////////////
            if (m_FilterBar)
            { FilterBar(); }

            //////////////////
            // Console Logs //
            //////////////////
            LogWindow();

            // Section off.
            ImGui.Separator();

            ///////////////////////////////////////////////////////////////////////////
            // Command-line ///////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////

            InputBar();

            ImGui.End();
            //semaphore.Release();
        }

        private static void FilterBar()
        {
            ImGui.InputText("Filter", ref m_TextFilter, (uint)(ImGui.GetWindowWidth() * 0.25f));
            ImGui.Separator();
        }

        private static unsafe void LogWindow()
        {
            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            if (ImGui.BeginChild("ScrollRegion##", new Vector2(0, -footerHeightToReserve), false, 0))
            {
                // Display colored command output.
                Vector2 size = default;
                ImGui.CalcTextSize(ref size, "00:00:00:0000");    // Timestamp.
                float timestamp_width = size.X;
                int count = 0;                                                                       // Item count.

                // Wrap items.

                // Display items.
                for (int i = 0; i < messages.Count; i++)
                {
                    var item = messages[i];

                    // Exit if word is filtered.
                    if (m_TextFilter.Length != 0 && !item.Message.Contains(m_TextFilter))
                    {
                        continue;
                    }

                    if (m_TimeStamps)
                    {
                        ImGui.PushTextWrapPos(ImGui.GetColumnWidth() - timestamp_width);
                    }

                    // Spacing between commands.
                    if (item.Severity == ConsoleColor.Command)
                    {
                        // Wrap before timestamps start.
                        if (count++ != 0)
                        {
                            ImGui.Dummy(new(-1, ImGui.GetFontSize()));                            // No space for the first command.
                        }
                    }

                    // Items.
                    if (m_ColoredOutput)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, consoleColorPalette[item.Severity]);
                        ImGui.TextUnformatted(item.Message);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextUnformatted(item.Message);
                    }

                    // Time stamp.
                    if (m_TimeStamps)
                    {
                        // No wrap for timestamps
                        ImGui.PopTextWrapPos();

                        // Right align.
                        ImGui.SameLine(ImGui.GetColumnWidth(-1) - timestamp_width);

                        // Draw time stamp.
                        ImGui.PushStyleColor(ImGuiCol.Text, consoleColorPalette[ConsoleColor.Timestamp]);
                        ImGui.Text(item.Timestamp);
                        ImGui.PopStyleColor();
                    }
                }

                // Stop wrapping since we are done displaying console items.
                if (!m_TimeStamps)
                {
                    ImGui.PopTextWrapPos();
                }

                // Auto-scroll logs.
                if (m_ScrollToBottom && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || m_AutoScroll))
                {
                    ImGui.SetScrollHereY(1.0f);

                    m_ScrollToBottom = false;
                }

                // Loop through command string vector.
            }
            ImGui.EndChild();
        }

        private static unsafe void InputBar()
        {
            // Variables.
            ImGuiInputTextFlags inputTextFlags =
                    ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackCharFilter | ImGuiInputTextFlags.CallbackCompletion |
                    ImGuiInputTextFlags.EnterReturnsTrue;

            // Only reclaim after enter key is pressed!
            bool reclaimFocus = false;

            // Input widget. (Width an always fixed width)
            ImGui.PushItemWidth(-ImGui.GetStyle().ItemSpacing.X * 7);
            if (ImGui.InputText("Input", ref m_Buffer, m_Buffer_size, inputTextFlags))
            {
                // Validate.
                if (!string.IsNullOrWhiteSpace(m_Buffer))
                {
                    string[] args = m_Buffer.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    // Run command line input.
                    if (commands.TryGetValue(args[0], out var command))
                    {
                        command(args.Skip(1).ToArray());
                    }
                    else
                    {
                        Log(LogSeverity.Error, "command not found");
                    }

                    // Scroll to bottom after its ran.
                    m_ScrollToBottom = true;
                }

                // Keep focus.
                reclaimFocus = true;

                // Clear command line.
                m_Buffer = new(new char[m_Buffer.Length]);
            }
            ImGui.PopItemWidth();

            // Reset suggestions when client provides char input.
            if (ImGui.IsItemEdited() && !m_WasPrevFrameTabCompletion)
            {
                m_CmdSuggestions.Clear();
            }
            m_WasPrevFrameTabCompletion = false;

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
                    ImGui.Checkbox("Colored Output", ref m_ColoredOutput);
                    ImGui.SameLine();
                    HelpMaker("Enable colored command output");

                    // Auto Scroll
                    ImGui.Checkbox("Auto Scroll", ref m_AutoScroll);
                    ImGui.SameLine();
                    HelpMaker("Automatically scroll to bottom of console log");

                    // Filter bar
                    ImGui.Checkbox("Filter Bar", ref m_FilterBar);
                    ImGui.SameLine();
                    HelpMaker("Enable console filter bar");

                    // Time stamp
                    ImGui.Checkbox("Time Stamps", ref m_TimeStamps);
                    ImGui.SameLine();
                    HelpMaker("Display command execution timestamps");

                    // Reset to default settings
                    if (ImGui.Button("Reset settings", new(ImGui.GetColumnWidth(), 0)))
                    {
                        ImGui.OpenPopup("Reset Settings?");
                    }

                    // Confirmation
                    if (ImGui.BeginPopupModal("Reset Settings?", ref m_resetModal, ImGuiWindowFlags.AlwaysAutoResize))
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
                    // Logging Colors
                    ImGuiColorEditFlags flags =
                            ImGuiColorEditFlags.Float | ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar;

                    ImGui.TextUnformatted("Color Palette");
                    ImGui.Indent();
                    ImGui.ColorEdit4("Command##", ref consoleColorPalette[ConsoleColor.Command], flags);
                    ImGui.ColorEdit4("Time Stamp##", ref consoleColorPalette[ConsoleColor.Timestamp], flags);

                    ImGui.ColorEdit4("Trace##", ref consoleColorPalette[ConsoleColor.Trace], flags);
                    ImGui.ColorEdit4("Debug##", ref consoleColorPalette[ConsoleColor.Debug], flags);
                    ImGui.ColorEdit4("Information##", ref consoleColorPalette[ConsoleColor.Information], flags);
                    ImGui.ColorEdit4("Warning##", ref consoleColorPalette[ConsoleColor.Warning], flags);
                    ImGui.ColorEdit4("Error##", ref consoleColorPalette[ConsoleColor.Error], flags);
                    ImGui.ColorEdit4("Critical##", ref consoleColorPalette[ConsoleColor.Critical], flags);
                    ImGui.Unindent();

                    ImGui.Separator();

                    // Window transparency.
                    ImGui.TextUnformatted("Background");
                    ImGui.SliderFloat("Transparency##", ref m_WindowAlpha, 0.1f, 1.0f);

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private static unsafe int InputCallback(ImGuiInputTextCallbackData* data)
        {
            // Exit if no buffer.
            if (data->BufTextLen == 0 && data->EventFlag != ImGuiInputTextFlags.CallbackHistory)
            {
                return 0;
            }

            // Get input string and console.
            string input_str = Encoding.UTF8.GetString(data->Buf, data->BufTextLen);
            string trim_str = input_str.Trim();

            int startPos = m_Buffer.IndexOf(' ');
            startPos = startPos == -1 ? 0 : startPos;
            int endPos = m_Buffer.LastIndexOf(' ');
            endPos = endPos == -1 ? m_Buffer.Length : endPos;

            Span<char> buffer = new(data->Buf, data->BufSize);

            switch (data->EventFlag)
            {
                case ImGuiInputTextFlags.CallbackCompletion:
                    {
                        // Find last word.
                        int startSubtrPos = trim_str.LastIndexOf(' ');
                        startSubtrPos = startSubtrPos == -1 ? 0 : startSubtrPos;

                        // Validate str
                        if (!string.IsNullOrEmpty(trim_str))
                        {
                            // Display suggestions on console.
                            if (!(m_CmdSuggestions.Count == 0))
                            {
                                Log(LogSeverity.Trace, "Suggestions: ");
                                foreach (var suggestion in m_CmdSuggestions)
                                {
                                    Log(LogSeverity.Trace, suggestion);
                                }

                                m_CmdSuggestions.Clear();
                            }

                            // Get partial completion and suggestions.
                            string partial = trim_str.Substring(startSubtrPos, endPos);
                            m_CmdSuggestions.AddRange(cmdAutocomplete.StartingWith(partial).Select(x => x.Key));

                            // Autocomplete only when one work is available.
                            if (!(m_CmdSuggestions.Count == 0) && m_CmdSuggestions.Count == 1)
                            {
                                buffer[startSubtrPos..data->BufTextLen].Fill((char)0);
                                string ne = m_CmdSuggestions[0];
                                m_CmdSuggestions.Clear();
                                data->Buf = (byte*)Marshal.StringToCoTaskMemUTF8(ne).ToPointer();
                                data->BufTextLen = ne.Length;
                                data->CursorPos = ne.Length;
                                data->BufDirty = 1;
                            }
                            else
                            {
                                // Partially complete word.
                                if (!string.IsNullOrEmpty(partial))
                                {
                                    int newLen = data->BufTextLen - startSubtrPos;
                                    buffer[startSubtrPos..data->BufTextLen].Fill((char)0);
                                    partial.CopyTo(buffer[startSubtrPos..]);
                                    data->BufDirty = 1;
                                }
                            }
                        }

                        // We have performed the completion event.
                        m_WasPrevFrameTabCompletion = true;
                    }
                    break;

                case ImGuiInputTextFlags.CallbackHistory:
                    {
                        // Clear buffer.
                        data->BufTextLen = 0;

                        // Traverse history.
                        if (data->EventKey == ImGuiKey.UpArrow)
                        {
                            if (m_HistoryIndex > 0)
                            {
                                --m_HistoryIndex;
                            }
                        }
                        else
                        {
                            if (m_HistoryIndex < history.Count)
                            {
                                ++m_HistoryIndex;
                            }
                        }

                        // Get history.
                        string prevCommand = history[m_HistoryIndex];

                        // Insert commands.
                        Unsafe.Copy(data->Buf, ref prevCommand);
                        data->BufTextLen = prevCommand.Length;
                    }
                    break;

                case ImGuiInputTextFlags.CallbackCharFilter:
                case ImGuiInputTextFlags.CallbackAlways:
                default:
                    break;
            }
            return 1;
        }
    }
}