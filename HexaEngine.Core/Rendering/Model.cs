namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;

    public class Material
    {
        public GraphicsPipelineDesc GraphicsPipelineDesc;
        public GraphicsPipelineState State;
    }

    public class Mesh
    {
    }

    public class Model
    {
        public Mesh[] Meshes;
        public Material[] Materials;
    }
}