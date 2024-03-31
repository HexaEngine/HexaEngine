namespace HexaEngine.Core.Debugging
{
    using Hexa.NET.ImGui;
    using System;

    /// <summary>
    /// Represents a shell terminal.
    /// </summary>
    public sealed class ShellTerminal : TerminalBase
    {
        private readonly List<string> history = new();
        private int historyIndex;

        private readonly ConsoleAppManager consoleAppManager;

        private readonly ImGuiInputTextCallback textCallback;

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

        protected override void Input(string text)
        {
            consoleAppManager.Write(text + Environment.NewLine);
            history.Add(text);
            historyIndex = history.Count;
        }

        private void ErrorTextReceived(object? sender, string e)
        {
            ProcessReceivedEvent(e, TerminalColor.Red);
        }

        private void StandardTextReceived(object? sender, string e)
        {
            ProcessReceivedEvent(e, TerminalColor.White);
        }

        private void ProcessReceivedEvent(string e, TerminalColor color)
        {
            var messages = Messages;
            lock (messages)
            {
                string[] lines = e.Split(Environment.NewLine);
                if (messages.Count != 0 && !messages[^1].Message.EndsWith(Environment.NewLine))
                {
                    var msg = messages[^1];
                    msg.Message += lines[0];
                    msg.Color = color;
                    SetMessage(messages.Count - 1, msg);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            Clear();
                        }
                        else if (i > 0)
                        {
                            AddMessage(lines[i], color);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            Clear();
                        }
                        else
                        {
                            AddMessage(lines[i], color);
                        }
                    }
                }

                ScrollToBottom();
            }
        }

        protected override unsafe int InputCallback(ImGuiInputTextCallbackData* data)
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
    }
}