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
        public uint VertexCount;
        public uint IndexCount;
    }

    public class Model
    {
#pragma warning disable CS8618 // Non-nullable field 'Meshes' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public Mesh[] Meshes;
#pragma warning restore CS8618 // Non-nullable field 'Meshes' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'Materials' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public Material[] Materials;
#pragma warning restore CS8618 // Non-nullable field 'Materials' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
    }
}