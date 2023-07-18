namespace HexaEngine.Core
{
    using Silk.NET.SDL;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents the time management system of the engine.
    /// </summary>
    public static class Time
    {
        private static readonly Sdl sdl = Application.sdl;
        private static ulong last;
        private static float fixedTime;
        private static float cumulativeFrameTime;

        /// <summary>
        /// Gets the time elapsed since the last frame in seconds.
        /// </summary>
        public static float Delta { get; private set; }

        /// <summary>
        /// Gets the cumulative frame time since the engine initialization in seconds.
        /// </summary>
        public static float CumulativeFrameTime { get => cumulativeFrameTime; }

        /// <summary>
        /// Gets or sets the fixed update rate in frames per second (FPS).
        /// </summary>
        public static int FixedUpdateRate { get; set; } = 10;

        /// <summary>
        /// Gets the fixed time step size in seconds.
        /// </summary>
        public static float FixedDelta => FixedUpdateRate / 1000F;

        /// <summary>
        /// Gets or sets the maximum allowed frame time in seconds.
        /// </summary>
        public static float MaxFrameTime { get; set; }

        /// <summary>
        /// Occurs when a fixed update is triggered.
        /// </summary>
        public static event EventHandler? FixedUpdate;

        /// <summary>
        /// Initializes the Time system.
        /// </summary>
        public static void Initialize()
        {
            last = sdl.GetPerformanceCounter();
            fixedTime = 0;
            cumulativeFrameTime = 0;
        }

        /// <summary>
        /// Updates the Time system and calculates the delta time.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the delta time is 0 or less than 0.</exception>
        public static void FrameUpdate()
        {
            ulong now = sdl.GetPerformanceCounter();
            double deltaTime = (double)(now - last) / sdl.GetPerformanceFrequency();
            last = now;

            Delta = (float)deltaTime;
            cumulativeFrameTime += Delta;
            fixedTime += Delta;

            if (deltaTime == 0 || deltaTime < 0)
            {
                throw new InvalidOperationException("Delta time cannot be 0 or less than 0");
            }

            while (fixedTime > FixedDelta)
            {
                fixedTime -= FixedDelta;
                FixedUpdate?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}