namespace HexaEngine.Core
{
    using Silk.NET.SDL;
    using System;

    /// <summary>
    /// Represents the time management system of the engine.
    /// </summary>
    public static class Time
    {
        private static readonly Sdl sdl = Application.sdl;
        private static long frame;
        private static ulong last;
        private static ulong lastFixed;
        private static float fixedTime;
        private static volatile float cumulativeFrameTime;
        private static volatile float gameTime;
        private static float gameTimeScale = 20;
        private static volatile float delta;

        /// <summary>
        /// Gets how many frames have passed since the start.
        /// </summary>
        public static long Frame => frame;

        /// <summary>
        /// Gets the time elapsed since the last frame in seconds.
        /// </summary>
        public static float Delta { get => delta; }

        /// <summary>
        /// Gets the cumulative frame time since the engine initialization in seconds.
        /// </summary>
        public static float CumulativeFrameTime { get => cumulativeFrameTime; }

        /// <summary>
        /// Gets or sets the fixed update rate in frames per second (FPS).
        /// </summary>
        public static int FixedUpdateRate { get; set; } = 60;

        /// <summary>
        /// Gets the fixed time step size in seconds.
        /// </summary>
        public static float FixedDelta => 1f / FixedUpdateRate;

        /// <summary>
        /// Gets or sets the maximum allowed frame time in seconds.
        /// </summary>
        public static float MaxFrameTime { get; set; }

        /// <summary>
        /// Gets or sets the game time, is normalized in the 24h format.
        /// </summary>
        public static float GameTime { get => gameTime; set => gameTime = value % 24; }

        /// <summary>
        /// Gets or sets the game time scale, 1s realtime multiplied by scale.
        /// </summary>
        public static float GameTimeScale { get => gameTimeScale; set => gameTimeScale = value; }

        /// <summary>
        /// Occurs when a fixed update is triggered.
        /// </summary>
        public static event EventHandler? FixedUpdate;

        /// <summary>
        /// Resets the time system.
        /// </summary>
        public static void ResetTime()
        {
            Interlocked.Exchange(ref last, 0);
            Interlocked.Exchange(ref lastFixed, 0);
            Interlocked.Exchange(ref fixedTime, 0);
            Interlocked.Exchange(ref cumulativeFrameTime, 0);
            Interlocked.Exchange(ref gameTime, 0);
        }

        /// <summary>
        /// Updates the Time system and calculates the delta time.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the delta time is 0 or less than 0.</exception>
        public static void FrameUpdate()
        {
            Interlocked.Increment(ref frame);

            ulong now = sdl.GetPerformanceCounter();

            if (last == 0)
            {
                last = now;
                delta = 1 / sdl.GetPerformanceFrequency();
                return;
            }

            double deltaTime = (double)(now - last) / sdl.GetPerformanceFrequency();
            last = now;

            delta = (float)deltaTime;
            cumulativeFrameTime += Delta;

            if (deltaTime == 0 || deltaTime < 0)
            {
                throw new InvalidOperationException("Delta time cannot be 0 or less than 0");
            }

            gameTime += (float)(deltaTime * gameTimeScale) / 60 / 60;
            gameTime %= 24;
        }

        /// <summary>
        /// Executes the fixed update tick based on a fixed time interval.
        /// </summary>
        /// <remarks>
        /// This method accumulates time between fixed update ticks and invokes the FixedUpdate event
        /// each time the accumulated time exceeds the fixed time interval.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the delta time is 0 or less than 0.</exception>
        public static void FixedUpdateTick()
        {
            ulong now = sdl.GetPerformanceCounter();

            if (lastFixed == 0)
            {
                lastFixed = now;
                return;
            }

            double deltaTime = (double)(now - lastFixed) / sdl.GetPerformanceFrequency();
            lastFixed = now;

            fixedTime += (float)deltaTime;

            if (deltaTime == 0 || deltaTime < 0)
            {
                throw new InvalidOperationException("Delta time cannot be 0 or less than 0");
            }

            while (fixedTime >= FixedDelta)
            {
                FixedUpdate?.Invoke(null, EventArgs.Empty);
                fixedTime -= FixedDelta;
            }
        }
    }
}