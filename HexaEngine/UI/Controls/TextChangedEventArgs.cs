namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core.Windows.Events;

    public class TextChangedEventArgs : RoutedEventArgs
    {
        public TextChangedEventArgs(ICollection<TextChange> changes, UndoAction undoAction)
        {
            Changes = changes;
            UndoAction = undoAction;
        }

        public TextChangedEventArgs(RoutedEvent routedEvent, ICollection<TextChange> changes, UndoAction undoAction) : base(routedEvent)
        {
            Changes = changes;
            UndoAction = undoAction;
        }

        public ICollection<TextChange> Changes { get; }

        public UndoAction UndoAction { get; }
    }
}