namespace HexaEngine.Editor.Dialogs
{
    public interface IDialog
    {
        bool Shown { get; }

        void Draw();
        void Close();
        void Reset();
        void Show();
    }
}