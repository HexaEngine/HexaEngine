namespace HexaEngine.Editor.Projects
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public interface IItemGroupItem : IXmlSerializable
    {
        string Name { get; }
    }
}