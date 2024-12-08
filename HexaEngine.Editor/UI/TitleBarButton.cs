namespace HexaEngine.Editor.UI
{
    using Hexa.NET.ImGui;
    using HexaEngine;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Extensions;
    using System;
    using System.Numerics;
    using System.Text;
    using static Hexa.NET.Utilities.Utils;

    public unsafe class TitleBarButton : TitleBarElement
    {
        public string label;
        public uint HoveredColor;
        public uint ActiveColor;
        public Vector2 size;
        public object? Userdata;
        public TitleBarButtonCallback Callback;
        public Func<object?, bool>? isVisible;

        public TitleBarButton(string label, uint hoveredColor, uint activeColor, Vector2 size, object? userdata, TitleBarButtonCallback callback)
        {
            this.label = label;
            HoveredColor = hoveredColor;
            ActiveColor = activeColor;
            this.size = size;
            Userdata = userdata;
            Callback = callback;
        }

        public override string Label => label;

        public override Vector2 Size => size;

        public override bool IsVisible => isVisible?.Invoke(Userdata) ?? true;

        public override void Draw(TitleBarContext context)
        {
            if (Button(context, label, HoveredColor, ActiveColor, size))
            {
                Callback(Userdata);
            }
        }

        private unsafe bool Button(TitleBarContext context, string label, uint hoveredColor, uint activeColor, Vector2 size)
        {
            int byteCount = Encoding.UTF8.GetByteCount(label);
            byte* pLabel;
            if (byteCount > StackAllocLimit)
            {
                pLabel = (byte*)Alloc(byteCount + 1);
            }
            else
            {
                byte* stackLabel = stackalloc byte[byteCount + 1];
                pLabel = stackLabel;
            }
            int offset = Encoding.UTF8.GetBytes(label, new Span<byte>(pLabel, byteCount));
            pLabel[offset] = 0;

            bool result = Button(context, pLabel, hoveredColor, activeColor, size);

            if (byteCount > StackAllocLimit)
            {
                Free(pLabel);
            }

            return result;
        }

        private bool Button(TitleBarContext context, ReadOnlySpan<byte> label, uint hoveredColor, uint activeColor, Vector2 size)
        {
            fixed (byte* pLabel = label)
            {
                return Button(context, pLabel, hoveredColor, activeColor, size);
            }
        }

        private unsafe bool Button(TitleBarContext context, byte* label, uint hoveredColor, uint activeColor, Vector2 size)
        {
            var id = ImGui.GetID(label);
            var mousePos = Mouse.Global;
            // Draw a custom close button on the right side of the title bar
            var pos = context.Cursor;

            var transitionState = ImGui.GetStateStorage().GetFloatRef(id, 0);

            context.Cursor += new Vector2(size.X, 0);

            ImRect rect = new(pos, pos + size);

            bool isHovered = rect.Contains(mousePos);
            bool isMouseDown = ImGuiP.IsMouseDown(ImGuiMouseButton.Left) && isHovered;

            if (!isHovered && context.HoveredId == id)
            {
                context.HoveredId = 0;
                *transitionState = 0.1f;
            }

            if (isHovered && context.HoveredId != id)
            {
                context.HoveredId = id;
                *transitionState = 0.1f;
            }

            if (*transitionState > 0)
            {
                *transitionState = *transitionState - Time.Delta;
            }
            if (*transitionState <= 0)
            {
                *transitionState = 0;
            }

            uint color;
            if (*transitionState != 0)
            {
                float s = *transitionState / 0.1f;
                uint colA = hoveredColor;
                uint colB = 0xFF;
                if (!isHovered)
                {
                    s = 1 - s;
                }
                color = ABGRLerp(colA, colB, s);
            }
            else
            {
                color = isMouseDown ? activeColor : isHovered ? hoveredColor : 0;
            }

            if (color != 0)
            {
                context.DrawList.AddRectFilled(rect.Min, rect.Max, color);
            }

            bool clicked = ImGuiP.IsMouseReleased(ImGuiMouseButton.Left);
            var textSizeClose = ImGui.CalcTextSize(label);
            textSizeClose.Y = ImGui.GetTextLineHeight() - ImGui.GetStyle().ItemSpacing.Y;
            var midpoint = rect.Midpoint() - textSizeClose / 2f;
            context.DrawList.AddText(midpoint, context.ForegroundColor, label);

            if (isHovered && clicked)
            {
                return true;
            }

            return false;
        }
    }
}