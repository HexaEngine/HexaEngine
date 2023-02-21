namespace HexaEngine.Editor.Materials.Generator.Structs
{
    /// <summary>
    /// An method definition.
    /// </summary>
    public struct Method
    {
        /// <summary>
        /// The name of the method.
        /// </summary>
        public string Name;

        /// <summary>
        /// The signature of the method.
        /// </summary>
        public MethodSignature Signature;

        /// <summary>
        /// The body of the method.
        /// </summary>
        public MethodBody Body;

        /// <summary>
        /// The return type of the method.
        /// </summary>
        public SType ReturnType;

        public Method(string name, MethodSignature signature, SType returnType)
        {
            Name = name;
            Signature = signature;
            ReturnType = returnType;
        }
    }
}