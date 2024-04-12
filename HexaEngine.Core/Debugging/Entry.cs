namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Unsafes;

    /// <summary>
    /// Represents an entry with timing data and a list of stages.
    /// </summary>
    public struct Entry
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
        public UnsafeList<Scope> Stages = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Entry"/> struct.
        /// </summary>
        public Entry()
        {
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
    }
}