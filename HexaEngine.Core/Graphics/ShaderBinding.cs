namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a shader binding with a name, type, shader stage, and slot.
    /// </summary>
    public struct ShaderBinding : IEquatable<ShaderBinding>
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

        public override readonly bool Equals(object? obj)
        {
            return obj is ShaderBinding binding && Equals(binding);
        }

        public readonly bool Equals(ShaderBinding other)
        {
            return Name == other.Name &&
                   Stage == other.Stage &&
                   Slot == other.Slot;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Name, Stage, Slot);
        }

        public static bool operator ==(ShaderBinding left, ShaderBinding right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShaderBinding left, ShaderBinding right)
        {
            return !(left == right);
        }
    }
}