namespace HexaEngine.Core
{
    using System;
    using System.Diagnostics;

    public static class Time
    {
        private static long lastTicks;
        private static float fixedTime;
        private static float cumulativeFrameTime;

        // Properties
        public static float Delta { get; private set; }

        public static TimeSpan DeltaTime { get; private set; }

        public static float CumulativeFrameTime { get => cumulativeFrameTime; }

        public static int FixedUpdateRate { get; set; } = 3;

        public static float FixedUpdatePerSecond => FixedUpdateRate / 1000F;

        public static event EventHandler? FixedUpdate;

        // Public Methods
        public static void Initialize()
        {
            lastTicks = Stopwatch.GetTimestamp();
            fixedTime = 0;
            cumulativeFrameTime = 0;
        }

        public static void FrameUpdate()
        {
            var ticks = Stopwatch.GetTimestamp();
            var deltaTime = (float)(ticks - lastTicks);
            deltaTime /= Stopwatch.Frequency;
            lastTicks = ticks;
            // Calculate the frame time by the time difference over the timer speed resolution.
            Delta = deltaTime;
            cumulativeFrameTime += Delta;
            fixedTime += Delta;
            if (deltaTime == 0 || deltaTime < 0)
            {
                throw new InvalidOperationException();
            }
            //Trace.WriteLine(Delta);
            while (fixedTime > FixedUpdatePerSecond)
            {
                fixedTime -= FixedUpdatePerSecond;
                FixedUpdate?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}