namespace ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using ImagePainter.Tools;
    using ImGuiNET;

    public class Toolbox : ImGuiWindow
    {
        private readonly List<Tool> tools = new();
        private Tool? current;

        public Toolbox()
        {
            Register<Pencil>();
            Register<Eraser>();
        }

        protected override string Name => "Toolbox";

        public Tool? Current { get => current; private set => current = value; }

        public override void Init(IGraphicsDevice device)
        {
            for (int i = 0; i < tools.Count; i++)
            {
                tools[i].Init(device);
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < tools.Count; i++)
            {
                tools[i].Dispose();
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            for (int i = 0; i < tools.Count; i++)
            {
                var tool = tools[i];
                var isActive = tool == current;
                if (isActive)
                    ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));

                if (ImGui.Button(tool.Icon))
                {
                    current = tool;
                }

                if (isActive)
                    ImGui.PopStyleColor();

                if (i % 4 != 0 || i == 0)
                {
                    ImGui.SameLine();
                }
            }
        }

        public void Register<T>(T tool) where T : Tool
        {
            lock (tools)
            {
                tools.Add(tool);
            }
        }

        public void Register<T>() where T : Tool, new()
        {
            lock (tools)
            {
                tools.Add(new T());
            }
        }

        public bool Unregister<T>(T tool) where T : Tool
        {
            lock (tools)
            {
                return tools.Remove(tool);
            }
        }

        public bool IsRegistered<T>(T tool) where T : Tool
        {
            lock (tools)
            {
                return tools.Contains(tool);
            }
        }
    }
}