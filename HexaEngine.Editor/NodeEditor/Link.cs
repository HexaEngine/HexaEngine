namespace HexaEngine.Editor.NodeEditor
{
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;

    public class Link
    {
        private readonly Node outputNode;
        private readonly Pin output;
        private readonly Node inputNode;
        private readonly Pin input;

        private NodeEditor? editor;
        private readonly int id;
        private int outputId;
        private int inputId;

        public Link(int id, Node outputNode, Pin output, Node inputNode, Pin input)
        {
            this.id = id;
            this.outputNode = outputNode;
            this.output = output;
            outputId = output.Id;
            this.inputNode = inputNode;
            this.input = input;
            inputId = input.Id;
        }

        [JsonIgnore]
        public int Id => id;

        [JsonIgnore]
        public Node OutputNode => outputNode;

        [JsonIgnore]
        public Pin Output => output;

        [JsonIgnore]
        public Node InputNode => inputNode;

        [JsonIgnore]
        public Pin Input => input;

        public LinkId GetId()
        {
            return new() { Id = id, IdInputNode = InputNode.Id, IdOutputNode = OutputNode.Id, IdInput = Input.Id, IdOutput = Output.Id };
        }

        public void Draw()
        {
            ImNodes.Link(id, outputId, inputId);
        }

        public virtual void Destroy()
        {
            if (editor == null)
            {
                return;
            }

            editor.RemoveLink(this);
            OutputNode.RemoveLink(this);
            Output.RemoveLink(this);
            InputNode.RemoveLink(this);
            Input.RemoveLink(this);
            editor = null;
        }

        public virtual void Initialize(NodeEditor editor)
        {
            this.editor = editor;

            outputId = output.Id;
            inputId = input.Id;

            OutputNode.AddLink(this);
            Output.AddLink(this);
            InputNode.AddLink(this);
            Input.AddLink(this);
        }
    }
}