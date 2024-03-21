namespace SchemaExporter
{
    using HexaEngine;
    using HexaEngine.UI;
    using System.Reflection;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlSchemas schemas = new XmlSchemas();
            XmlSchemaExporter exporter = new(schemas);

            XmlReflectionImporter importer = new();

            Assembly assembly = Assembly.GetAssembly(typeof(Platform));

            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (type.Namespace != null && type.Namespace.StartsWith("HexaEngine.UI") && typeof(UIElement).IsAssignableFrom(type))
                {
                    exporter.ExportTypeMapping(importer.ImportTypeMapping(type));
                }
            }

            using (TextWriter writer = new StreamWriter("output.xsd"))
            {
                foreach (XmlSchema schema in schemas)
                {
                    schema.Write(writer);
                }
            }

            Console.WriteLine("XSD generated successfully.");
        }
    }
}