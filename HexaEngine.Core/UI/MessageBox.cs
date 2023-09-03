namespace HexaEngine.Core.UI
{
    using ImGuiNET;
    using System;

    public struct MessageBox
    {
        public string Title;
        public string Message;
        public MessageBoxType Type;
        private bool shown;

        public MessageBoxResult Result;
        public object? Userdata;
        public Action<MessageBox, object?>? Callback;

        public MessageBox(string title, string message, MessageBoxType type, object? userdata = null, Action<MessageBox, object?>? callback = null)
        {
            Title = title;
            Message = message;
            Type = type;
            Userdata = userdata;
            Callback = callback;
        }

        public static MessageBox Show(string title, string message, MessageBoxType type = MessageBoxType.Ok)
        {
            MessageBox box = new(title, message, type);
            MessageBoxes.Show(box);
            return box;
        }

        public static MessageBox Show(string title, string message, object? userdata, Action<MessageBox, object?> callback, MessageBoxType type = MessageBoxType.Ok)
        {
            MessageBox box = new(title, message, type, userdata, callback);
            MessageBoxes.Show(box);
            return box;
        }

        public bool Draw()
        {
            if (!shown)
            {
                ImGui.OpenPopup(Title);
                shown = true;
            }

            bool open = true;
            if (!ImGui.BeginPopupModal(Title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                return shown;
            }

            ImGui.SetWindowPos(ImGui.GetIO().DisplaySize * 0.5f, ImGuiCond.Appearing);

            ImGui.Text(Message);

            ImGui.Separator();

            switch (Type)
            {
                case MessageBoxType.Ok:
                    if (ImGui.Button("Ok"))
                    {
                        open = false;
                        Result = MessageBoxResult.Ok;
                    }
                    break;

                case MessageBoxType.OkCancel:
                    if (ImGui.Button("Ok"))
                    {
                        open = false;
                        Result = MessageBoxResult.Ok;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        open = false;
                        Result = MessageBoxResult.Cancel;
                    }
                    break;

                case MessageBoxType.YesCancel:
                    if (ImGui.Button("Yes"))
                    {
                        open = false;
                        Result = MessageBoxResult.Yes;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        open = false;
                        Result = MessageBoxResult.Cancel;
                    }
                    break;

                case MessageBoxType.YesNo:
                    if (ImGui.Button("Yes"))
                    {
                        open = false;
                        Result = MessageBoxResult.Yes;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        open = false;
                        Result = MessageBoxResult.No;
                    }
                    break;

                case MessageBoxType.YesNoCancel:
                    if (ImGui.Button("Yes"))
                    {
                        open = false;
                        Result = MessageBoxResult.Yes;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        open = false;
                        Result = MessageBoxResult.No;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        open = false;
                        Result = MessageBoxResult.Cancel;
                    }
                    break;
            }

            if (!open)
            {
                Callback?.Invoke(this, Userdata);
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();

            return !open;
        }
    }
}