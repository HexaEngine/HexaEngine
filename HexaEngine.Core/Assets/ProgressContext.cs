namespace HexaEngine.Core.Assets
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;

    public class ProgressContext
    {
        private readonly IImportProgress progress;
        private readonly List<SubStep> subSteps = [];

        private int currentIdx = 0;

        public ProgressContext(IImportProgress progress, int rootSubSteps)
        {
            this.progress = progress;

            for (int i = 0; i < rootSubSteps; i++)
            {
                subSteps.Add(new(0, 0));
            }
        }

        public void SetSteps(int count)
        {
            subSteps.Clear();
            for (int i = 0; i < count; i++)
            {
                subSteps.Add(new(0, 0));
            }
        }

        private struct SubStep
        {
            public int Progress;
            public int MaxProgress;

            public SubStep(int progress, int maxProgress)
            {
                Progress = progress;
                MaxProgress = maxProgress;
            }
        }

        public void BeginStep(int itemCount, string title)
        {
            var current = subSteps[currentIdx];
            current.Progress = 0;
            current.MaxProgress = itemCount;
            subSteps[currentIdx] = current;
            progress.BeginStep(title);
        }

        public void EndStep()
        {
            progress.EndStep();
            currentIdx++;
            if (currentIdx == subSteps.Count)
            {
                progress.Report(1);
                return;
            }
        }

        public void AddProgress(string message)
        {
            var current = subSteps[currentIdx];
            current.Progress++;

            float subStepProgress = current.Progress / (float)current.MaxProgress;
            float baseProgress = currentIdx / (float)subSteps.Count;
            float endProgress = (currentIdx + 1) / (float)subSteps.Count;
            float progress = MathUtil.Lerp(baseProgress, endProgress, subStepProgress);
            subSteps[currentIdx] = current;

            this.progress.Report(progress);
            LogMessage(LogSeverity.Info, message);
        }

        public void LogMessage(LogSeverity severity, string message)
        {
            progress.LogMessage(new(null!, severity, message));
        }

        public void LogMessage(string message)
        {
            LogMessage(Evaluate(message), message);
        }

        /// <summary>
        /// A helper method that evaluates the log severity based on a log message.
        /// </summary>
        /// <param name="message">The log message to evaluate.</param>
        /// <returns>The determined log severity.</returns>
        private static LogSeverity Evaluate(string message)
        {
            LogSeverity type = LogSeverity.Info;
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
                type = LogSeverity.Info;
            }
            else if (message.Contains("information", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Info;
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
    }
}