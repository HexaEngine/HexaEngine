namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Unsafes;

    /// <summary>
    /// Represents a scope used for profiling with associated timing data and samples.
    /// </summary>
    public struct Scope
    {
        /// <summary>
        /// Gets or sets the unique identifier of the scope.
        /// </summary>
        public uint Id;

        /// <summary>
        /// Gets or sets the level of the scope.
        /// </summary>
        public byte Level;

        /// <summary>
        /// Gets or sets the start timestamp of the scope.
        /// </summary>
        public ulong Start;

        /// <summary>
        /// Gets or sets the end timestamp of the scope.
        /// </summary>
        public ulong End;

        /// <summary>
        /// Gets or sets a value indicating whether the scope has been finalized.
        /// </summary>
        public bool Finalized = false;

        /// <summary>
        /// Gets or sets a value indicating whether the scope has been used.
        /// </summary>
        public bool Used;

        /// <summary>
        /// Gets or sets the name of the scope.
        /// </summary>
        public StdString Name;

        /// <summary>
        /// Gets or sets the ring buffer for start samples.
        /// </summary>
        public UnsafeRingBuffer<float> StartSamples;

        /// <summary>
        /// Gets or sets the ring buffer for end samples.
        /// </summary>
        public UnsafeRingBuffer<float> EndSamples;

        /// <summary>
        /// Gets or sets the duration of the scope.
        /// </summary>
        public double Duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scope"/> struct with the specified identifier, name, and sample count.
        /// </summary>
        /// <param name="id">The unique identifier of the scope.</param>
        /// <param name="name">The name of the scope.</param>
        /// <param name="sampleCount">The maximum number of samples to store.</param>
        public Scope(uint id, string name, int sampleCount = 1000)
        {
            Id = id;
            Name = name;
            StartSamples = new(sampleCount);
            EndSamples = new(sampleCount);
        }
    }
}