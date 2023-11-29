namespace HexaEngine.Editor.LensEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System.Numerics;

    public abstract class LensEffectBase : ILensEffect
    {
        protected ImGuiName name;

        public LensEffectBase()
        {
            name = new ImGuiName(Name);
        }

        public abstract string Name { get; }

        public bool Enabled { get; set; } = true;

        protected string Id => name.RawId;

        public virtual bool Draw()
        {
            bool result = false;

            result |= DrawContent();

            return result;
        }

        public abstract bool DrawContent();

        public abstract void Generate(CodeWriter writer, int index);
    }
}