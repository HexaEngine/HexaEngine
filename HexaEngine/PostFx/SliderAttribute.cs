namespace HexaEngine.PostFx
{
    using Hexa.NET.ImGui;
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SliderAttribute : Attribute
    {
        public SliderAttribute(uint min, uint max)
        {
            UIntMin = min;
            UIntMax = max;
        }

        public SliderAttribute(int min, int max)
        {
            IntMin = min;
            IntMax = max;
        }

        public SliderAttribute(float min, float max)
        {
            FloatMin = min;
            FloatMax = max;
        }

        public ImGuiDataType DataType { get; }

        public uint UIntMin { get; }

        public uint UIntMax { get; }

        public int IntMin { get; }

        public int IntMax { get; }

        public float FloatMin { get; }

        public float FloatMax { get; }
    }
}