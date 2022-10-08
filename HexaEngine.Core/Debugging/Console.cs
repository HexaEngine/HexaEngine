﻿namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Debugging.Collections;
    using ImGuiNET;
    using KeraLua;
    using Kitty.Core.Scripting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public class ConsoleColorPalette
    {
        private readonly Vector4[] values;

        public ConsoleColorPalette()
        {
            values = new Vector4[Enum.GetValues<LogSeverity>().Length];
            this[LogSeverity.Command] = new(1.0f, 1.0f, 1.0f, 1.0f);
            this[LogSeverity.Log] = new(1.0f, 1.0f, 1.0f, 0.5f);
            this[LogSeverity.Warning] = new(1.0f, 0.87f, 0.37f, 1.0f);
            this[LogSeverity.Error] = new(1.0f, 0.365f, 0.365f, 1.0f);
            this[LogSeverity.Info] = new(0.46f, 0.96f, 0.46f, 1.0f);
            this[LogSeverity.Timestamp] = new(1.0f, 1.0f, 1.0f, 0.5f);
        }

        public ref Vector4 this[LogSeverity index]
        {
            get { return ref values[(int)index]; }
        }
    }

    public static class ImGuiConsole
    {
        private static readonly Dictionary<IScript, Task> tasks = new();
        private static readonly List<LogMessage> messages = new();
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
                    return;
                if (messages.Count > 0)
                {
                    if (messages[^1].Message.EndsWith(Environment.NewLine))
                    {
                        messages.Add(new() { Severity = LogSeverity.Log, Message = message, Timestamp = DateTime.Now });
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
                    messages.Add(new() { Severity = LogSeverity.Log, Message = message, Timestamp = DateTime.Now });
            }

            public override void WriteLine(string? message)
            {
                if (message == null)
                    return;
                messages.Add(new() { Severity = LogSeverity.Log, Message = message, Timestamp = DateTime.Now });
                m_ScrollToBottom = true;
            }
        }

        public static bool IsDisplayed { get => m_consoleOpen; set => m_consoleOpen = value; }

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
            consoleColorPalette[LogSeverity.Command] = new(1.0f, 1.0f, 1.0f, 1.0f);
            consoleColorPalette[LogSeverity.Log] = new(1.0f, 1.0f, 1.0f, 0.5f);
            consoleColorPalette[LogSeverity.Warning] = new(1.0f, 0.87f, 0.37f, 1.0f);
            consoleColorPalette[LogSeverity.Error] = new(1.0f, 0.365f, 0.365f, 1.0f);
            consoleColorPalette[LogSeverity.Info] = new(0.46f, 0.96f, 0.46f, 1.0f);
            consoleColorPalette[LogSeverity.Timestamp] = new(1.0f, 1.0f, 1.0f, 0.5f);
        }

        public static void RegisterCommand(string command, Action<string[]> callback)
        {
            commands.Add(command, callback);
            cmdAutocomplete.Add(command, callback);
        }

        public static void Log(LogSeverity type, string msg)
        {
            messages.Add(new LogMessage() { Severity = type, Message = msg, Timestamp = DateTime.Now });
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
        }

        public static void Log(Exception e)
        {
            messages.Add(new LogMessage() { Severity = LogSeverity.Error, Message = e.ToString(), Timestamp = DateTime.Now });
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
        }

        public static async Task LogAsync(LogSeverity type, string msg)
        {
            await semaphore.WaitAsync();
            messages.Add(new LogMessage() { Severity = type, Message = msg, Timestamp = DateTime.Now });
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
        }

        public static void Log(string msg)
        {
            LogSeverity type = LogSeverity.Log;
            if (msg.Contains("error", StringComparison.CurrentCultureIgnoreCase))
                type = LogSeverity.Error;
            if (msg.Contains("warn", StringComparison.CurrentCultureIgnoreCase))
                type = LogSeverity.Warning;
            if (msg.Contains("warning", StringComparison.CurrentCultureIgnoreCase))
                type = LogSeverity.Warning;
            messages.Add(new LogMessage() { Severity = type, Message = msg, Timestamp = DateTime.Now });
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
        }

        public static async Task LogAsync(string msg)
        {
            await semaphore.WaitAsync();
            LogSeverity type = LogSeverity.Log;
            if (msg.Contains("error", StringComparison.CurrentCultureIgnoreCase))
                type = LogSeverity.Error;
            if (msg.Contains("warn", StringComparison.CurrentCultureIgnoreCase))
                type = LogSeverity.Warning;
            if (msg.Contains("warning", StringComparison.CurrentCultureIgnoreCase))
                type = LogSeverity.Warning;
            messages.Add(new LogMessage() { Severity = type, Message = msg, Timestamp = DateTime.Now });
            if (messages.Count > max_messages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
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

        private static void LogWindow()
        {
            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            if (ImGui.BeginChild("ScrollRegion##", new Vector2(0, -footerHeightToReserve), false, 0))
            {
                // Display colored command output.
                float timestamp_width = ImGui.CalcTextSize("00:00:00:0000").X;    // Timestamp.
                int count = 0;                                                                       // Item count.

                // Wrap items.

                // Display items.
                for (int i = 0; i < messages.Count; i++)
                {
                    var item = messages[i];

                    // Exit if word is filtered.
                    if (m_TextFilter.Length != 0 && !m_TextFilter.Contains(item.Message))
                        continue;

                    if (m_TimeStamps)
                        ImGui.PushTextWrapPos(ImGui.GetColumnWidth() - timestamp_width);

                    // Spacing between commands.
                    if (item.Severity == LogSeverity.Command)
                    {
                        // Wrap before timestamps start.
                        if (count++ != 0) ImGui.Dummy(new(-1, ImGui.GetFontSize()));                            // No space for the first command.
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
                        ImGui.PushStyleColor(ImGuiCol.Text, consoleColorPalette[LogSeverity.Timestamp]);
                        ImGui.Text(item.Timestamp.ToShortTimeString());
                        ImGui.PopStyleColor();
                    }
                }

                // Stop wrapping since we are done displaying console items.
                if (!m_TimeStamps)
                    ImGui.PopTextWrapPos();

                // Auto-scroll logs.
                if (m_ScrollToBottom && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || m_AutoScroll))
                    ImGui.SetScrollHereY(1.0f);
                m_ScrollToBottom = false;

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
            if (ImGui.InputText("Input", ref m_Buffer, m_Buffer_size, inputTextFlags, InputCallback))
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
                ImGui.SetKeyboardFocusHere(-1); // Focus on command line after clearing.
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
                        ImGui.OpenPopup("Reset Settings?");

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
                    ImGui.ColorEdit4("Command##", ref consoleColorPalette[LogSeverity.Command], flags);
                    ImGui.ColorEdit4("Log##", ref consoleColorPalette[LogSeverity.Log], flags);
                    ImGui.ColorEdit4("Warning##", ref consoleColorPalette[LogSeverity.Warning], flags);
                    ImGui.ColorEdit4("Error##", ref consoleColorPalette[LogSeverity.Error], flags);
                    ImGui.ColorEdit4("Info##", ref consoleColorPalette[LogSeverity.Info], flags);
                    ImGui.ColorEdit4("Time Stamp##", ref consoleColorPalette[LogSeverity.Timestamp], flags);
                    ImGui.Unindent();

                    ImGui.Separator();

                    // Window transparency.
                    ImGui.TextUnformatted("Background");
                    ImGui.SliderFloat("Transparency##", ref m_WindowAlpha, 0.1f, 1.0f);

                    ImGui.EndMenu();
                }

                // TODO: Reimplement console scripts.
                // All scripts.

                if (ImGui.BeginMenu("Scripts"))
                {
                    // Show registered scripts.
                    foreach (var script in ScriptManager.Scripts)
                    {
                        if (!tasks.ContainsKey(script))
                        {
                            if (ImGui.MenuItem(script.Name))
                            {
                                tasks.Add(script, script.RunAsync().ContinueWith(x => tasks.Remove(script)));
                            }
                        }
                        else
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.98f, 0.26f, 0.95f, 0.40f));
                            if (ImGui.MenuItem($"> {script.Name}"))
                            {
                                //tasks[script].St
                            }
                            ImGui.PopStyleColor();
                        }
                    }

                    // Reload scripts.
                    ImGui.Separator();
                    if (ImGui.Button("Reload Scripts", new(ImGui.GetColumnWidth(), 0)))
                    {
                        ScriptManager.Reload();
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private static unsafe int InputCallback(ImGuiInputTextCallbackData* data)
        {
            // Exit if no buffer.
            if (data->BufTextLen == 0 && data->EventFlag != ImGuiInputTextFlags.CallbackHistory)
                return 0;

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
                                Log(LogSeverity.Command, "Suggestions: ");
                                foreach (var suggestion in m_CmdSuggestions)
                                    Log(LogSeverity.Log, suggestion);

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
                            if (m_HistoryIndex > 0) --m_HistoryIndex;
                        }
                        else
                        {
                            if (m_HistoryIndex < history.Count) ++m_HistoryIndex;
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