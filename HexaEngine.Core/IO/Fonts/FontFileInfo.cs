namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using System;

    public unsafe struct FontFileInfo : IEquatable<FontFileInfo>
    {
        public string Path;
        public string Family;
        public FontStyle Style;
        public FontWeight Weight;

        public FontFileInfo(string path, string family, FontStyle style, FontWeight weight)
        {
            Path = path;
            Family = family;
            Style = style;
            Weight = weight;
        }

        public static FontFileInfo ReadFrom(FTLibrary library, string path)
        {
            FTError error;
            FTFace faceHandle;
            error = (FTError)FreeType.NewFace(library, path, 0, &faceHandle);
            if (error != FTError.Ok)
            {
                throw new($"Failed to load font file, {error}");
            }

            FTFaceRec* face = (FTFaceRec*)faceHandle.Handle;

            var family = Utils.ToStringFromUTF8(face->FamilyName) ?? "Unknown";

            FontStyle style = FontStyle.Regular;

            style |= (face->StyleFlags & (1 << 0)) != 0 ? FontStyle.Italic : FontStyle.Regular;
            style |= (face->StyleFlags & (1 << 1)) != 0 ? FontStyle.Bold : FontStyle.Regular;

            TtOs2* os2 = (TtOs2*)faceHandle.GetSfntTable(FTSfntTag.Os2);

            FontWeight weight = FontWeight.Regular;

            if (os2 != null)
            {
                weight = (FontWeight)os2->UsWeightClass;
            }

            faceHandle.DoneFace();

            return new(path, family, style, weight);
        }

        public static FontFileInfo ReadFrom(FTFace faceHandle, string path)
        {
            FTFaceRec* face = (FTFaceRec*)faceHandle.Handle;

            var family = Utils.ToStringFromUTF8(face->FamilyName) ?? "Unknown";

            FontStyle style = FontStyle.Regular;

            style |= (face->StyleFlags & (1 << 0)) != 0 ? FontStyle.Italic : FontStyle.Regular;
            style |= (face->StyleFlags & (1 << 1)) != 0 ? FontStyle.Bold : FontStyle.Regular;

            TtOs2* os2 = (TtOs2*)faceHandle.GetSfntTable(FTSfntTag.Os2);

            FontWeight weight = FontWeight.Regular;

            if (os2 != null)
            {
                weight = (FontWeight)os2->UsWeightClass;
            }

            return new(path, family, style, weight);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is FontFileInfo info && Equals(info);
        }

        public readonly bool Equals(FontFileInfo other)
        {
            return Path == other.Path &&
                   Family == other.Family &&
                   Style == other.Style &&
                   Weight == other.Weight;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path, Family, Style, Weight);
        }

        public static bool operator ==(FontFileInfo left, FontFileInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FontFileInfo left, FontFileInfo right)
        {
            return !(left == right);
        }

        public static implicit operator FontIdentifier(FontFileInfo font)
        {
            return new(font.Family.ToLowerInvariant(), font.Style, font.Weight);
        }
    }
}