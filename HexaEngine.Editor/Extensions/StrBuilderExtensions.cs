namespace HexaEngine.Editor.Extensions
{
    using Hexa.NET.Utilities.Text;
    using System.Buffers.Binary;

    public static class StrBuilderExtensions
    {
        public static StrBuilder BuildLabel(this ref StrBuilder builder, ReadOnlySpan<byte> text)
        {
            builder.Reset();
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, string text)
        {
            builder.Reset();
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, ReadOnlySpan<byte> icon, ReadOnlySpan<byte> text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, ReadOnlySpan<byte> icon, string text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, string icon, ReadOnlySpan<byte> text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, string icon, string text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, char icon, ReadOnlySpan<byte> text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, char icon, string text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, char icon)
        {
            builder.Reset();
            builder.Append(icon);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, char icon, ReadOnlySpan<byte> id)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, char icon, int id)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, ReadOnlySpan<byte> text, ReadOnlySpan<byte> id)
        {
            builder.Reset();
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, ReadOnlySpan<byte> text, string id)
        {
            builder.Reset();
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, ReadOnlySpan<byte> text, int id)
        {
            builder.Reset();
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, string text, ReadOnlySpan<byte> id)
        {
            builder.Reset();
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, string text, string id)
        {
            builder.Reset();
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, char icon, ReadOnlySpan<byte> text, ReadOnlySpan<byte> id)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, char icon, string id)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, char icon, string text, ReadOnlySpan<byte> id)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, char icon, ReadOnlySpan<byte> text, string id)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabelId(this ref StrBuilder builder, char icon, string text, string id)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildId(this ref StrBuilder builder, string id)
        {
            builder.Reset();
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildId(this ref StrBuilder builder, int id)
        {
            builder.Reset();
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildId(this ref StrBuilder builder, ReadOnlySpan<byte> id)
        {
            builder.Reset();
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildId(this ref StrBuilder builder, int id0, int id1)
        {
            builder.Reset();
            builder.Append("##"u8);
            builder.Append(id0);
            builder.Append("_"u8);
            builder.Append(id1);
            builder.End();
            return builder;
        }

        public static unsafe void AppendHex(this ref StrBuilder builder, nint value, bool leadingZeros, bool uppercase)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }
    }
}