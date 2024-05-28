namespace HexaEngine.Analyzers.Annotations
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DisposeIgnoreAttribute : Attribute
    {
    }
}