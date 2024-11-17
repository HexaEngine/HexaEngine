namespace HexaEngine.Editor.UI
{
    using Hexa.NET.ImGui;
    using HexaEngine.Graphics.Renderers;
    using System;
    using System.Numerics;

    public class TitleBarBuilder
    {
        private readonly List<ITitleBarElement> elements = new();
        private int rightAlignAfter;
        private float buttonSize = 50;
        private int titleBarHeight = 30;

        public int Height { get => titleBarHeight; set => titleBarHeight = value; }

        public float ButtonSize { get => buttonSize; set => buttonSize = value; }

        public int RightAlignAfter { get => rightAlignAfter; set => rightAlignAfter = value; }

        public List<ITitleBarElement> Elements => elements;

        public TitleBarBuilder AddButton(string label, uint hoveredColor, uint activeColor, object? userdata, TitleBarButtonCallback callback, Func<object?, bool>? isVisible = null)
        {
            elements.Add(new TitleBarButton(label, hoveredColor, activeColor, new Vector2(buttonSize, titleBarHeight), userdata, callback) { isVisible = isVisible });
            return this;
        }

        public TitleBarBuilder InsertButton(string before, string label, uint hoveredColor, uint activeColor, object? userdata, TitleBarButtonCallback callback)
        {
            TitleBarButton button = new(label, hoveredColor, activeColor, new Vector2(buttonSize, titleBarHeight), userdata, callback);
            int idx = elements.FindIndex(x => x.Label == before);
            if (idx == -1)
            {
                idx = elements.Count;
            }

            elements.Insert(idx, button);
            return this;
        }

        public TitleBarBuilder AddTitle()
        {
            elements.Add(new TitleBarTitle());
            return this;
        }

        public TitleBarBuilder AddElement(ITitleBarElement element)
        {
            elements.Add(element);
            return this;
        }

        public TitleBarBuilder InsertElement(string before, ITitleBarElement element)
        {
            int idx = elements.FindIndex(x => x.Label == before);
            if (idx == -1)
            {
                idx = elements.Count;
            }
            elements.Insert(idx, element);
            return this;
        }

        public TitleBarBuilder RightAlign()
        {
            rightAlignAfter = elements.Count - 1;
            return this;
        }

        public TitleBarBuilder RightAlign(string before)
        {
            int idx = elements.FindIndex(x => x.Label == before);
            if (idx == -1)
            {
                idx = elements.Count;
            }
            rightAlignAfter = idx;
            return this;
        }

        public void Draw(TitleBarContext context)
        {
            ImGuiManager.PushFont("WidgetsFont");
            var bg = ImGui.GetBackgroundDrawList();
            bg.AddRectFilled(context.Area.Min, context.Area.Max, context.BackgroundColor);

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                if (element.IsVisible)
                {
                    element.Draw(context);
                }

                if (i == rightAlignAfter)
                {
                    AlignContext(context);
                }
            }
            ImGuiManager.PopFont();
        }

        public void AlignContext(TitleBarContext context)
        {
            float x = 0;
            for (int i = rightAlignAfter + 1; i < elements.Count; i++)
            {
                if (elements[i].IsVisible)
                {
                    x += elements[i].Size.X;
                }
            }
            context.Cursor = new(context.Area.Max.X - x, context.Cursor.Y);
        }

        public float ComputeLeftContentSize()
        {
            float x = 0;
            for (int i = 0; i < rightAlignAfter; i++)
            {
                if (elements[i].IsVisible)
                {
                    x += elements[i].Size.X;
                }
            }
            return x;
        }

        public float ComputeRightContentSize()
        {
            float x = 0;
            for (int i = rightAlignAfter + 1; i < elements.Count; i++)
            {
                if (elements[i].IsVisible)
                {
                    x += elements[i].Size.X;
                }
            }
            return x;
        }

        public void ComputePadding(out float left, out float right)
        {
            left = ComputeLeftContentSize();
            right = ComputeRightContentSize();
        }
    }
}