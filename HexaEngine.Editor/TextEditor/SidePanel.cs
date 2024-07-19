using HexaEngine.Core;

namespace HexaEngine.Editor.TextEditor
{
    public abstract class SidePanel : DisposableBase
    {
        public abstract string Icon { get; }

        public abstract string Title { get; }

        public void Draw()
        {
            DrawContent();
        }

        public abstract void DrawContent();

        protected override void DisposeCore()
        {
        }
    }
}