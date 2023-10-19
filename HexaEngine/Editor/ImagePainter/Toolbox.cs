namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.ImagePainter.Tools;
    using ImGuiNET;

    public class Toolbox : EditorWindow
    {
        private readonly List<Tool> tools = new();
        private Tool? current;

        public Toolbox()
        {
            Register<Pencil>();
            Register<Eraser>();
            Register<ColorGrabber>();
        }

        protected override string Name => "Toolbox";

        public Tool? Current { get => current; private set => current = value; }

        protected override void InitWindow(IGraphicsDevice device)
        {
            for (int i = 0; i < tools.Count; i++)
            {
                tools[i].Init(device);
            }
        }

        protected override void DisposeCore()
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
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));
                }

                if (ImGui.Button(tool.Icon))
                {
                    current = tool;
                }

                if (isActive)
                {
                    ImGui.PopStyleColor();
                }

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