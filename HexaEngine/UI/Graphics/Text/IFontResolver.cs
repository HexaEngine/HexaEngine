namespace HexaEngine.UI.Graphics.Text
{
    using System.Diagnostics.CodeAnalysis;

    public interface IFontResolver
    {
        IReadOnlyList<FontFamily> GetFontFamilies();

        string? Resolve(string familyName, FontStyle style, FontWeight weight);

        FontFamily? Resolve(string familyName);

        bool TryResolve(string familyName, FontStyle style, FontWeight weight, [MaybeNullWhen(false)] out string? fontFile);

        bool TryResolve(string familyName, [MaybeNullWhen(false)] out FontFamily? fontFamily);
    }
}