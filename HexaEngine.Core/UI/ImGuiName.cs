namespace HexaEngine.Core.UI
{
    using System;

    public struct ImGuiName
    {
        public string RawId;
        public string Name;
        public string UniqueName;
        public string Id;

        public ImGuiName(string name)
        {
            RawId = Guid.NewGuid().ToString();
            Name = name;
            UniqueName = $"{Name}##{RawId}";
            Id = $"##{RawId}";
        }

        public static implicit operator string(ImGuiName name)
        {
            return name.UniqueName;
        }
    }
}