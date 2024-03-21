namespace HexaEngine.UI.Markup
{
    using HexaEngine.UI.Markup.Parser;
    using HexaEngine.UI.Xaml;
    using HexaEngine.UI.Xaml.Schema;
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Xml;

    public class XamlReader
    {
        private const string ClrNamespaceIdentifier = "clr-namespace:";
        private const string AssemblyIdentifier = "assembly=";
        private const string XmlnsCustomIdentifier = "xmlns:";
        private readonly Dictionary<string, Assembly> assemblyCache = [];
        private readonly Dictionary<Assembly, Dictionary<string, Type>> typeCache = [];
        private readonly XmlnsDictionary xmlnsDic = [];

        private readonly List<IPropertyParser> propertyParsers =
        [
            new EnumParser(),
            new StringParser(),
            new IntParser(),
            new FloatParser(),
            new ThicknessParser(),
            new BrushParser(),
        ];

        private Assembly GetAssembly(string assemblyPath)
        {
            if (!assemblyCache.TryGetValue(assemblyPath, out var assembly))
            {
                assembly = Assembly.Load(assemblyPath);
                assemblyCache.Add(assemblyPath, assembly);
                typeCache.Add(assembly, []);
            }
            return assembly;
        }

        private Type GetType(Assembly assembly, string typeName)
        {
            var cache = typeCache[assembly];
            if (!cache.TryGetValue(typeName, out var type))
            {
                type = assembly.GetType(typeName);
                if (type == null)
                {
                    throw new InvalidOperationException("Couldn't find type");
                }
                cache.Add(typeName, type);
            }
            return type;
        }

        private string rootNamespaceUri;

        public object? Parse(string filePath)
        {
            object? root = null;
            xmlnsDic.Clear();
            using XmlReader reader = XmlReader.Create(filePath);

            XamlSchemaContext schemaContext = new(AppDomain.CurrentDomain.GetAssemblies());

            object? current = null;
            IAddChild? addChild = null;
            IList? list = null;

            Stack<object> stack = new();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        rootNamespaceUri ??= reader.NamespaceURI;

                        ParseNamespaceUri(rootNamespaceUri, reader.NamespaceURI, out var clrNamespace, out var assemblyPath);
                        int indexOfDot = reader.Name.IndexOf('.');
                        if (indexOfDot != -1)
                        {
                            string propName = reader.Name[(indexOfDot + 1)..];
                            object nextProp = CreatePropertyType(current, propName);
                            if (current != null)
                            {
                                stack.Push(current);
                            }

                            current = nextProp;
                            addChild = current as IAddChild;
                            list = current as IList;

                            continue;
                        }

                        XamlTypeName typeName = XamlTypeName.Parse(reader.Name, xmlnsDic);

                        XamlType xamlType = schemaContext.GetXamlType(typeName);

                        object next = Activator.CreateInstance(xamlType.UnderlyingType);

                        PopulateProperties(xamlType.UnderlyingType, next, reader, schemaContext);

                        addChild?.AddChild(next);
                        list?.Add(next);

                        if (reader.IsEmptyElement)
                        {
                            continue;
                        }

                        if (current != null)
                        {
                            stack.Push(current);
                        }

                        current = next;
                        addChild = current as IAddChild;
                        list = current as IList;

                        root ??= current;

                        break;

                    case XmlNodeType.Text:
                        addChild?.AddText(reader.Value);
                        break;

                    case XmlNodeType.EndElement:

                        stack.TryPop(out current);
                        addChild = current as IAddChild;
                        list = current as IList;
                        break;
                }
            }

            return root;
        }

        private static object? CreatePropertyType(object instance, string propertyName)
        {
            var propertyInfo = instance.GetType().GetProperty(propertyName);
            object value;
            if (propertyInfo.CanWrite)
            {
                value = Activator.CreateInstance(propertyInfo.PropertyType);
                propertyInfo.SetValue(instance, value);
            }
            else
            {
                value = propertyInfo.GetValue(instance);
            }

            return value;
        }

        private void PopulateProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, object target, XmlReader reader, XamlSchemaContext schemaContext)
        {
            while (reader.MoveToNextAttribute())
            {
                // ignore namespace declarations
                if (reader.Name == "xmlns")
                {
                    rootNamespaceUri = reader.Value;
                    continue;
                }

                if (reader.Name.StartsWith(XmlnsCustomIdentifier))
                {
                    AddNamespace(reader);
                    continue;
                }

                int indexOfDot = reader.Name.IndexOf('.');
                if (indexOfDot != -1)
                {
                    string typeName = reader.Name[..indexOfDot];
                    string attachedPropertyName = reader.Name[(indexOfDot + 1)..];

                    XamlTypeName xamlTypeName = XamlTypeName.Parse(typeName, xmlnsDic);
                    XamlType xamlType = schemaContext.GetXamlType(xamlTypeName);
                    Type attachedType = xamlType.UnderlyingType;
                    var dp = DependencyProperty.GetByName(attachedType, attachedPropertyName);

                    for (var i = 0; i < propertyParsers.Count; i++)
                    {
                        var propertyParser = propertyParsers[i];
                        if (propertyParser.TryParse(dp.PropertyType, reader.Value, CultureInfo.InvariantCulture, out var result))
                        {
                            ((DependencyObject)target).SetValue(dp, result);
                            break;
                        }
                    }
                }

                var propertyInfo = type.GetProperty(reader.Name);

                // ignore unknown properties
                if (propertyInfo == null)
                {
                    continue;
                }

                for (var i = 0; i < propertyParsers.Count; i++)
                {
                    var propertyParser = propertyParsers[i];
                    if (propertyParser.TryParse(propertyInfo.PropertyType, reader.Value, CultureInfo.InvariantCulture, out var result))
                    {
                        propertyInfo.SetValue(target, result);
                        break;
                    }
                }
            }
        }

        private void AddNamespace(XmlReader reader)
        {
            xmlnsDic.Add(reader.Name["xmlns:".Length..], reader.Value);
        }

        private static string ParseType(string name)
        {
            int start = name.IndexOf(':');
            if (start == -1)
            {
                return name;
            }
            return name[(start + 1)..];
        }

        private static void ParseNamespaceUri(string rootNamespaceUri, string namespaceUri, out string clrNamespace, out string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(namespaceUri))
            {
                namespaceUri = rootNamespaceUri;
            }

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
            }
            else if (Uri.IsWellFormedUriString(namespaceUri, UriKind.Absolute))
            {
                clrNamespace = null;
                assemblyPath = null;
                return;
            }
            else
            {
                throw new InvalidOperationException("Namespace URI is invalid.");
            }
        }

        public static int Find(string str, char target, int index)
        {
            int currentIndex = index;

            while (currentIndex < str.Length && str[currentIndex] != target)
            {
                currentIndex++;
            }

            return currentIndex - index;
        }
    }
}