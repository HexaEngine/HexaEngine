namespace HexaEngine.Editor.Tools
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class AsyncTool : ITool
    {
        private readonly ILogger logger;
        private readonly string name;
        private readonly string filter;
        private readonly Func<string, Task> open;

        public AsyncTool(string name, string filter, Func<string, Task> open)
        {
            logger = LoggerFactory.GetLogger($"Tool - {name}");
            this.name = name;
            this.filter = filter;
            this.open = open;
        }

        public string Name => name;

        public string Filter => filter;

        public Task Open(string path)
        {
            return open(path).ContinueWith(task =>
            {
                if (!task.IsCompletedSuccessfully && task.Exception != null)
                {
                    logger.Error($"Failed to open file with Tool: {Name}");
                    logger.Log(task.Exception);
                    MessageBox.Show($"Failed to open file with Tool: {Name}", task.Exception.Message);
                }
                task.Dispose();

                return Task.CompletedTask;
            });
        }

        public bool CanOpen(string path)
        {
            var ex = Path.GetExtension(path.AsSpan());
            int j = 0;
            for (var i = 0; i < filter.Length; i++)
            {
                var c = filter[i];
                var cEx = ex[j];

                // just reset it when a | symbol is found, you never know what users name their files.
                if (c == '|')
                {
                    j = 0;
                }

                if (c == cEx)
                {
                    j++;
                }
                else
                {
                    j = 0;
                }
                if (j == ex.Length)
                {
                    return true;
                }
            }
            return false;
        }
    }
}