namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    public class DefaultFontResolver : IFontResolver
    {
        private readonly FTLibrary library;
        private readonly List<FontFamily> fontFamilies = [];
        private readonly Dictionary<string, FontFamily> nameToFamily = [];
        private readonly Dictionary<FontIdentifier, string> InstalledFonts = [];
        private readonly string[] SupportedFonts;

        public DefaultFontResolver(FTLibrary library)
        {
            this.library = library;
            string fontDir;

            List<string> fontPaths = [];

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                fontDir = "/Library/Fonts/";
                fontPaths.AddRange(Directory.GetFiles(fontDir, "*.ttf", SearchOption.AllDirectories));
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fontPaths.AddRange(LinuxSystemFontResolver.Resolve());
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fontDir = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts");

                var systemFontPaths = Directory.GetFiles(fontDir, "*.ttf", SearchOption.AllDirectories);
                fontPaths.AddRange(systemFontPaths);

                var appdataFontDir = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Microsoft\Windows\Fonts");
                if (Directory.Exists(appdataFontDir))
                {
                    var appdataFontPaths = Directory.GetFiles(appdataFontDir, "*.ttf", SearchOption.AllDirectories);
                    fontPaths.AddRange(appdataFontPaths);
                }
            }

            fontPaths.AddRange(FileSystem.GetFiles("fonts/", "*.ttf"));
            SupportedFonts = [.. fontPaths];
            SetupFontsFiles();
            return;
        }

        private void SetupFontsFiles()
        {
            List<FontFileInfo> tempFontInfoList = [];
            foreach (string fontPathFile in SupportedFonts)
            {
                try
                {
                    FontFileInfo fontInfo = FontFileInfo.ReadFrom(library, fontPathFile);
                    tempFontInfoList.Add(fontInfo);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            // Deserialize all font families
            foreach (IGrouping<string, FontFileInfo> familyGroup in tempFontInfoList.GroupBy(info => info.Family))
            {
                try
                {
                    var group = familyGroup.ToArray();
                    string familyName = familyGroup.Key;
                    FontFamily family = DeserializeFontFamily(familyName, group);
                    nameToFamily.Add(familyName.ToLowerInvariant(), family);
                    fontFamilies.Add(family);
                    for (int i = 0; i < group.Length; i++)
                    {
                        InstalledFonts.TryAdd(group[i], group[i].Path);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private static FontFamily DeserializeFontFamily(string fontFamilyName, IEnumerable<FontFileInfo> fontList)
        {
            return new(fontFamilyName, fontList.ToList());
        }

        public IReadOnlyList<FontFamily> GetFontFamilies()
        {
            return fontFamilies;
        }

        public FontFamily? Resolve(string familyName)
        {
            if (nameToFamily.TryGetValue(familyName.ToLowerInvariant(), out var family))
            {
                return family;
            }

            return null;
        }

        public bool TryResolve(string familyName, [MaybeNullWhen(false)] out FontFamily? fontFamily)
        {
            return nameToFamily.TryGetValue(familyName.ToLowerInvariant(), out fontFamily);
        }

        public string? Resolve(string familyName, FontStyle style, FontWeight weight)
        {
            InstalledFonts.TryGetValue(new FontIdentifier(familyName.ToLowerInvariant(), style, weight), out var fontFile);
            return fontFile;
        }

        public bool TryResolve(string familyName, FontStyle style, FontWeight weight, [MaybeNullWhen(false)] out string? fontFile)
        {
            if (InstalledFonts.TryGetValue(new FontIdentifier(familyName.ToLowerInvariant(), style, weight), out fontFile))
            {
                return true;
            }

            fontFile = null;
            return false;
        }
    }
}