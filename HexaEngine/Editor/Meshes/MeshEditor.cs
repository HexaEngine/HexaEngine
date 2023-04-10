namespace HexaEngine.Editor.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Editor.Dialogs;
    using ImGuiNET;

    public class Mesh
    {
        public readonly IBuffer VB;
        public readonly IBuffer IB;
        public readonly uint Stride;

        public Mesh(IGraphicsDevice device, MeshData data)
        {
            VB = data.CreateVertexBuffer(device, Usage.Dynamic, CpuAccessFlags.Write);
            IB = data.CreateIndexBuffer(device, Usage.Dynamic, CpuAccessFlags.Write);
            Stride = data.GetStride();
        }
    }

    public class MeshEditor : ImGuiWindow
    {
        private readonly OpenFileDialog openDialog = new();

        private ModelFile model;

        protected override string Name => "Meshes";

        public MeshEditor()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;
        }

        private void DrawMenuBar()
        {
            if (!ImGui.BeginMenuBar())
            {
                ImGui.EndMenuBar();
                return;
            }

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open"))
                {
                    openDialog.Show();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }

        private void DrawWindows(IGraphicsContext context)
        {
            if (openDialog.Draw())
            {
                if (openDialog.Result == OpenFileResult.Ok)
                {
                    model = ModelFile.LoadExternal(openDialog.SelectedFile);
                    LoadModel();
                }
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            DrawMenuBar();
        }

        private void LoadModel()
        {
            model.GetMesh(0);
        }

        private void UnloadModel()
        {
        }
    }
}