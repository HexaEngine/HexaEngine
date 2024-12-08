namespace HexaEngine.Editor.Projects
{
    using System.Xml.Serialization;

    public interface IItemGroupItem : IXmlSerializable
    {
        string Name { get; }
    }
}