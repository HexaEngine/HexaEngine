// Copyright (c) 2020 - present, Roland Munguia
// Distributed under the MIT License (http://opensource.org/licenses/MIT)

// Modified and ported by me.

namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Collections;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

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
        private static bool wasPrevFrameTabCompletion;
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
        private static readonly string m_ConsoleName = "Console";
        private static bool consoleOpen;
        private static ConsoleColor foregroundColor;
        private static ConsoleColor backgroundColor;
        private static readonly SemaphoreSlim semaphore = new(1);
        private static readonly unsafe ImGuiInputTextCallback inputTextCallback = InputCallback;
        private const int maxMessages = 4096;

        public static void Initialize()
        {
            Logger.Writers.Add(logListener);

            DefaultSettings();

            RegisterCommand("clear", _ =>
            {
                messages.Clear();
            });
            RegisterCommand("info", _ =>
            {
                WriteLine($"HexaEngine: v{Assembly.GetExecutingAssembly().GetName().Version}");
            });
            RegisterCommand("qqq", _ =>
            {
                throw new Exception("Command qqq was triggered!");
            });
        }

        public static ConsoleColor ForegroundColor { get => foregroundColor; set => foregroundColor = value; }

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
                        messages.Add(new(foregroundColor, message));
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
                    messages.Add(new(foregroundColor, message));
                }
            }
        }

        public static bool IsDisplayed { get => consoleOpen; set => consoleOpen = value; }

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

        public static void RegisterCommand(string command, Action<string[]> callback)
        {
            commands.Add(command, callback);
            cmdAutocomplete.Add(command, callback);
        }

        public static void Write(string? message)
        {
            semaphore.Wait();
            if (messages.Count > 0)
            {
                if (messages[^1].Message.EndsWith(Environment.NewLine))
                {
                    messages.Add(new(foregroundColor, message ?? "<null>"));
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
                messages.Add(new(foregroundColor, message ?? "<null>"));
            }

            if (messages.Count > maxMessages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
            scrollToBottom = true;
        }

        public static void Write(object? value) => Write(value?.ToString());

        public static void WriteLine(string? msg)
        {
            semaphore.Wait();
            messages.Add(new(foregroundColor, $"{msg}{Environment.NewLine}"));
            if (messages.Count > maxMessages)
            {
                messages.Remove(messages[0]);
            }
            semaphore.Release();
            scrollToBottom = true;
        }

        public static void WriteLine(object? value) => WriteLine(value?.ToString());

        public static void Draw()
        {
            //semaphore.Wait();
            ///////////////////////////////////////////////////////////////////////////
            // Window and Settings ////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////

            // Begin Console Window.
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, windowAlpha);
            if (!ImGui.Begin(m_ConsoleName, ref consoleOpen, ImGuiWindowFlags.MenuBar))
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
            if (filterBar)
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
            ImGui.InputText("Filter", ref textFilter, (uint)(ImGui.GetWindowWidth() * 0.25f));
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

                // Display items.
                for (int i = 0; i < messages.Count; i++)
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
                        ImGui.Text(item.Timestamp);
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
            if (ImGui.InputText("Input", ref buffer, bufferSize, inputTextFlags, inputTextCallback))
            {
                // Validate.
                if (!string.IsNullOrWhiteSpace(buffer))
                {
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

                // Clear command line.
                buffer = new(new char[buffer.Length]);
            }
            ImGui.PopItemWidth();

            // Reset suggestions when client provides char input.
            if (ImGui.IsItemEdited() && !wasPrevFrameTabCompletion)
            {
                cmdSuggestions.Clear();
            }
            wasPrevFrameTabCompletion = false;

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
            if (data->BufTextLen == 0 && data->EventFlag != ImGuiInputTextFlags.CallbackHistory)
            {
                return 0;
            }

            // Get input string and console.
            string input_str = Encoding.UTF8.GetString(data->Buf, data->BufTextLen);
            string trim_str = input_str.Trim();

            int startPos = ImGuiConsole.buffer.IndexOf(' ');
            startPos = startPos == -1 ? 0 : startPos;
            int endPos = ImGuiConsole.buffer.LastIndexOf(' ');
            endPos = endPos == -1 ? ImGuiConsole.buffer.Length : endPos;

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
                            if (!(cmdSuggestions.Count == 0))
                            {
                                var old = foregroundColor;
                                foregroundColor = ConsoleColor.Gray;
                                WriteLine("Suggestions: ");
                                foreach (var suggestion in cmdSuggestions)
                                {
                                    WriteLine(suggestion);
                                }
                                foregroundColor = old;

                                cmdSuggestions.Clear();
                            }

                            // Get partial completion and suggestions.
                            string partial = trim_str.Substring(startSubtrPos, endPos);
                            cmdSuggestions.AddRange(cmdAutocomplete.StartingWith(partial).Select(x => x.Key));

                            // Autocomplete only when one work is available.
                            if (!(cmdSuggestions.Count == 0) && cmdSuggestions.Count == 1)
                            {
                                buffer[startSubtrPos..data->BufTextLen].Clear();
                                string ne = cmdSuggestions[0];
                                cmdSuggestions.Clear();
                                data->Buf = (byte*)Marshal.StringToHGlobalAnsi(ne);
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
                                    buffer[startSubtrPos..data->BufTextLen].Clear();
                                    partial.CopyTo(buffer[startSubtrPos..]);
                                    data->BufDirty = 1;
                                }
                            }
                        }

                        // We have performed the completion event.
                        wasPrevFrameTabCompletion = true;
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
                                --historyIndex;
                            }
                        }
                        else
                        {
                            if (historyIndex < history.Count)
                            {
                                ++historyIndex;
                            }
                        }

                        // Get history.
                        string prevCommand = history[historyIndex];

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