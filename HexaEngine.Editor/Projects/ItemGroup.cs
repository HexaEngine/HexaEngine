namespace HexaEngine.Editor.Projects
{
    using System.Collections;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class ItemGroup : HexaProjectItem, IList<IItemGroupItem>, IXmlSerializable
    {
        private readonly List<IItemGroupItem> items = new();

        public IItemGroupItem this[int index] { get => items[index]; set => items[index] = value; }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public override XmlSchema? GetSchema()
        {
            return null;
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("ItemGroup");
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "ItemGroup")
                    break;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    var element = ItemRegistry.CreateNew(reader.Name);
                    element.ReadXml(reader);
                    items.Add(element);
                }
            }
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ItemGroup");
            foreach (var item in items)
            {
                writer.WriteStartElement(item.Name);
                item.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public void Add(IItemGroupItem item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(IItemGroupItem item)
        {
            return items.Contains(item);
        }

        public void CopyTo(IItemGroupItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IItemGroupItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public IEnumerable<T> GetAll<T>() where T : class, IItemGroupItem
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is T t)
                {
                    yield return t;
                }
            }
        }

        public int IndexOf(IItemGroupItem item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, IItemGroupItem item)
        {
            items.Insert(index, item);
        }

        public bool Remove(IItemGroupItem item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}