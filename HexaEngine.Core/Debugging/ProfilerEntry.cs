namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Unsafes;
    using System;

    /// <summary>
    /// Represents an entry with timing data and a list of stages.
    /// </summary>
    public struct ProfilerEntry : IEquatable<ProfilerEntry>
    {
        /// <summary>
        /// Gets or sets the start timestamp of the entry.
        /// </summary>
        public ulong Start;

        /// <summary>
        /// Gets or sets the end timestamp of the entry.
        /// </summary>
        public ulong End;

        /// <summary>
        /// Gets or sets the list of stages associated with this entry.
        /// </summary>
        public UnsafeList<ProfilerScope> Stages = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilerEntry"/> struct.
        /// </summary>
        public ProfilerEntry()
        {
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ProfilerEntry entry && Equals(entry);
        }

        public readonly bool Equals(ProfilerEntry other)
        {
            return Start == other.Start &&
                   End == other.End &&
                   Stages.Equals(other.Stages);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Start, End, Stages);
        }

        /// <summary>
        /// Maps the unique identifier of a stage to its index in the list of stages.
        /// </summary>
        /// <param name="id">The unique identifier of the stage to find.</param>
        /// <returns>The index of the stage in the list, or -1 if not found.</returns>
        public uint IdToIndex(uint id)
        {
            for (uint i = 0; i < Stages.Size; i++)
            {
                if (Stages[i].Id == id)
                {
                    return i;
                }
            }
            return unchecked((uint)-1);
        }

        public static bool operator ==(ProfilerEntry left, ProfilerEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProfilerEntry left, ProfilerEntry right)
        {
            return !(left == right);
        }
    }
}