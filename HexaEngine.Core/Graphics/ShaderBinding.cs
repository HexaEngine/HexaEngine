namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a shader binding with a name, type, shader stage, and slot.
    /// </summary>
    public struct ShaderBinding
    {
        /// <summary>
        /// Gets or sets the name of the binding.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the shader stage in which the binding is used.
        /// </summary>
        public ShaderStage Stage;

        /// <summary>
        /// Gets or sets the slot or index associated with the binding.
        /// </summary>
        public uint Slot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderBinding"/> struct.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="stage">The shader stage in which the binding is used.</param>
        /// <param name="slot">The slot or index associated with the binding.</param>
        public ShaderBinding(string name, ShaderStage stage, uint slot)
        {
            Name = name;
            Stage = stage;
            Slot = slot;
        }
    }
}