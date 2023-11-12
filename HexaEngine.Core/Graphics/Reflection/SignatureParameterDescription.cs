namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Describes a parameter in a signature.
    /// </summary>
    public struct SignatureParameterDescription
    {
        /// <summary>
        /// The semantic name.
        /// </summary>
        public string SemanticName;

        /// <summary>
        /// The semantic index.
        /// </summary>
        public uint SemanticIndex;

        /// <summary>
        /// The register.
        /// </summary>
        public uint Register;

        /// <summary>
        /// The system value type.
        /// </summary>
        public Name SystemValueType;

        /// <summary>
        /// The component type.
        /// </summary>
        public RegisterComponentType ComponentType;

        /// <summary>
        /// The mask.
        /// </summary>
        public byte Mask;

        /// <summary>
        /// The read-write mask.
        /// </summary>
        public byte ReadWriteMask;

        /// <summary>
        /// The stream.
        /// </summary>
        public uint Stream;

        /// <summary>
        /// The minimum precision.
        /// </summary>
        public MinPrecision MinPrecision;
    }
}