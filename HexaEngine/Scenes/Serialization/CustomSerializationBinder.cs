namespace HexaEngine.Scenes.Serialization
{
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OldNameAttribute(string assemblyName, string typeName) : Attribute
    {
        public string AssemblyName { get; } = assemblyName;

        public string TypeName { get; } = typeName;
    }

    public class CustomSerializationBinder : DefaultSerializationBinder
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "")]
        public CustomSerializationBinder()
        {
            foreach (var assemblyType in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assemblyType.GetTypes())
                {
                    foreach (var attribute in type.GetCustomAttributes<OldNameAttribute>())
                    {
                        typeMappings[(attribute.AssemblyName, attribute.TypeName)] = type;
                    }
                }
            }
        }

        public static readonly Dictionary<(string assembly, string type), Type> typeMappings = [];

        public override Type BindToType(string? assemblyName, string typeName)
        {
            if (typeMappings.TryGetValue((assemblyName ?? "", typeName), out Type? mappedType))
            {
                return mappedType;
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}