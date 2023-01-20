namespace HexaEngine.Editor.Dialogs
{
    public interface IDialog
    {
        bool Shown { get; }

        void Draw();
        void Hide();
        void Reset();
        void Show();
    }
}