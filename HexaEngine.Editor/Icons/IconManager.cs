namespace HexaEngine.Editor.Icons
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Textures;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Themes;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Xml.Serialization;

    /// <summary>
    /// A class for managing and interacting with icons.
    /// </summary>
    public static class IconManager
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(IconManager));
        private static readonly List<Icon> icons = [];
        private static IGraphicsDevice device = Application.GraphicsDevice;
        private static Icon Default = null!;
        private static readonly AtlasBuilder atlasBuilder = new(256 * 8 + 2, 2048, Format.R8G8B8A8UNorm);
        private static Texture2D iconAtlas = null!;
        private static readonly List<IconGlyphTileInfo> glyphs = new();
        private static readonly Dictionary<Guid, IconGlyphTileInfo> keyToGlyph = new();
        private static readonly IconCache iconCache = new("cache/iconcache.bin");

        /// <summary>
        /// Initializes the IconManager, loads icons from XML, and sets up default icons.
        /// </summary>
        internal static unsafe void Init()
        {
            Default = new Icon(device, atlasBuilder);

            try
            {
                var serializer = new XmlSerializer(typeof(IconsDescription));
                var fs = FileSystem.OpenText("assets/icons/icons.xml");
                var desc = (IconsDescription)serializer.Deserialize(fs)!;

                var theme = ThemeManager.ThemeName;

                List<Guid> iconGuids = new();
                for (int i = 0; i < desc.Icons.Count; i++)
                {
                    var iconDesc = desc.Icons[i];

                    if (iconDesc.Theme != "any" && iconDesc.Theme != theme)
                    {
                        continue;
                    }

                    var key = new Guid(MD5.HashData(MemoryMarshal.AsBytes(iconDesc.Path.AsSpan())));
                    iconGuids.Add(key);
                    icons.Add(new(iconDesc));
                }

                if (iconCache.TryGet(iconGuids, out int width, out int height, out Format format, out byte* data, out int rowPitch, out var iconGlyphs))
                {
                    Point2 pen = default;
                    int maxHeight = 0;
                    int lastHeight = 0;

                    for (int i = 0; i < iconGlyphs.Length; i++)
                    {
                        var glyph = iconGlyphs[i];
                        glyphs.Add(glyph);
                        keyToGlyph.Add(glyph.Key, glyph);
                        if (glyph.Pos.Y != lastHeight)
                        {
                            pen.Y += maxHeight;
                            pen.X = 0;
                            maxHeight = 0;
                            lastHeight = glyph.Pos.Y;
                        }
                        else
                        {
                            maxHeight = Math.Max(maxHeight, glyph.Size.Y);
                        }
                        pen.X += glyph.Size.X;
                    }

                    for (int i = 0; i < icons.Count; i++)
                    {
                        AddToAtlas(icons[i]);
                    }

                    atlasBuilder.SetBuffer(format, width, height, data, pen, maxHeight);

                    UpdateAtlas();
                }
                else
                {
                    for (int i = 0; i < icons.Count; i++)
                    {
                        AddToAtlas(icons[i]);
                    }

                    UpdateAtlas();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load icons");
                Logger.Log(ex);
                MessageBox.Show("Failed to load icons", ex.Message);
            }

            icons.Sort(IconPriorityComparer.Default);
        }

        private static unsafe void UpdateAtlas()
        {
            byte* data;
            int width;
            int height;
            int rowPitch;
            atlasBuilder.Build(&data, &width, &height, &rowPitch);

            iconCache.Set(glyphs, width, height, Format.R8G8B8A8UNorm, data, rowPitch * height);

            Texture2DDescription description = new(Format.R8G8B8A8UNorm, width, height, 1, 1, GpuAccessFlags.Read);

            iconAtlas = new Texture2D(description, new SubresourceData(data, rowPitch));

            Default.SetAtlas(iconAtlas);
            for (int i = 0; i < icons.Count; i++)
            {
                icons[i].SetAtlas(iconAtlas);
            }
        }

        private static unsafe void RebuildAtlas()
        {
            keyToGlyph.Clear();
            glyphs.Clear();
            atlasBuilder.Reset();
            Default.CreateDefault(device, atlasBuilder);

            try
            {
                var theme = ThemeManager.ThemeName;

                for (int i = 0; i < icons.Count; i++)
                {
                    var icon = icons[i];

                    var desc = icon.Description;

                    if (desc.Theme != "any" && desc.Theme != theme)
                    {
                        continue;
                    }

                    AddToAtlas(icon);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load icons");
                Logger.Log(ex);
                MessageBox.Show("Failed to load icons", ex.Message);
            }

            UpdateAtlas();
        }

        private static unsafe bool AddToAtlas(Icon icon)
        {
            bool updated = false;
            var desc = icon.Description;
            var key = new Guid(MD5.HashData(MemoryMarshal.AsBytes(desc.Path.AsSpan())));

            if (!keyToGlyph.TryGetValue(key, out var info))
            {
                var image = device.TextureLoader.LoadFormAssets(desc.Path);
                var pos = atlasBuilder.Append(image, 256, 256);
                info = new(key, pos, new(256));
                keyToGlyph.Add(key, info);
                glyphs.Add(info);
                image.Dispose();
                updated = true;
            }

            icon.AtlasPos = info.Pos;
            icon.Size = info.Size;
            return updated;
        }

        /// <summary>
        /// Adds an icon to the manager, replacing any existing icon with the same name.
        /// </summary>
        /// <param name="icon">The icon to add.</param>
        public static void AddIcon(IconDescription iconDesc)
        {
            var old = GetIconByName(iconDesc.Name);
            var removedOld = false;
            if (old != null)
            {
                removedOld = icons.Remove(old);
            }

            if (!removedOld)
            {
                Icon icon = new(iconDesc);
                bool updated = AddToAtlas(icon);
                icons.Add(icon);
                if (updated)
                {
                    UpdateAtlas();
                }
            }
            else
            {
                Icon icon = new(iconDesc);
                icons.Add(icon);
                RebuildAtlas();
            }

            icons.Sort(IconPriorityComparer.Default);
        }

        /// <summary>
        /// Removes an icon from the manager and releases associated resources.
        /// </summary>
        /// <param name="icon">The icon to remove.</param>
        public static bool RemoveIcon(IconDescription iconDesc)
        {
            var icon = GetIconByName(iconDesc.Name);

            if (icon == null)
            {
                return false;
            }

            if (!icons.Remove(icon))
            {
                return false;
            }

            RebuildAtlas();

            return true;
        }

        /// <summary>
        /// Gets an icon by its name.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <returns>The icon with the specified name, or null if not found.</returns>
        public static Icon? GetIconByName(string name)
        {
            for (int i = 0; i < icons.Count; i++)
            {
                var icon = icons[i];
                if (icon.Name == name)
                {
                    return icon;
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to get an icon by its name and returns a Boolean indicating success.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="icon">The retrieved icon, if successful.</param>
        /// <returns>True if an icon with the specified name is found, otherwise false.</returns>
        public static bool TryGetIconByName(string name, [NotNullWhen(true)] out Icon? icon)
        {
            icon = GetIconByName(name);
            return icon != null;
        }

        /// <summary>
        /// Gets an appropriate icon for a file based on name matching and priorities.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <returns>The icon associated with the file, or the default icon if not found.</returns>
        public static Icon GetIconForFile(string name)
        {
            for (int i = 0; i < icons.Count; i++)
            {
                var icon = icons[i];
                if (icon.Regex.IsMatch(name))
                {
                    return icon;
                }
            }
            return Default;
        }

        /// <summary>
        /// Gets an appropriate icon for a file based on name matching and priorities.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <returns>The icon associated with the file, or the default icon if not found.</returns>
        public static Icon GetIconForDirectory(string name)
        {
            for (int i = 0; i < icons.Count; i++)
            {
                var icon = icons[i];
                if (icon.Target == "/")
                {
                    return icon;
                }
            }
            return Default;
        }

        /// <summary>
        /// Releases resources held by the IconManager, including default and custom icons.
        /// </summary>
        internal static void Dispose()
        {
            iconAtlas.Dispose();
            atlasBuilder.Dispose();
        }
    }
}