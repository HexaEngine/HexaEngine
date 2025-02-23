namespace HexaEngine.Editor.MaterialEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using HexaEngine.Core;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Materials;

    public unsafe class BasePinRenderer : DisposableRefBase, IPinRenderer
    {
        public virtual void Draw(Pin pin)
        {
            switch (pin.Kind)
            {
                case PinKind.Input:
                    ImNodes.BeginInputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                    DrawContent(pin);
                    ImNodes.EndInputAttribute();
                    break;

                case PinKind.Output:
                    ImNodes.BeginOutputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                    DrawContent(pin);
                    ImNodes.EndOutputAttribute();
                    break;

                case PinKind.Static:
                    ImNodes.BeginStaticAttribute(pin.Id);
                    DrawContent(pin);
                    ImNodes.EndStaticAttribute();
                    break;

                case PinKind.InputOutput:
                    break;
            }
        }

        protected virtual void DrawContent(Pin pin)
        {
            ImGui.Text(pin.Name);
        }

        protected override void DisposeCore()
        {
        }
    }

    public class BasePinRenderer<T> : DisposableRefBase, IPinRenderer where T : Pin
    {
        public void Draw(Pin pin)
        {
            if (pin is T t)
            {
                Draw(t);
            }
        }

        public virtual void Draw(T pin)
        {
            if (pin.Kind == PinKind.Input)
            {
                ImNodes.BeginInputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                DrawContent(pin);
                ImNodes.EndInputAttribute();
            }
            if (pin.Kind == PinKind.Output)
            {
                ImNodes.BeginOutputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                DrawContent(pin);
                ImNodes.EndOutputAttribute();
            }
            if (pin.Kind == PinKind.Static)
            {
                ImNodes.BeginStaticAttribute(pin.Id);
                DrawContent(pin);
                ImNodes.EndStaticAttribute();
            }
        }

        protected virtual void DrawContent(T pin)
        {
            ImGui.Text(pin.Name);
        }

        protected override void DisposeCore()
        {
        }
    }
}