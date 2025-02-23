namespace HexaEngine.Materials
{
    using HexaEngine.Core.Collections;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class NodeContainer
    {
        [JsonConstructor]
        public NodeContainer()
        {
        }

        public NodeContainer(NodeEditor editor)
        {
            for (int i = 0; i < editor.Nodes.Count; i++)
            {
                var node = editor.Nodes[i];
                Nodes.Add(node);
                var list = PinMap[i] = new();
                for (int j = 0; j < node.Pins.Count; j++)
                {
                    var pin = node.Pins[j];
                    list.Add(pin);
                }
            }
            for (int i = 0; i < editor.Links.Count; i++)
            {
                Links.Add(editor.Links[i].GetId());
            }
            State = editor.SaveState();
            CurrentId = editor.CurrentId;
        }

        public List<Node> Nodes { get; } = new();

        public Dictionary<int, List<Pin>> PinMap { get; } = new();

        public List<LinkId> Links { get; } = new();

        public string? State { get; set; }

        public int CurrentId { get; set; }

        public void Build(NodeEditor editor)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                editor.AddNode(node);
                var list = PinMap[i];
                for (int j = 0; j < list.Count; j++)
                {
                    node.AddPin(list[j]);
                }
            }
            editor.Initialize(false);
            editor.BeginUpdate();
            for (int i = 0; i < Links.Count; i++)
            {
                editor.CreateLinkFromId(Links[i]);
            }
            editor.EndUpdate();
            editor.RestoreState(State);
            editor.CurrentId = CurrentId;
        }
    }
}