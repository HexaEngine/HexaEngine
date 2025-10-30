namespace HexaEngine.UI.Controls
{
    public class TextChange
    {
        public TextChange(int addedLength, int offset, int removedLength)
        {
            AddedLength = addedLength;
            Offset = offset;
            RemovedLength = removedLength;
        }

        public int AddedLength { get; }

        public int Offset { get; }

        public int RemovedLength { get; }
    }
}