namespace HexaEngine.UI.Xaml
{
    using HexaEngine.UI.Markup;
    using HexaEngine.UI.Xaml.Schema;
    using System;
    using System.Reflection;

    public class XamlSchemaContext
    {
        private readonly List<Assembly> referenceAssemblies;
        private readonly Dictionary<string, Assembly> namespaceToAssembly = [];
        private readonly Dictionary<Assembly, XmlnsDefinitionAttribute?> definitions = [];

        public XamlSchemaContext()
        {
            referenceAssemblies = [];
        }

        public virtual IEnumerable<string> GetAllXamlNamespaces()
        {
            foreach (var keyValuePair in namespaceToAssembly)
            {
                yield return keyValuePair.Key;
            }
        }

        public virtual ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
        {
            List<XamlType> types = [];
            if (namespaceToAssembly.TryGetValue(xamlNamespace, out var assembly))
            {
                XmlnsDefinitionAttribute? xmlnsDefinition = definitions[assembly];
                string @namespace = xmlnsDefinition?.ClrNamespace ?? string.Empty;

                foreach (var type in assembly.DefinedTypes)
                {
                    if (type.Namespace == null || !type.Namespace.StartsWith(@namespace))
                    {
                        continue;
                    }
                    types.Add(new(type, this));
                }
            }
            return types;
        }

        public XamlSchemaContext(IEnumerable<Assembly> assemblies)
        {
            referenceAssemblies = new(assemblies);
            for (int i = 0; i < referenceAssemblies.Count; i++)
            {
                var assembly = referenceAssemblies[i];
                XmlnsDefinitionAttribute? attribute = assembly.GetCustomAttribute<XmlnsDefinitionAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                namespaceToAssembly.Add(attribute.XmlNamespace, assembly);
                definitions.Add(assembly, attribute);
            }
        }

        public XamlType GetXamlType(Type clrType)
        {
            return new XamlType(clrType, this);
        }

        public XamlType GetXamlType(XamlTypeName xamlTypeName)
        {
            if (namespaceToAssembly.TryGetValue(xamlTypeName.Namespace, out var assembly))
            {
                return GetXamlTypeInternal(xamlTypeName, assembly);
            }

            if (TryParseNamespaceUri(xamlTypeName.Namespace, out string clrNamespace, out string assemblyPath))
            {
                assembly = OnAssemblyResolve(assemblyPath);
                XmlnsDefinitionAttribute xmlnsDefinition = new(xamlTypeName.Namespace, clrNamespace);
                definitions[assembly] = xmlnsDefinition;
                namespaceToAssembly.Add(xamlTypeName.Namespace, assembly);
                return GetXamlTypeInternal(xamlTypeName, assembly);
            }

            return new XamlType(xamlTypeName.Namespace, xamlTypeName.Name, xamlTypeName.TypeArguments.Select(GetXamlType).ToList(), this);
        }

        private XamlType GetXamlTypeInternal(XamlTypeName xamlTypeName, Assembly? assembly)
        {
            XmlnsDefinitionAttribute? xmlnsDefinition = definitions[assembly!];
            string @namespace = xmlnsDefinition?.ClrNamespace ?? string.Empty;
            Type? clrType = assembly!.GetType($"{@namespace}.{xamlTypeName.Name}")!;

            if (clrType == null)
            {
                foreach (var item in assembly.DefinedTypes!)
                {
                    if (item.Namespace == null || !item.Namespace.StartsWith(@namespace))
                    {
                        continue;
                    }
                    if (item.Name == xamlTypeName.Name)
                    {
                        clrType = item;
                    }
                }
            }

            if (clrType == null)
            {
                return new XamlType(xamlTypeName.Namespace, xamlTypeName.Name, xamlTypeName.TypeArguments.Select(GetXamlType).ToList(), this);
            }

            return new XamlType(clrType, this);
        }

        private const string ClrNamespaceIdentifier = "clr-namespace:";
        private const string AssemblyIdentifier = "assembly=";

        private static bool TryParseNamespaceUri(string namespaceUri, out string clrNamespace, out string assemblyPath)
        {
            // example: clr-namespace:HexaEngine.UI.Controls;assembly=HexaEngine
            int clrNamespaceIndex = namespaceUri.IndexOf(ClrNamespaceIdentifier);
            int assemblyIndex = namespaceUri.IndexOf(AssemblyIdentifier);

            if (clrNamespaceIndex >= 0 && assemblyIndex >= 0)
            {
                int clrNamespaceStartIndex = clrNamespaceIndex + ClrNamespaceIdentifier.Length;
                int clrNamespaceLength = Find(namespaceUri, ';', clrNamespaceStartIndex);
                clrNamespace = namespaceUri.Substring(clrNamespaceStartIndex, clrNamespaceLength);

                int assemblyStartIndex = assemblyIndex + AssemblyIdentifier.Length;
                int assemblyLength = Find(namespaceUri, ';', assemblyStartIndex);
                assemblyPath = namespaceUri.Substring(assemblyStartIndex, assemblyLength);
                return true;
            }
            else if (Uri.IsWellFormedUriString(namespaceUri, UriKind.Absolute))
            {
                clrNamespace = null!;
                assemblyPath = null!;
                return false;
            }
            else
            {
                clrNamespace = null!;
                assemblyPath = null!;
                return false;
            }
        }

        private static int Find(string str, char target, int index)
        {
            int currentIndex = index;

            while (currentIndex < str.Length && str[currentIndex] != target)
            {
                currentIndex++;
            }

            return currentIndex - index;
        }

        public List<Assembly> ReferenceAssemblies => referenceAssemblies;

        protected internal virtual Assembly OnAssemblyResolve(string assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
    }
}