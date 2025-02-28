namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes.Textures;

    public abstract class InputNode : Node
    {
        protected InputNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0xC82A2AFF.RGBAToVec4();
            TitleHoveredColor = 0xDA3C3CFF.RGBAToVec4();
            TitleSelectedColor = 0xE04949FF.RGBAToVec4();
        }
    }
}