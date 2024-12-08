﻿namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using System;

    /// <summary>
    /// Represents a message box that can be displayed to the user.
    /// </summary>
    public struct MessageBox
    {
        /// <summary>
        /// Gets or sets the title of the message box.
        /// </summary>
        public string Title;

        /// <summary>
        /// Gets or sets the message text to be displayed in the message box.
        /// </summary>
        public string Message;

        /// <summary>
        /// Gets the type of the message box, which determines the available actions.
        /// </summary>
        public MessageBoxType Type;

        private bool shown;

        /// <summary>
        /// Gets or sets the result of the message box (e.g., user response).
        /// </summary>
        public MessageBoxResult Result;

        /// <summary>
        /// Gets or sets optional user data associated with the message box.
        /// </summary>
        public object? Userdata;

        /// <summary>
        /// Gets or sets an optional callback action to be invoked when the message box is closed.
        /// </summary>
        public Action<MessageBox, object?>? Callback;

        /// <summary>
        /// Initializes a new instance of the MessageBox struct with the provided parameters.
        /// </summary>
        /// <param name="title">The title of the message box.</param>
        /// <param name="message">The message text to be displayed.</param>
        /// <param name="type">The type of the message box.</param>
        /// <param name="userdata">Optional user data associated with the message box.</param>
        /// <param name="callback">Optional callback action to be invoked when the message box is closed.</param>
        public MessageBox(string title, string message, MessageBoxType type, object? userdata = null, Action<MessageBox, object?>? callback = null)
        {
            Title = title;
            Message = message;
            Type = type;
            Userdata = userdata;
            Callback = callback;
        }

        /// <summary>
        /// Shows a message box with the specified title, message, and type.
        /// </summary>
        /// <param name="title">The title of the message box.</param>
        /// <param name="message">The message text to be displayed.</param>
        /// <param name="type">The type of the message box.</param>
        /// <returns>The created message box instance.</returns>
        public static MessageBox Show(string title, string message, MessageBoxType type = MessageBoxType.Ok)
        {
            MessageBox box = new(title, message, type);
            MessageBoxes.Show(box);
            return box;
        }

        /// <summary>
        /// Shows a message box with the specified title, message, user data, callback, and type.
        /// </summary>
        /// <param name="title">The title of the message box.</param>
        /// <param name="message">The message text to be displayed.</param>
        /// <param name="userdata">Optional user data associated with the message box.</param>
        /// <param name="callback">Optional callback action to be invoked when the message box is closed.</param>
        /// <param name="type">The type of the message box.</param>
        /// <returns>The created message box instance.</returns>
        public static MessageBox Show(string title, string message, object? userdata, Action<MessageBox, object?> callback, MessageBoxType type = MessageBoxType.Ok)
        {
            MessageBox box = new(title, message, type, userdata, callback);
            MessageBoxes.Show(box);
            return box;
        }

        /// <summary>
        /// Draws the message box and handles user interactions.
        /// </summary>
        /// <returns>True if the message box is still shown; otherwise, false.</returns>
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

            ImGuiP.SetWindowPos(ImGui.GetIO().DisplaySize * 0.5f, ImGuiCond.Appearing);

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