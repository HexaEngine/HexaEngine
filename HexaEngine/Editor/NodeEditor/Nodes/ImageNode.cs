namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Core.Graphics;
    using ImGuiNET;
    using System.Numerics;

    public class ImageNode : Node
    {
        private IShaderResourceView? image;
        private nint imgId;

        public ImageNode(NodeEditor graph, string name, bool removable, bool isStatic) : base(graph, name, removable, isStatic)
        {
            CreatePin("out Image", PinKind.Output, PinType.Texture2D, ImNodesNET.PinShape.Quad);
        }

        public ImageNode(NodeEditor graph, string name, PinType type, bool removable, bool isStatic) : base(graph, name, removable, isStatic)
        {
            CreatePin("out Image", PinKind.Output, type, ImNodesNET.PinShape.Quad);
        }

        public IShaderResourceView? Image
        {
            get => image;
            set
            {
                if (value == null)
                {
                    image = value;
                }
            }
        }

        public Vector2 Size = new(128, 128);

        protected override void DrawContent()
        {
            ImGui.Image(imgId, Size);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }

    public class ImageCubeNode : Node
    {
        private IShaderResourceView? image;
        private nint imgId;

        public ImageCubeNode(NodeEditor graph, string name, bool removable, bool isStatic) : base(graph, name, removable, isStatic)
        {
            CreatePin("out Image", PinKind.Output, PinType.TextureCube, ImNodesNET.PinShape.Quad);
        }

        public IShaderResourceView? Image
        {
            get => image;
            set
            {
                if (value == null)
                {
                    image = value;
                }
            }
        }

        public Vector2 Size = new(128, 128);

        protected override void DrawContent()
        {
            //ImGui.Image(imgId, Size);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}