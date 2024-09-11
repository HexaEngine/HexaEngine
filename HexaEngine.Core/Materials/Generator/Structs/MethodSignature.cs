namespace HexaEngine.Materials.Generator.Structs
{
    using System.Collections.Generic;

    public struct MethodSignature
    {
        /// <summary>
        /// The method parameters.
        /// </summary>
        public List<Parameter> Parameters = new();

        public MethodSignature(List<Parameter> parameters)
        {
            Parameters = parameters;
        }
    }
}