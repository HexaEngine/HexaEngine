namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;

    public class SplitNodeRenderer : BaseNodeRenderer<SplitNode>
    {
        private static readonly PinType[] modes;
        private static readonly string[] names;

        static SplitNodeRenderer()
        {
            modes = [PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4];
            names = modes.Select(x => x.ToString()).ToArray();
        }

        protected override void DrawContentBeforePins(SplitNode node)
        {
            ImGui.PushItemWidth(100);
            if (ImGui.Combo("##Mode", ref node.item, names, names.Length))
            {
                node.mode = modes[node.item];
                node.UpdateMode();
            }
            ImGui.PopItemWidth();
        }
    }
}