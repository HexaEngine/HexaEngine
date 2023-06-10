namespace HexaEngine.Core.Graphics
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public class CPUProfiler
    {
        private readonly Dictionary<string, double> stages;
        private readonly Dictionary<string, long> startTimeStamps;

        public CPUProfiler(int initialStageCount)
        {
            stages = new Dictionary<string, double>(initialStageCount);
            startTimeStamps = new Dictionary<string, long>(initialStageCount);
        }

        public double this[string stage]
        {
            get
            {
                if (stages.TryGetValue(stage, out var value))
                {
                    return value;
                }

                return -1.0;
            }
        }

        public void Begin(string o)
        {
            startTimeStamps.Add(o, Stopwatch.GetTimestamp());
        }

        public void End(string o)
        {
            long timestamp = Stopwatch.GetTimestamp();
            if (!stages.TryGetValue(o, out var value))
            {
                value = 0.0;
            }

            stages[o] = value + (timestamp - startTimeStamps[o]) / (double)Stopwatch.Frequency;
            startTimeStamps.Remove(o);
        }

        public void Clear()
        {
            stages.Clear();
        }
    }
}