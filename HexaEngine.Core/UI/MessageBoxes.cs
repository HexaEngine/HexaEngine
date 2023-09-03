namespace HexaEngine.Core.UI
{
    using System.Collections.Generic;

    public static class MessageBoxes
    {
        private static readonly List<MessageBox> messageBoxes = new();

        public static void Draw()
        {
            lock (messageBoxes)
            {
                for (int i = 0; i < messageBoxes.Count; i++)
                {
                    MessageBox box = messageBoxes[i];
                    if (box.Draw())
                    {
                        messageBoxes.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public static void Show(MessageBox messageBox)
        {
            lock (messageBoxes)
            {
                messageBoxes.Add(messageBox);
            }
        }

        public static bool Close(string title)
        {
            lock (messageBoxes)
            {
                for (int i = 0; i < messageBoxes.Count; i++)
                {
                    var box = messageBoxes[i];
                    if (box.Title == title)
                    {
                        messageBoxes.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}