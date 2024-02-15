namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using System;

    public class EditorWindowCategory(string name) : IEquatable<EditorWindowCategory>
    {
        public string Name = name;
        public List<IEditorWindow> Windows = new();
        public ImGuiName UIName = new(name);

        public int Count => Windows.Count;

        public IEditorWindow this[int index]
        {
            get => Windows[index];
            set => Windows[index] = value;
        }

        public void Add(IEditorWindow window)
        {
            Windows.Add(window);
        }

        public void Remove(IEditorWindow window)
        {
            Windows.Remove(window);
        }

        public bool Contains(IEditorWindow window)
        {
            return Windows.Contains(window);
        }

        public void Clear()
        {
            Windows.Clear();
        }

        public void DrawMenu()
        {
            if (Name == string.Empty)
            {
                for (int i = 0; i < Windows.Count; i++)
                {
                    Windows[i].DrawMenu();
                }
            }
            else if (ImGui.BeginMenu(UIName.UniqueName))
            {
                for (int i = 0; i < Windows.Count; i++)
                {
                    Windows[i].DrawMenu();
                }
                ImGui.EndMenu();
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is EditorWindowCategory category && Equals(category);
        }

        public bool Equals(EditorWindowCategory? other)
        {
            return Name == other?.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(EditorWindowCategory left, EditorWindowCategory right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EditorWindowCategory left, EditorWindowCategory right)
        {
            return !(left == right);
        }
    }
}