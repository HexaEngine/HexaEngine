namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PipelineEditor : ImGuiWindow
    {
        private NodeEditor editor = new();

        public PipelineEditor()
        {
            var output = editor.CreateNode("Output", false, false);
            output.CreatePin("Position", PinKind.Input, PinType.Vector4, ImNodesNET.PinShape.CircleFilled);
            output.CreatePin("Normal", PinKind.Input, PinType.Vector3, ImNodesNET.PinShape.CircleFilled);
            output.CreatePin("Tangent", PinKind.Input, PinType.Vector3, ImNodesNET.PinShape.CircleFilled);
            output.CreatePin("Color", PinKind.Input, PinType.Vector4, ImNodesNET.PinShape.QuadFilled);
            var input = editor.CreateNode("Input",false, false);
            input.CreatePin("Position", PinKind.Output, PinType.Vector4, ImNodesNET.PinShape.CircleFilled);
            input.CreatePin("TexCoord", PinKind.Output, PinType.Vector2, ImNodesNET.PinShape.CircleFilled);
            var texture = editor.CreateNode("BaseColor Texture", false, false);
            texture.CreatePin("Color", PinKind.Output, PinType.Vector4, ImNodesNET.PinShape.QuadFilled);
            texture.CreatePin("TexCoord", PinKind.Input, PinType.Vector2, ImNodesNET.PinShape.CircleFilled);
            var transform = editor.CreateNode("WorldViewProj", false, false);
            transform.CreatePin("In Position", PinKind.Input, PinType.Vector4, ImNodesNET.PinShape.CircleFilled);
            transform.CreatePin("Out Position", PinKind.Output, PinType.Vector4, ImNodesNET.PinShape.CircleFilled);
            
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name => "Pipeline editor";

        public override void DrawContent(IGraphicsContext context)
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Add"))
                {

                }
                ImGui.EndMenuBar();
            }
            editor.Draw();    
        }
    }
}
