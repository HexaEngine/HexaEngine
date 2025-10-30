namespace HexaEngine.UI.Xaml.Schema
{
    public class XamlTypeName
    {
        public XamlTypeName()
        {
        }

        public XamlTypeName(string name, string @namespace)
        {
            Name = name;
            Namespace = @namespace;
            TypeArguments = [];
        }

        public XamlTypeName(string name, string @namespace, IEnumerable<XamlTypeName> typeArguments)
        {
            Name = name;
            Namespace = @namespace;
            TypeArguments = new(typeArguments);
        }

        public XamlTypeName(XamlType xamlType)
        {
            Name = xamlType.Name;
            Namespace = xamlType.PreferredXamlNamespace;
            TypeArguments = new(xamlType.TypeArguments.Select(x => new XamlTypeName(x)));
        }

        public string Name { get; set; } = null!;

        public string Namespace { get; set; } = null!;

        public List<XamlTypeName> TypeArguments { get; } = null!;

        public static XamlTypeName Parse(string typeName, IXamlNamespaceResolver namespaceResolver)
        {
            // Check if the type name contains type arguments
            int colonIndex = typeName.IndexOf(':');
            int bracketIndex = typeName.IndexOf('[');
            if (bracketIndex != -1)
            {
                string baseTypeName = typeName[..bracketIndex];
                string genericArgsString = typeName.Substring(bracketIndex + 1, typeName.Length - bracketIndex - 2);
                string[] genericArgNames = genericArgsString.Split(',');

                var genericTypeArgs = new List<XamlTypeName>();
                foreach (string genericArgName in genericArgNames)
                {
                    string trimmedGenericArgName = genericArgName.Trim();
                    genericTypeArgs.Add(Parse(trimmedGenericArgName, namespaceResolver));
                }

                if (colonIndex != -1)
                {
                    return new XamlTypeName(baseTypeName[(colonIndex + 1)..], namespaceResolver.GetNamespace(baseTypeName[..(colonIndex - 1)]), genericTypeArgs);
                }
                else
                {
                    return new XamlTypeName(baseTypeName, namespaceResolver.GetNamespace(string.Empty), genericTypeArgs);
                }
            }
            else
            {
                if (colonIndex != -1)
                {
                    return new XamlTypeName(typeName[(colonIndex + 1)..], namespaceResolver.GetNamespace(typeName[..(colonIndex - 1)]));
                }
                else
                {
                    return new XamlTypeName(typeName, namespaceResolver.GetNamespace(string.Empty));
                }
            }
        }
    }
}