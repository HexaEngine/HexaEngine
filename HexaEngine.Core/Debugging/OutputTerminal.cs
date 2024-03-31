namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents an output terminal for displaying messages.
    /// </summary>
    public class OutputTerminal : TerminalBase
    {
        private readonly TerminalTraceListener traceListener;
        private readonly TerminalConsoleRedirect consoleRedirect;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputTerminal"/> class.
        /// </summary>
        public OutputTerminal()
        {
            traceListener = new(this);
            Trace.Listeners.Add(traceListener);
            consoleRedirect = new(this);
            Console.SetOut(consoleRedirect);
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
                if (outputTerminal.Messages.Count != 0 && !outputTerminal.Messages[^1].Message.EndsWith(Environment.NewLine))
                {
                    var msg = outputTerminal.Messages[^1];
                    msg.Message += lines[0];
                    outputTerminal.SetMessage(outputTerminal.Messages.Count - 1, msg);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "\f")
                        {
                            outputTerminal.Clear();
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
                            outputTerminal.Clear();
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

        /// <summary>
        /// Writes the specified text to the output terminal.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(string text)
        {
            AddMessage(text);
        }

        /// <summary>
        /// Writes the specified text followed by a new line to the output terminal.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteLine(string text)
        {
            Write(text + Environment.NewLine);
        }
    }
}