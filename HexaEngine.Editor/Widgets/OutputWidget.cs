namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Logging;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public class LogQueue
    {
        private readonly ILogger logger;
        private readonly int maxMessages;
        private readonly ConsoleMessage[] messages;
        private int head;
        private int tail;
        private int count;
        private readonly SemaphoreSlim semaphore = new(1);

        public LogQueue(ILogger logger, int maxMessages = 8192)
        {
            this.logger = logger;
            this.maxMessages = maxMessages;
            messages = new ConsoleMessage[maxMessages];
        }

        public string Name => logger.Name;

        public void Lock()
        {
            semaphore.Wait();
        }

        public void ReleaseLock()
        {
            semaphore.Release();
        }

        public void Clear()
        {
            semaphore.Wait();
            head = 0;
            tail = 0;
            count = 0;
            Array.Clear(messages, 0, messages.Length); // Optionally clear the array references to help garbage collection
            semaphore.Release();
        }

        public void AddMessage(LogMessage message)
        {
            semaphore.Wait();
            messages[tail] = message;
            tail = (tail + 1) % maxMessages; // Wrap-around tail index

            if (count == maxMessages)
            {
                // If the buffer is full, overwrite the oldest message
                head = (head + 1) % maxMessages; // Move head forward
            }
            else
            {
                count++;
            }
            semaphore.Release();
        }

        public IEnumerable<ConsoleMessage> GetMessages()
        {
            semaphore.Wait();
            int current = head;
            for (int i = 0; i < count; i++)
            {
                yield return messages[current];
                current = (current + 1) % maxMessages; // Move to next message
            }
            semaphore.Release();
        }
    }

    public class OutputLogWriter : ILogWriter
    {
        private readonly Dictionary<ILogger, LogQueue> loggerToQueue = [];
        private readonly List<LogQueue> queues = [];
        private readonly SemaphoreSlim semaphore = new(1);

        public IReadOnlyList<LogQueue> LogQueues => queues;

        public void Lock()
        {
            semaphore.Wait();
        }

        public void ReleaseLock()
        {
            semaphore.Release();
        }

        private LogQueue GetOrCreateQueue(ILogger logger)
        {
            semaphore.Wait();
            if (!loggerToQueue.TryGetValue(logger, out var queue))
            {
                queue = new LogQueue(logger);
                queues.Add(queue);
                loggerToQueue.Add(logger, queue);
            }
            semaphore.Release();
            return queue;
        }

        private async ValueTask<LogQueue> GetOrCreateQueueAsync(ILogger logger)
        {
            await semaphore.WaitAsync();
            if (!loggerToQueue.TryGetValue(logger, out var queue))
            {
                queue = new LogQueue(logger);
                queues.Add(queue);
                loggerToQueue.Add(logger, queue);
            }
            semaphore.Release();
            return queue;
        }

        public void Clear()
        {
            for (int i = 0; i < queues.Count; i++)
            {
                queues[i].Clear();
            }
        }

        public void Dispose()
        {
        }

        public void Flush()
        {
        }

        public void Write(string message)
        {
            // do nothing?
        }

        public Task WriteAsync(string message)
        {
            // do nothing?
            return Task.CompletedTask;
        }

        public void WriteLine(string message)
        {
            // do nothing?
        }

        public Task WriteLineAsync(string message)
        {
            // do nothing?
            return Task.CompletedTask;
        }

        public void Log(LogMessage message)
        {
            GetOrCreateQueue(message.Logger).AddMessage(message);
        }

        public async Task LogAsync(LogMessage message)
        {
            var queue = await GetOrCreateQueueAsync(message.Logger);
            queue.AddMessage(message);
        }
    }

    public class OutputWidget : EditorWindow
    {
        private static readonly OutputLogWriter outputLogWriter;
        private LogQueue? logQueue;
        private bool wordWrap;
        private static readonly ConsoleColorPalette colorPalette = new();

        static OutputWidget()
        {
            outputLogWriter = new();
            LoggerFactory.AddGlobalWriter(outputLogWriter);
        }

        public OutputWidget()
        {
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name { get; } = "Output";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            if (ImGui.BeginMenuBar())
            {
                ImGui.Text("Show output from:");
                bool combo;
                ImGui.SetNextItemWidth(200f);
                if (logQueue == null)
                {
                    combo = ImGui.BeginCombo("##Output", (byte*)null);
                }
                else
                {
                    combo = ImGui.BeginCombo("##Output", logQueue.Name);
                }

                if (combo)
                {
                    outputLogWriter.Lock();

                    for (int i = 0; i < outputLogWriter.LogQueues.Count; i++)
                    {
                        var queue = outputLogWriter.LogQueues[i];
                        var selected = queue == logQueue;
                        if (ImGui.Selectable(queue.Name, selected))
                        {
                            logQueue = queue;
                        }
                    }
                    ImGui.EndCombo();

                    outputLogWriter.ReleaseLock();
                }

                ImGuiP.SeparatorEx(ImGuiSeparatorFlags.Vertical, 1);

                if (ImGui.MenuItem($"{UwU.CircleXmark}", false, logQueue != null))
                {
                    logQueue?.Clear();
                }

                ImGuiP.SeparatorEx(ImGuiSeparatorFlags.Vertical, 1);

                if (ImGui.MenuItem($"{UwU.Reply}"))
                {
                    wordWrap = !wordWrap;
                }

                ImGui.EndMenuBar();
            }
            Vector2 avail = ImGui.GetContentRegionAvail();
            ImGuiWindowFlags childFlags = ImGuiWindowFlags.None;

            if (!wordWrap)
            {
                avail = new(-1);
                childFlags |= ImGuiWindowFlags.HorizontalScrollbar;
            }

            ImGui.BeginChild("##Content", avail, ImGuiChildFlags.AlwaysUseWindowPadding, childFlags);
            DrawMessages();
            ImGui.EndChild();
        }

        private void DrawMessages()
        {
            if (logQueue == null)
            {
                return;
            }

            foreach (var item in logQueue.GetMessages())
            {
                var color = colorPalette[item.ForegroundColor];
                ImGui.PushStyleColor(ImGuiCol.Text, color);
                if (wordWrap)
                {
                    ImGui.TextWrapped(item.Message);
                }
                else
                {
                    ImGui.TextColored(color, item.Message);
                }
                ImGui.PopStyleColor();
            }
        }
    }
}