namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class InputNode : Node
    {
        public InputNode(List<(string, PinType)> values, NodeEditor graph, string name, bool removable, bool isStatic) : base(graph, name, removable, isStatic)
        {
            for (int i = 0; i < values.Count; i++)
            {
            }
        }
    }
}