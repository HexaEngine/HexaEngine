namespace HexaEngine.SPIRVCross
{
    using System.Reflection;
    using System.Runtime.InteropServices;

    public static unsafe partial class SPIRV
    {
        internal static IntPtr LoadNativeLibrary()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return LibraryLoader.LoadLocalLibrary("spirv-cross-c-shared.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LibraryLoader.LoadLocalLibrary("spirv-cross-c-shared.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return LibraryLoader.LoadLocalLibrary("spirv-cross-c-shared.dylib");
            }
            else
            {
                return LibraryLoader.LoadLocalLibrary("spirv-cross-c-shared");
            }
        }
    }

    internal static class LibraryLoader
    {
        static LibraryLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Extension = ".dll";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Extension = ".dylib";
            else
                Extension = ".so";
        }

        public static string Extension { get; }

        public static IntPtr LoadLocalLibrary(string libraryName)
        {
            if (!libraryName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                libraryName += Extension;

            var osPlatform = GetOSPlatform();
            var architecture = GetArchitecture();

            var libraryPath = GetNativeAssemblyPath(osPlatform, architecture, libraryName);

            static string GetOSPlatform()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "win";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "linux";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "osx";

                throw new ArgumentException("Unsupported OS platform.");
            }

            static string GetArchitecture()
            {
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X86: return "x86";
                    case Architecture.X64: return "x64";
                    case Architecture.Arm: return "arm";
                    case Architecture.Arm64: return "arm64";
                }

                throw new ArgumentException("Unsupported architecture.");
            }

            static string GetNativeAssemblyPath(string osPlatform, string architecture, string libraryName)
            {
                var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (assemblyLocation == null)
                    throw new Exception();

                var paths = new[]
                {
                    Path.Combine(assemblyLocation, libraryName),
                    Path.Combine(assemblyLocation, "runtimes", osPlatform, "native", libraryName),
                    Path.Combine(assemblyLocation, "runtimes", $"{osPlatform}-{architecture}", "native", libraryName),
                };

                foreach (var path in paths)
                {
                    if (File.Exists(path))
                        return path;
                }

                return libraryName;
            }

            IntPtr handle;
#if NETSTANDARD2_0
            handle = LoadPlatformLibrary(libraryPath);
#else
            handle = NativeLibrary.Load(libraryPath);
#endif

            if (handle == IntPtr.Zero)
                throw new DllNotFoundException($"Unable to load library '{libraryName}'.");

            return handle;
        }

        public static T LoadFunction<T>(IntPtr library, string name)
        {
#if NETSTANDARD2_0
            nint symbol = GetSymbol(library, name);
#else
            nint symbol = NativeLibrary.GetExport(library, name);
#endif

            if (symbol == nint.Zero)
                throw new EntryPointNotFoundException($"Unable to load symbol '{name}'.");

            return Marshal.GetDelegateForFunctionPointer<T>(symbol);
        }

#if NETSTANDARD2_0

        private static IntPtr LoadPlatformLibrary(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName))
                throw new ArgumentNullException("libraryName");

            IntPtr handle;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                handle = Win32.LoadLibrary(libraryName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                handle = Linux.dlopen(libraryName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                handle = Mac.dlopen(libraryName);
            else
                throw new PlatformNotSupportedException($"Current platform is unknown, unable to load library '{libraryName}'.");

            return handle;
        }

        private static IntPtr GetSymbol(IntPtr library, string symbolName)
        {
            if (string.IsNullOrEmpty(symbolName))
                throw new ArgumentNullException("symbolName");

            IntPtr handle;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                handle = Win32.GetProcAddress(library, symbolName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                handle = Linux.dlsym(library, symbolName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                handle = Mac.dlsym(library, symbolName);
            else
                throw new PlatformNotSupportedException($"Current platform is unknown, unable to load symbol '{symbolName}' from library {library}.");

            return handle;
        }

#pragma warning disable IDE1006 // Naming Styles

        private static class Mac
        {
            private const string SystemLibrary = "/usr/lib/libSystem.dylib";

            private const int RTLD_LAZY = 1;
            private const int RTLD_NOW = 2;

            public static IntPtr dlopen(string path, bool lazy = true) =>
                dlopen(path, lazy ? RTLD_LAZY : RTLD_NOW);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlopen(string path, int mode);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport(SystemLibrary)]
            public static extern void dlclose(IntPtr handle);
        }

        private static class Linux
        {
            private const string SystemLibrary = "libdl.so";

            private const int RTLD_LAZY = 1;
            private const int RTLD_NOW = 2;

            public static IntPtr dlopen(string path, bool lazy = true) =>
                dlopen(path, lazy ? RTLD_LAZY : RTLD_NOW);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlopen(string path, int mode);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport(SystemLibrary)]
            public static extern void dlclose(IntPtr handle);
        }

        private static class Win32
        {
            private const string SystemLibrary = "Kernel32.dll";

            [DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern void FreeLibrary(IntPtr hModule);
        }

#pragma warning restore IDE1006 // Naming Styles
#endif
    }
}