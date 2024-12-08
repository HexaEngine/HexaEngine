﻿/*
Copyright (c) 2005-2007 empira Software GmbH, Cologne (Germany)
Modified work Copyright (c) 2016 David Dunscombe
*/

namespace HexaEngine.UI.Graphics.Text
{
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public static class LinuxSystemFontResolver
    {
        private const string libfontconfig = "libfontconfig.so.1";

        [DllImport(libfontconfig)]
        private static extern IntPtr FcInitLoadConfigAndFonts();

        private static readonly Lazy<IntPtr> fcConfig = new Lazy<IntPtr>(FcInitLoadConfigAndFonts);

        [DllImport(libfontconfig)]
        private static extern FcPatternHandle FcPatternCreate();

        [DllImport(libfontconfig)]
        private static extern int FcPatternGetString(IntPtr p, [MarshalAs(UnmanagedType.LPStr)] string obj, int n, ref IntPtr s);

        [DllImport(libfontconfig)]
        private static extern void FcPatternDestroy(IntPtr pattern);

        public class FcPatternHandle : SafeHandle
        {
            private FcPatternHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => this.handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                FcPatternDestroy(this.handle);
                return true;
            }
        }

        [DllImport(libfontconfig)] public static extern FcObjectSetHandle FcObjectSetCreate();

        [DllImport(libfontconfig)] public static extern int FcObjectSetAdd(FcObjectSetHandle os, [MarshalAs(UnmanagedType.LPStr)] string obj);

        [DllImport(libfontconfig)] public static extern void FcObjectSetDestroy(IntPtr os);

        public class FcObjectSetHandle : SafeHandle
        {
            private FcObjectSetHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => this.handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                FcObjectSetDestroy(this.handle);
                return true;
            }

            public static FcObjectSetHandle Create(params string[] objs)
            {
                var os = FcObjectSetCreate();
                foreach (var obj in objs)
                {
                    FcObjectSetAdd(os, obj);
                }

                FcObjectSetAdd(os, "");
                return os;
            }
        }

        [DllImport(libfontconfig)] public static extern FcFontSetHandle FcFontList(IntPtr config, FcPatternHandle pattern, FcObjectSetHandle os);

        [DllImport(libfontconfig)] public static extern void FcFontSetDestroy(IntPtr fs);

        public struct FcFontSet
        {
            public int nfont;
            public int sfont;
            public IntPtr fonts;
        }

        public class FcFontSetHandle : SafeHandle
        {
            private FcFontSetHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => this.handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                FcFontSetDestroy(this.handle);
                return true;
            }

            public FcFontSet Read()
            {
                return Marshal.PtrToStructure<FcFontSet>(this.handle);
            }
        }

        private static string? GetString(IntPtr handle, string obj)
        {
            var ptr = IntPtr.Zero;
            var result = FcPatternGetString(handle, obj, 0, ref ptr);
            if (result == 0)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            else
            {
                return null;
            }
        }

        private static IEnumerable<string> ResolveFontConfig()
        {
            var config = fcConfig.Value;
            using (var pattern = FcPatternCreate())
            using (var os = FcObjectSetHandle.Create("family", "style", "file"))
            using (var fs = FcFontList(config, pattern, os))
            {
                var fset = fs.Read();
                for (int index = 0; index < fset.nfont; index++)
                {
                    var font = Marshal.ReadIntPtr(fset.fonts, index * Marshal.SizeOf<IntPtr>());
                    var family = GetString(font, "family");
                    var style = GetString(font, "style");
                    var file = GetString(font, "file");

                    if (family is null || style is null || file is null)
                    {
                        continue;
                    }

                    yield return file;
                }
            }
        }

        public static string[] Resolve()
        {
            try
            {
                return ResolveFontConfig().Where(x => x.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)).ToArray();
            }
            catch (Exception)
            {
                return ResolveFallback().Where(x => x.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)).ToArray();
            }
        }

        private static IEnumerable<string> ResolveFallback()
        {
            var fontList = new List<string>();

            void AddFontsToFontList(string path)
            {
                if (!Directory.Exists(path))
                {
                    return;
                }

                foreach (string subDir in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                {
                    fontList.AddRange(Directory.EnumerateFiles(subDir, "*", SearchOption.AllDirectories));
                }
            }

            var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in SearchPaths())
            {
                if (hs.Contains(path))
                {
                    continue;
                }

                hs.Add(path);
                AddFontsToFontList(path);
            }

            return fontList.ToArray();
        }

        private static IEnumerable<string> SearchPaths()
        {
            var dirs = new List<string>();
            try
            {
                Regex confRegex = new Regex("<dir>(?<dir>.*)</dir>", RegexOptions.Compiled);
                using (var reader = new StreamReader(File.OpenRead("/etc/fonts/fonts.conf")))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Match match = confRegex.Match(line);
                        if (!match.Success)
                        {
                            continue;
                        }

                        string path = match.Groups["dir"].Value.Trim();
                        if (path.StartsWith("~"))
                        {
                            path = Environment.GetEnvironmentVariable("HOME") + path.Substring(1);
                        }

                        dirs.Add(path);
                    } // Whend
                } // End Using reader
            }
            catch (Exception)
            {
            }

            dirs.Add("/usr/share/fonts");
            dirs.Add("/usr/local/share/fonts");
            dirs.Add(Environment.GetEnvironmentVariable("HOME") + "/.fonts");
            return dirs;
        }
    }
}