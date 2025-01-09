namespace HexaEngine.Editor.Extensions
{
    using Hexa.NET.Utilities.Text;

    public static class StrBuilderExtensions
    {
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

        public static StrBuilder BuildId(this ref StrBuilder builder, ReadOnlySpan<byte> id)
        {
            builder.Reset();
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }
    }
}