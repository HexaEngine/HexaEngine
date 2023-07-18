namespace HexaEngine.Scenes
{
    using System.Collections.Concurrent;
    using System.Diagnostics;

    public readonly struct SceneProfiler
    {
        private readonly ConcurrentDictionary<object, double> stages;
        private readonly ConcurrentDictionary<object, long> startTimeStamps;

        public SceneProfiler(int initialStageCount)
        {
            stages = new ConcurrentDictionary<object, double>(2, initialStageCount);
            startTimeStamps = new ConcurrentDictionary<object, long>(2, initialStageCount);
        }

        public double this[object stage]
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

        public int Count => stages.Count;

        public void Start(object o)
        {
            startTimeStamps.TryAdd(o, Stopwatch.GetTimestamp());
        }

        public void End(object o)
        {
            long timestamp = Stopwatch.GetTimestamp();
            if (!stages.TryGetValue(o, out var value))
            {
                value = 0.0;
            }

            stages[o] = value + (timestamp - startTimeStamps[o]) / (double)Stopwatch.Frequency;
            startTimeStamps.TryRemove(o, out _);
        }

        public void Set(object o, double value)
        {
            stages[o] = value;
            startTimeStamps.TryRemove(o, out _);
        }

        public void Clear()
        {
            stages.Clear();
        }
    }
}