namespace HexaEngine.Editor.NodeEditor
{
    using ImNodesNET;

    public class Link
    {
        public readonly NodeEditor Graph;
        public readonly int Id;
        public readonly Node SourceNode;
        public readonly int SourceNodeId;
        public readonly Pin Output;
        public readonly int OutputId;
        public readonly Node TargetNode;
        public readonly int TargetNodeId;
        public readonly Pin Input;
        public readonly int InputId;
        private object? _value;

        public Link(NodeEditor graph, Node sourceNode, Pin output, Node targetNode, Pin input)
        {
            Graph = graph;
            Id = graph.GetUniqueId();
            SourceNode = sourceNode;
            SourceNodeId = sourceNode.Id;
            Output = output;
            OutputId = output.Id;
            TargetNode = targetNode;
            TargetNodeId = targetNode.Id;
            Input = input;
            InputId = input.Id;
            Output.ValueChanged += Output_ValueChanged;
        }

        private void Output_ValueChanged(object? sender, EventArgs e)
        {
            Value = Output.Value;
            Input.Value = Value;
        }

        public event EventHandler? ValueChanged;

        public object? Value
        {
            get => _value;
            set
            {
                _value = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Draw()
        {
            ImNodes.Link(Id, OutputId, InputId);
        }

        public virtual void Destroy()
        {
            Output.ValueChanged -= Output_ValueChanged;
            Graph.RemoveLink(this);
            SourceNode.RemoveLink(this);
            Output.RemoveLink(this);
            TargetNode.RemoveLink(this);
            Input.RemoveLink(this);
        }
    }
}