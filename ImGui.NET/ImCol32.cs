namespace ImGuiNET
{
    using System.Numerics;

    public struct ImCol32
    {
        public uint Value;

        public const int IM_COL32_R_SHIFT = 0;
        public const int IM_COL32_G_SHIFT = 8;
        public const int IM_COL32_B_SHIFT = 16;
        public const int IM_COL32_A_SHIFT = 24;

        public const uint IM_COL32_A_MASK = 0xFF000000;

        public static uint Pack(byte r, byte g, byte b, byte a)
        {
            return (uint)((a << IM_COL32_A_SHIFT) | (b << IM_COL32_B_SHIFT) | (g << IM_COL32_G_SHIFT) | (r << IM_COL32_R_SHIFT));
        }

        public ImCol32(uint rgba)
        {
            byte r = (byte)((rgba & 0xFF000000) >> 24);
            byte g = (byte)((rgba & 0x00FF0000) >> 16);
            byte b = (byte)((rgba & 0x0000FF00) >> 8);
            byte a = (byte)(rgba & 0x000000FF);
            Value = Pack(r, g, b, a);
        }

        public ImCol32(Vector4 color)
        {
            Value = Pack((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255), (byte)(color.W * 255));
        }

        public ImCol32(Vector3 color, float alpha)
        {
            Value = Pack((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255), (byte)(alpha * 255));
        }

        public ImCol32(float r, float g, float b, float a)
        {
            Value = Pack((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
        }

        public ImCol32(uint r, uint g, uint b, uint a)
        {
            Value = Pack((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static implicit operator uint(ImCol32 v)
        {
            return v.Value;
        }
    }
}