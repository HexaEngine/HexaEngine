namespace HexaEngine.Editor.Dialogs
{
    using ImGuiNET;

    public enum MessageBoxResult
    {
        Ok,
        Yes,
        No,
        Cancel,
    }

    public enum MessageBoxFlags
    {
        Ok,
        YesNo,
        Cancel,
    }

    public class MessageBox : DialogBase
    {
        private readonly MessageBoxFlags flags;

        public MessageBox(string message, MessageBoxFlags flags)
        {
            Message = message;
            this.flags = flags;
            Title = string.Empty;
        }

        public MessageBox(string title, string message, MessageBoxFlags flags)
        {
            this.flags = flags;
            Title = title;
            Message = message;
        }

        public string Message { get; }

        public string Title { get; }

        public MessageBoxResult Result { get; private set; }

        public override string Name => Title;

        protected override ImGuiWindowFlags Flags { get; }

        public override void Reset()
        {
        }

        protected override void DrawContent()
        {
            ImGui.Text(Message);
            switch (flags)
            {
                case MessageBoxFlags.Ok:
                    if (ImGui.Button("Ok"))
                    {
                        Result = MessageBoxResult.Ok;
                        Hide();
                    }
                    break;

                case MessageBoxFlags.YesNo:
                    if (ImGui.Button("No"))
                    {
                        Result = MessageBoxResult.No;
                        Hide();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Yes"))
                    {
                        Result = MessageBoxResult.Yes;
                        Hide();
                    }
                    break;

                case MessageBoxFlags.Cancel:
                    if (ImGui.Button("Cancel"))
                    {
                        Result = MessageBoxResult.Cancel;
                        Hide();
                    }
                    break;
            }
        }
    }
}