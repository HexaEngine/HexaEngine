namespace HexaEngine.Editor.Dialogs
{
    public interface IPopup
    {
        string Name { get; }

        bool Shown { get; }

        void Close();

        void Draw();

        void Reset();

        void Show();
    }
}