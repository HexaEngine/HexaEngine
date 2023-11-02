namespace HexaEngine.Editor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.UI;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Serialization;

    public class IconPriorityComparer : IComparer<Icon>
    {
        public static readonly IconPriorityComparer Default = new();

        public int Compare(Icon? x, Icon? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            if (x.Priority < y.Priority)
            {
                return 1;
            }
            else if (x.Priority > y.Priority)
            {
                return -1;
            }

            return 0;
        }
    }

    /// <summary>
    /// A class for managing and interacting with icons.
    /// </summary>
    public static class IconManager
    {
        private static readonly List<Icon> icons = new();
        private static Icon Default;

        /// <summary>
        /// Initializes the IconManager, loads icons from XML, and sets up default icons.
        /// </summary>
        /// <param name="device">The graphics device for rendering icons.</param>
        internal static void Init(IGraphicsDevice device)
        {
            Default = new Icon(device);
            try
            {
                var serializer = new XmlSerializer(typeof(IconsDescription));
                var fs = FileSystem.OpenText("assets/icons/icons.xml");
                var desc = (IconsDescription)serializer.Deserialize(fs);

                var theme = ThemeManager.ThemeName;

                for (int i = 0; i < desc.Icons.Count; i++)
                {
                    var icon = desc.Icons[i];

                    if (icon.Theme != theme)
                    {
                        continue;
                    }

                    icons.Add(new Icon(device, icon));
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

        /// <summary>
        /// Adds an icon to the manager, replacing any existing icon with the same name.
        /// </summary>
        /// <param name="icon">The icon to add.</param>
        public static void AddIcon(Icon icon)
        {
            var old = GetIconByName(icon.Name);
            if (old != null)
            {
                icons.Remove(old);
                old.Dispose();
            }

            icons.Add(icon);
            icons.Sort(IconPriorityComparer.Default);
        }

        /// <summary>
        /// Removes an icon from the manager and releases associated resources.
        /// </summary>
        /// <param name="icon">The icon to remove.</param>
        public static void RemoveIcon(Icon icon)
        {
            icons.Remove(icon);
            icon.Dispose();
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
        internal static void Release()
        {
            Default.Dispose();
            for (int i = 0; i < icons.Count; i++)
            {
                icons[i].Dispose();
            }
        }
    }

    [XmlRoot(ElementName = "Icon")]
    public class IconDescription
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "theme")]
        public string Theme { get; set; }

        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }

        [XmlAttribute(AttributeName = "target")]
        public string Target { get; set; }

        [XmlAttribute(AttributeName = "priority")]
        public int Priority { get; set; }
    }

    [XmlRoot(ElementName = "Icons")]
    public class IconsDescription
    {
        [XmlElement(ElementName = "Icon")]
        public List<IconDescription> Icons { get; set; }
    }
}