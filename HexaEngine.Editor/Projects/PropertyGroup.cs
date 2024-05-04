namespace HexaEngine.Editor.Projects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using System.Xml.Schema;

    public class PropertyGroup : HexaProjectItem, IList<PropertyGroupItem>
    {
        private readonly List<PropertyGroupItem> properties = [];
        private readonly Dictionary<string, PropertyGroupItem> nameToItem = new();

        public PropertyGroupItem this[int index] { get => properties[index]; set => properties[index] = value; }

        public PropertyGroupItem this[string index] { get => nameToItem[index]; set => nameToItem[index] = value; }

        public int Count => properties.Count;

        public bool IsReadOnly => false;

        public override XmlSchema? GetSchema()
        {
            return null;
        }

        public override void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                return;
            }

            reader.ReadStartElement("PropertyGroup");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "PropertyGroup")
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    string name = reader.Name; // Get the element name (which corresponds to PropertyGroupItem name)
                    string value = reader.ReadElementContentAsString(); // Get the value of the element

                    Add(new PropertyGroupItem(name, value));

                    // </PropertyGroupItem>
                }
            }

            reader.ReadEndElement(); // </PropertyGroup>
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("PropertyGroup");

            foreach (var property in properties)
            {
                writer.WriteElementString(property.Name, property.Value);
            }

            writer.WriteEndElement();
        }

        public PropertyGroupItem GetPropertyItem(string name)
        {
            return nameToItem[name];
        }

        public bool TryGetPropertyItem(string name, [NotNullWhen(true)] out PropertyGroupItem? value)
        {
            return nameToItem.TryGetValue(name, out value);
        }

        public string GetProperty(string name)
        {
            return nameToItem[name].Value;
        }

        public bool TryGetProperty(string name, [NotNullWhen(true)] out string? value)
        {
            if (nameToItem.TryGetValue(name, out var item))
            {
                value = item.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void SetProperty(string name, string value)
        {
            nameToItem[name].Value = value;
        }

        public void AddProperty(string name, string value)
        {
            PropertyGroupItem item = new(name, value);
            properties.Add(item);
            nameToItem.Add(name, item);
        }

        public bool HasProperty(string name)
        {
            return nameToItem.ContainsKey(name);
        }

        public void Add(PropertyGroupItem item)
        {
            properties.Add(item);
            nameToItem.Add(item.Name, item);
        }

        public void Clear()
        {
            properties.Clear();
            nameToItem.Clear();
        }

        public bool Contains(PropertyGroupItem item)
        {
            return properties.Contains(item);
        }

        public void CopyTo(PropertyGroupItem[] array, int arrayIndex)
        {
            properties.CopyTo(array, arrayIndex);
        }

        public IEnumerator<PropertyGroupItem> GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        public int IndexOf(PropertyGroupItem item)
        {
            return properties.IndexOf(item);
        }

        public void Insert(int index, PropertyGroupItem item)
        {
            properties.Insert(index, item);
        }

        public bool Remove(PropertyGroupItem item)
        {
            return properties.Remove(item) && nameToItem.Remove(item.Name);
        }

        public void RemoveAt(int index)
        {
            var key = properties[index].Name;
            properties.RemoveAt(index);
            nameToItem.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return properties.GetEnumerator();
        }
    }
}