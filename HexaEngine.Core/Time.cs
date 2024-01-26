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
        private static float fixedTime;
        private static float cumulativeFrameTime;
        private static float gameTime;
        private static float gameTimeScale = 20;

        /// <summary>
        /// Gets how many frames have passed since the start.
        /// </summary>
        public static long Frame => frame;

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
            last = sdl.GetPerformanceCounter();
            fixedTime = 0;
            cumulativeFrameTime = 0;
            gameTime = 0;
        }

        /// <summary>
        /// Updates the Time system and calculates the delta time.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the delta time is 0 or less than 0.</exception>
        public static void FrameUpdate()
        {
            frame++;
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

            gameTime += (float)(deltaTime * gameTimeScale) / 60 / 60;
            gameTime %= 24;

            while (fixedTime > FixedDelta)
            {
                fixedTime -= FixedDelta;
                FixedUpdate?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}