namespace HexaEngine.Core
{
    using System;
    using System.Diagnostics;

    public static class Time
    {
        private static long last;
        private static float fixedTime;
        private static float cumulativeFrameTime;

        public static float Delta { get; private set; }

        public static float CumulativeFrameTime { get => cumulativeFrameTime; }

        public static int FixedUpdateRate { get; set; } = 3;

        public static float FixedUpdateDelta => FixedUpdateRate / 1000F;

        public static event EventHandler? FixedUpdate;

        public static void Initialize()
        {
            last = Stopwatch.GetTimestamp();
            fixedTime = 0;
            cumulativeFrameTime = 0;
        }

        public static void FrameUpdate()
        {
            long now = Stopwatch.GetTimestamp();
            double deltaTime = (double)(now - last) / Stopwatch.Frequency;
            last = now;

            Delta = (float)deltaTime;
            cumulativeFrameTime += Delta;
            fixedTime += Delta;

            if (deltaTime == 0 || deltaTime < 0)
            {
                throw new InvalidOperationException("Delta time cannot be 0 or less than 0");
            }

            while (fixedTime > FixedUpdateDelta)
            {
                fixedTime -= FixedUpdateDelta;
                FixedUpdate?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}