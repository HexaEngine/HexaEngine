namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes.Textures;
    using Newtonsoft.Json;

    public abstract class OutputNode : Node, ITypedNode
    {
        protected OutputNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0x31762CFF.RGBAToVec4();
            TitleHoveredColor = 0x4D9648FF.RGBAToVec4();
            TitleSelectedColor = 0x6FB269FF.RGBAToVec4();
        }

        [JsonIgnore]
        public SType Type { get; protected set; }
    }
}