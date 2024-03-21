namespace HexaEngine.UI.Xaml
{
    using System;

    public class XamlType
    {
        private readonly Type underlyingType;

        protected XamlType(string typeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
        {
            Name = typeName;
            TypeArguments = typeArguments;
            SchemaContext = schemaContext;
        }

        public XamlType(string unknownTypeNamespace, string unknownTypeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
        {
            Name = unknownTypeName;
            TypeArguments = typeArguments;
            SchemaContext = schemaContext;
        }

        public XamlType(Type underlyingType, XamlSchemaContext schemaContext)
        {
            Name = underlyingType.Name;
            TypeArguments = underlyingType.GetGenericArguments().Select(x => new XamlType(x, schemaContext)).ToList();
            this.underlyingType = underlyingType;
            SchemaContext = schemaContext;
        }

        public XamlType? BaseType => underlyingType.BaseType != null ? new(underlyingType.BaseType, SchemaContext) : null;

        public string Name { get; }

        public string PreferredXamlNamespace => SchemaContext.GetAllXamlNamespaces().First();

        public IList<XamlType> TypeArguments { get; }

        public XamlSchemaContext SchemaContext { get; }

        public Type UnderlyingType => underlyingType;

        protected virtual Type LookupUnderlyingType()
        {
            return underlyingType;
        }
    }
}