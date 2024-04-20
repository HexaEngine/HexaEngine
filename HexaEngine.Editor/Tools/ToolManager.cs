namespace HexaEngine.Editor.Tools
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.TextEditor;
    using HexaEngine.Scenes;
    using System.Threading.Tasks;

    public static class ToolManager
    {
        private static readonly List<ITool> tools = [];

        static ToolManager()
        {
            AddTool(new AsyncTool("Scene Editor", ".hexlvl", path => SceneManager.AsyncLoad(path, SceneInitFlags.SkipOnLoadWait)));
            AddTool(new Tool("Image Painter", ".dds|.png|.jpg|.ico|.bmp|.tga|.hdr", path =>
            {
                if (WindowManager.TryGetWindow<ImagePainterWindow>(out var imagePainterWindow))
                {
                    imagePainterWindow.Open(path);
                    imagePainterWindow.Focus();
                }
            }));

            AddTool(new Tool("Material Editor", ".material", path =>
            {
                if (WindowManager.TryGetWindow<MaterialEditorWindow>(out var materialEditorWindow))
                {
                    var metadata = SourceAssetsDatabase.GetMetadata(path);
                    if (metadata == null)
                    {
                        return;
                    }

                    var artifactGUID = ArtifactDatabase.GetArtifactForSource(metadata.Guid)?.Guid ?? default;
                    if (artifactGUID == default)
                    {
                        return;
                    }

                    materialEditorWindow.Material = artifactGUID;
                    materialEditorWindow.Focus();
                }
            }));

            AddTool(new Tool("Text Editor", ".txt|.cs|.c|.cpp|.h|.hpp|.hlsl|.glsl|.hlsli|.svg|.sln|.csproj|.gitignore|.md", path =>
            {
                if (WindowManager.TryGetWindow<TextEditorWindow>(out var textEditorWindow))
                {
                    textEditorWindow.Open(path);
                    textEditorWindow.Focus();
                }
            }));
        }

        public static IReadOnlyList<ITool> Tools => tools;

        public static ITool? GetTool(string path)
        {
            for (var i = 0; i < tools.Count; i++)
            {
                var tool = tools[i];
                if (tool.CanOpen(path))
                {
                    return tool;
                }
            }
            return null;
        }

        public static ITool? GetToolByName(string name)
        {
            for (var i = 0; i < tools.Count; i++)
            {
                var tool = tools[i];
                if (tool.Name == name)
                {
                    return tool;
                }
            }
            return null;
        }

        public static void AddTool(ITool tool)
        {
            tools.Add(tool);
        }

        public static void AddTool<T>() where T : ITool, new()
        {
            T tool = new();
            tools.Add(tool);
        }

        public static void RemoveTool(ITool tool)
        {
            tools.Remove(tool);
        }

        public static Task Open(string path)
        {
            var tool = GetTool(path);
            if (tool == null)
            {
                return Task.CompletedTask;
            }

            return tool.Open(path);
        }
    }
}