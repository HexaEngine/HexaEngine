namespace HexaEngine.Core.Debugging
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public enum DisplayOptions
    {
        Auto = 0,
        Seconds,
        Milliseconds,
        Microseconds,
        Nanoseconds,
        Ticks,
    }

    public class MiniProfiler
    {
        public static readonly MiniProfiler Instance = new();
        private readonly Dictionary<string, long> starts = [];
        private readonly Dictionary<string, (double sum, long count)> avgSamples = new();

        public void Begin(string name)
        {
            starts[name] = Stopwatch.GetTimestamp();
        }

        public double End(string name)
        {
            long end = Stopwatch.GetTimestamp();
            long start = starts[name];
            starts.Remove(name);
            return (double)(end - start) / Stopwatch.Frequency;
        }

        public double EndMs(string name)
        {
            long end = Stopwatch.GetTimestamp();
            long start = starts[name];
            starts.Remove(name);
            return (double)(end - start) / Stopwatch.Frequency * 1000;
        }

        public void EndDebug(string name, DisplayOptions displayOption = DisplayOptions.Auto)
        {
            double end = End(name);
            var (factor, unit) = GetDisplayValues(end, displayOption);
            Debug.WriteLine($"{name}: {end * factor}{unit}");
        }

        public void EndImGui(string name, DisplayOptions displayOption = DisplayOptions.Auto)
        {
            double end = End(name);
            var (factor, unit) = GetDisplayValues(end, displayOption);
            ImGuiConsole.WriteLine($"{name}: {end * factor}{unit}");
        }

        public void EndSumAvg(string name)
        {
            double end = End(name);

            avgSamples.TryGetValue(name, out var samples);
            samples.sum += end;
            samples.count++;
            avgSamples[name] = samples;
        }

        public void ReportSumAvg(string name, DisplayOptions displayOption = DisplayOptions.Auto)
        {
            (double sum, long count) = avgSamples[name];
            var avg = sum / count;
            avgSamples[name] = default;

            var (factorAvg, unitAvg) = GetDisplayValues(avg, displayOption);
            var (factorSum, unitSum) = GetDisplayValues(sum, displayOption);
            Debug.WriteLine($"{name}: Sum: {sum * factorSum}{unitSum} Avg: {avg * factorAvg}{unitAvg}");
        }

        public void ReportImGuiSumAvg(string name, DisplayOptions displayOption = DisplayOptions.Auto)
        {
            (double sum, long count) = avgSamples[name];
            var avg = sum / count;
            avgSamples[name] = default;

            var (factorAvg, unitAvg) = GetDisplayValues(avg, displayOption);
            var (factorSum, unitSum) = GetDisplayValues(sum, displayOption);
            ImGuiConsole.WriteLine($"{name}: Sum: {sum * factorSum}{unitSum} Avg: {avg * factorAvg}{unitAvg}");
        }

        public void Clear()
        {
            starts.Clear();
            avgSamples.Clear();
        }

        private static (double factor, string unit) GetDisplayValues(double value, DisplayOptions displayOption)
        {
            double factor = 1;
            string unit;

            switch (displayOption)
            {
                case DisplayOptions.Milliseconds:
                    factor = 1000;
                    unit = "ms";
                    break;

                case DisplayOptions.Microseconds:
                    factor = 1_000_000;
                    unit = "µs";
                    break;

                case DisplayOptions.Nanoseconds:
                    factor = 1_000_000_000;
                    unit = "ns";
                    break;

                case DisplayOptions.Ticks:
                    factor = Stopwatch.Frequency;
                    unit = "ticks";
                    break;

                case DisplayOptions.Auto:
                default:
                    if (value >= 0.1)
                    {
                        unit = "s";
                    }
                    else if (value >= 0.00001)
                    {
                        factor = 1000;
                        unit = "ms";
                    }
                    else if (value >= 0.000_00001)
                    {
                        factor = 1_000_000;
                        unit = "µs";
                    }
                    else
                    {
                        factor = 1_000_000_000;
                        unit = "ns";
                    }
                    break;
            }

            return (factor, unit);
        }
    }
}