namespace HexaEngine.Scenes
{
    using System.Collections.Concurrent;
    using System.Diagnostics;

    public struct SceneProfiler
    {
        private readonly Dictionary<object, double> stages;
        private readonly Dictionary<object, long> startTimeStamps;
        private readonly object _lock = new();
        private bool enabled = true;

        public SceneProfiler(int initialStageCount)
        {
            stages = new Dictionary<object, double>(initialStageCount);
            startTimeStamps = new Dictionary<object, long>(initialStageCount);
        }

        public double this[object stage]
        {
            get
            {
                if (!enabled)
                {
                    return -1.0;
                }
                lock (_lock)
                {
                    if (stages.TryGetValue(stage, out var value))
                    {
                        return value;
                    }
                }
                return -1.0;
            }
        }

        public int Count => stages.Count;

        public bool Enabled { get => enabled; set => enabled = value; }

        public void Start(object o)
        {
            if (!enabled)
            {
                return;
            }

            lock (_lock)
            {
                startTimeStamps.TryAdd(o, Stopwatch.GetTimestamp());
            }
        }

        public void End(object o)
        {
            if (!enabled)
            {
                return;
            }

            lock (_lock)
            {
                long timestamp = Stopwatch.GetTimestamp();
                if (!stages.TryGetValue(o, out var value))
                {
                    value = 0.0;
                }

                stages[o] = value + (timestamp - startTimeStamps[o]) / (double)Stopwatch.Frequency;
                startTimeStamps.Remove(o, out _);
            }
        }

        public void Set(object o, double value)
        {
            lock (_lock)
            {
                stages[o] = value;
                startTimeStamps.Remove(o, out _);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                stages.Clear();
            }
        }
    }
}