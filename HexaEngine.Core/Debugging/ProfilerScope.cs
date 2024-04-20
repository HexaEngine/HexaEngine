namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a scope used for profiling with associated timing data and samples.
    /// </summary>
    public struct ProfilerScope : IEquatable<ProfilerScope>
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

        public float LastStartSample;
        public float LastEndSample;

        /// <summary>
        /// Gets or sets the duration of the scope.
        /// </summary>
        public double Duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilerScope"/> struct with the specified identifier, name, and sample count.
        /// </summary>
        /// <param name="id">The unique identifier of the scope.</param>
        /// <param name="name">The name of the scope.</param>
        /// <param name="sampleCount">The maximum number of samples to store.</param>
        public ProfilerScope(uint id, string name, int sampleCount = 1000)
        {
            Id = id;
            Name = name;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ProfilerScope scope && Equals(scope);
        }

        public readonly bool Equals(ProfilerScope other)
        {
            return Id == other.Id &&
                   Level == other.Level &&
                   Start == other.Start &&
                   End == other.End &&
                   Finalized == other.Finalized &&
                   Used == other.Used &&
                   EqualityComparer<StdString>.Default.Equals(Name, other.Name) &&
                   Duration == other.Duration;
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Level);
            hash.Add(Start);
            hash.Add(End);
            hash.Add(Finalized);
            hash.Add(Used);
            hash.Add(Name);
            hash.Add(Duration);
            return hash.ToHashCode();
        }

        public static bool operator ==(ProfilerScope left, ProfilerScope right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProfilerScope left, ProfilerScope right)
        {
            return !(left == right);
        }
    }
}