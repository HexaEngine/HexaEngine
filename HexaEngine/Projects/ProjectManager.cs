namespace HexaEngine.Projects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Assets;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Dotnet;
    using HexaEngine.Scripting;
    using HexaEngine.Windows;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;

    public static class ProjectManager
    {
        private static FileSystemWatcher? watcher;
        private static bool scriptProjectChanged;

        public static readonly List<string> ReferencedAssemblyNames = new();

        public static readonly List<Assembly> ReferencedAssemblies = new();

        static ProjectManager()
        {
            ReferencedAssemblyNames.Add("HexaEngine.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            foreach (string name in ReferencedAssemblyNames)
            {
                var assembly = Assembly.Load(name);
                ReferencedAssemblies.Add(assembly);
            }
        }

        public static string? CurrentFolder { get; private set; }

        public static string? CurrentProjectPath { get; private set; }

        public static string? CurrentProjectAssetsFolder { get; private set; }

        public static bool ScriptProjectChanged => scriptProjectChanged;

        public static HexaProject? Project { get; set; }

        public static event Action<HexaProject?>? ProjectChanged;

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public static async void Load(string path)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            CurrentProjectPath = path;
            Project = HexaProject.Load(CurrentProjectPath);
            if (Project == null)
                return;
            CurrentFolder = Path.GetDirectoryName(CurrentProjectPath);
            var assets = Project.Items.FirstOrDefault(x => x.Name == "assets");
            if (assets == null)
            {
                assets = Project.CreateFolder("assets");
                Project.Save();
            }
            CurrentProjectAssetsFolder = assets.GetAbsolutePath();
            FileSystem.AddSource(CurrentProjectAssetsFolder);
            Paths.CurrentProjectFolder = CurrentProjectAssetsFolder;
            ProjectHistory.AddEntry(Project.Name, CurrentProjectPath);
            ProjectChanged?.Invoke(Project);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string solutionName = Path.GetFileName(CurrentFolder);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'path2' in 'string Path.Combine(string path1, string path2)'.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'path1' in 'string Path.Combine(string path1, string path2)'.
            string projectPath = Path.Combine(CurrentFolder, solutionName);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'path1' in 'string Path.Combine(string path1, string path2)'.
#pragma warning restore CS8604 // Possible null reference argument for parameter 'path2' in 'string Path.Combine(string path1, string path2)'.
            watcher = new(projectPath);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
            _ = Task.Factory.StartNew(UpdateScripts);
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            scriptProjectChanged = true;
        }

        public static void Create(string path)
        {
            CurrentFolder = path;
            GenerateProject();
            GenerateSolution();
        }

        private static void GenerateProject()
        {
            if (CurrentFolder == null) return;
            string projectName = Path.GetFileName(CurrentFolder);
            CurrentProjectPath = Path.Combine(CurrentFolder, $"{projectName}.hexproj");
            Project = HexaProject.Create(CurrentProjectPath);
            var assets = Project.CreateFolder("assets");
            Project.Save();
            CurrentProjectAssetsFolder = assets.GetAbsolutePath();
            FileSystem.AddSource(CurrentProjectAssetsFolder);
            Paths.CurrentProjectFolder = CurrentProjectAssetsFolder;
            ProjectHistory.AddEntry(Project.Name, CurrentProjectPath);
            ProjectChanged?.Invoke(Project);
        }

        private static void GenerateSolution()
        {
            if (CurrentFolder == null) return;
            string solutionName = Path.GetFileName(CurrentFolder);
            string solutionPath = Path.Combine(CurrentFolder, solutionName + ".sln");
            Dotnet.New(DotnetTemplate.Sln, CurrentFolder, solutionName);
            Dotnet.New(DotnetTemplate.Classlib, Path.Combine(CurrentFolder, solutionName));
            string projectPath = Path.Combine(CurrentFolder, solutionName, $"{solutionName}.csproj");
            string projectFilePath = Path.Combine(CurrentFolder, solutionName, $"{solutionName}.csproj");
            Dotnet.Sln(SlnCommand.Add, solutionPath, projectPath);
            Dotnet.AddDlls(projectFilePath, ReferencedAssemblies.ConvertAll(x => x.Location));
        }

        public static void OpenVisualStudio()
        {
            if (CurrentFolder == null) return;
            string? solutionName = Path.GetFileName(CurrentFolder);
            string solutionPath = Path.Combine(CurrentFolder, solutionName + ".sln");
            ProcessStartInfo psi = new();
            psi.FileName = "cmd";
            psi.Arguments = $"/c start devenv \"{solutionPath}\"";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        public static Task UpdateScripts()
        {
            AssemblyManager.Unload();
            if (CurrentFolder == null) return Task.CompletedTask;
            string solutionName = Path.GetFileName(CurrentFolder);
            Build();
            string outputFilePath = Path.Combine(CurrentFolder, "bin", $"{solutionName}.dll");
            AssemblyManager.Load(outputFilePath);
            return Task.CompletedTask;
        }

        private static void Build()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string solutionName = Path.GetFileName(CurrentFolder);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'path2' in 'string Path.Combine(string path1, string path2, string path3)'.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'path1' in 'string Path.Combine(string path1, string path2, string path3)'.
            string projectFilePath = Path.Combine(CurrentFolder, solutionName, $"{solutionName}.csproj");
#pragma warning restore CS8604 // Possible null reference argument for parameter 'path1' in 'string Path.Combine(string path1, string path2, string path3)'.
#pragma warning restore CS8604 // Possible null reference argument for parameter 'path2' in 'string Path.Combine(string path1, string path2, string path3)'.
            Dotnet.Build(projectFilePath, Path.Combine(CurrentFolder, "bin"));
            scriptProjectChanged = false;
        }

        private static void BuildShaders(string output)
        {
            var entries = ShaderCache.Entries;
            foreach (var entry in entries)
            {
                string path = Path.Combine(output, entry.Name);
            }
        }

        public static Task Publish(PublishSettings settings)
        {
            var debugType = settings.StripDebugInfo ? DebugType.None : DebugType.Full;
            if (settings.StartupScene == null) return Task.CompletedTask;
            if (CurrentFolder == null) return Task.CompletedTask;
            if (CurrentProjectPath == null) return Task.CompletedTask;
            if (CurrentProjectAssetsFolder == null) return Task.CompletedTask;

            for (int i = 0; i < settings.Scenes.Count; i++)
            {
                var scene = settings.Scenes[i];
                var path = Path.Combine(CurrentProjectAssetsFolder, scene);
                if (!File.Exists(path)) return Task.CompletedTask;
            }

            Debug.WriteLine("Publishing Project");
            string buildPath = Path.Combine(CurrentFolder, "build");
            if (Directory.Exists(buildPath))
            {
                Directory.Delete(buildPath, true);
            }
            Directory.CreateDirectory(buildPath);
            string assetsBuildPath = Path.Combine(buildPath, "assets");
            Directory.CreateDirectory(assetsBuildPath);

            // copy scenes to assets
            for (int i = 0; i < settings.Scenes.Count; i++)
            {
                var scene = settings.Scenes[i];
                var path = Path.Combine(CurrentProjectAssetsFolder, scene);
                File.Copy(path, Path.Combine(assetsBuildPath, scene));
            }

            // publish script assembly
            string solutionName = Path.GetFileName(CurrentFolder);
            string scriptProjPath = Path.Combine(CurrentFolder, solutionName);
            string scriptPublishPath = Path.Combine(scriptProjPath, "bin", "Publish");
            PublishOptions scriptPublishOptions = new()
            {
                Framework = "net7.0",
                Profile = settings.Profile,
                RuntimeIdentifer = settings.RuntimeIdentifier,
                PublishReadyToRun = false,
                PublishSingleFile = false,
                SelfContained = false,
                DebugSymbols = !settings.StripDebugInfo,
                DebugType = debugType,
            };
            Dotnet.Publish(scriptProjPath, scriptPublishPath, scriptPublishOptions);

            // copy script assembly
            string assemblyPath = Path.Combine(scriptPublishPath, $"{solutionName}.dll");
            string assemblyPdbPath = Path.Combine(scriptPublishPath, $"{solutionName}.pdb");
            string assemblyBuildPath = Path.Combine(assetsBuildPath, Path.GetFileName(assemblyPath));
            string assemblyPdbBuildPath = Path.Combine(assetsBuildPath, Path.GetFileName(assemblyPdbPath));
            string assemblyPathRelative = Path.GetRelativePath(buildPath, assemblyBuildPath);
            File.Copy(assemblyPath, assemblyBuildPath);
            if (!settings.StripDebugInfo)
                File.Copy(assemblyPdbPath, assemblyPdbBuildPath);

            // app assets
            List<AssetDesc> assets = new();
            string assetsPackagePath = Path.Combine(assetsBuildPath, $"{solutionName}.assets");
            assets.AddRange(AssetDesc.CreateFromDir(CurrentProjectAssetsFolder));

            // shared assets
            string assetsPath = Paths.CurrentAssetsPath;
            string sharedAssetsPath = Path.Combine(assetsPath, "shared");
            string sharedAssetPackagePath = Path.Combine(assetsPath, "shared.assets");
            if (File.Exists(sharedAssetPackagePath))
            {
                AssetArchive archive = new(sharedAssetPackagePath);
                assets.AddRange(archive.Assets.Cast<AssetDesc>());
            }
            else
            {
                assets.AddRange(AssetDesc.CreateFromDir(sharedAssetsPath));
            }

            AssetArchive.CreateFrom(assets.ToArray(), assetsPackagePath, Compression.LZ4, CompressionLevel.SmallestSize);

            // app config
            string configBuildPath = Path.Combine(buildPath, "app.config");
            AppConfig config = new()
            {
                StartupScene = Path.Combine("assets", settings.StartupScene),
                ScriptAssembly = assemblyPathRelative,
            };
            config.Save(configBuildPath);

            // build app
            string appTempPath = Path.Combine(CurrentFolder, ".build");
            string appTempPublishPath = Path.Combine(appTempPath, "publish");
            string appTempProjPath = Path.Combine(appTempPath, $"{solutionName}.csproj");
            string appTempProgramPath = Path.Combine(appTempPath, "Program.cs");
            if (Directory.Exists(appTempPath))
            {
                Directory.Delete(appTempPath, true);
            }
            Directory.CreateDirectory(appTempPath);
            Dotnet.New(DotnetTemplate.Console, appTempPath, solutionName);
            Dotnet.AddDlls(appTempProjPath, Path.GetFullPath("HexaEngine.dll"), Path.GetFullPath("HexaEngine.Core.dll"), Path.GetFullPath("HexaEngine.D3D11.dll"));
            if (settings.RuntimeIdentifier?.Contains("win") ?? false)
                Dotnet.ChangeOutputType(appTempProjPath, "WinExe");
            File.WriteAllText(appTempProgramPath, PublishProgram);
            PublishOptions appPublishOptions = new()
            {
                Framework = "net7.0",
                Profile = settings.Profile,
                RuntimeIdentifer = settings.RuntimeIdentifier,
                PublishReadyToRun = settings.ReadyToRun,
                PublishSingleFile = settings.SingleFile,
                SelfContained = true,
                DebugSymbols = !settings.StripDebugInfo,
                DebugType = debugType,
            };
            Dotnet.Publish(appTempPath, appTempPublishPath, appPublishOptions);

            // copy native runtimes
            string localRuntimesPath = Path.GetFullPath("runtimes");
#pragma warning disable CS8604 // Possible null reference argument for parameter 'path2' in 'string Path.Combine(string path1, string path2, string path3)'.
            string targetRuntimePath = Path.Combine(localRuntimesPath, settings.RuntimeIdentifier, "native");
#pragma warning restore CS8604 // Possible null reference argument for parameter 'path2' in 'string Path.Combine(string path1, string path2, string path3)'.
            CopyDirectory(targetRuntimePath, appTempPublishPath, true);

            // copy binaries to build folder
            CopyDirectory(appTempPublishPath, buildPath, true);

            if (settings.StripDebugInfo)
                DeleteFile(buildPath, "*.pdb", true);

            Debug.WriteLine("Published Project");

            return Task.CompletedTask;
        }

        private static void DeleteFile(string dir, string filter, bool recursive)
        {
            string[] files;
            if (recursive)
            {
                files = Directory.GetFiles(dir, filter, SearchOption.AllDirectories);
            }
            else
            {
                files = Directory.GetFiles(dir, filter, SearchOption.TopDirectoryOnly);
            }
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, string filter, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Extension != filter) continue;
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private const string PublishProgram =
@"
namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.D3D11;
    using HexaEngine.Projects;
    using HexaEngine.Scripting;
    using HexaEngine.Windows;

    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            CrashLogger.Initialize();
            DXGIAdapter.Init();
            AppConfig config = AppConfig.Load();
            FileSystem.Initialize();
            AssemblyManager.Load(config.ScriptAssembly);
            Application.Run(new Window() { Flags = RendererFlags.SceneGraph, StartupScene = config.StartupScene });
        }
    }
}
";
    }

    public static class ComponentRegistry
    {
        private static readonly List<Type> _components = new();

        static ComponentRegistry()
        {
            Scripts = new();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Assembly assembly = Assembly.GetAssembly(typeof(Window));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _components = new(assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Components = new((IEnumerable<Type>)_components);
            AssemblyManager.AssemblyLoaded += AssemblyLoaded;
            AssemblyManager.AssembliesUnloaded += AssembliesUnloaded;
        }

        private static void AssembliesUnloaded(object? sender, EventArgs? e)
        {
            Components.Clear();
            Scripts.Clear();
        }

        private static void AssemblyLoaded(object? sender, Assembly e)
        {
            Assembly assembly = e;
            foreach (var type in assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null))
            {
                Components.Add(type);
            }

            foreach (var type in AssemblyManager.GetAssignableTypes<IScript>(assembly))
            {
                Scripts.Add(type);
            }
        }

        public static ObservableCollection<Type> Components { get; private set; }

        public static ObservableCollection<Type> Scripts { get; private set; }

        public static void RegisterAssembly(string path)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Assembly assembly = AssemblyManager.Load(path);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var type in assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null))
            {
                Components.Add(type);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            foreach (var type in AssemblyManager.GetAssignableTypes<IScript>(assembly))
            {
                Scripts.Add(type);
            }
        }

        public static IEnumerable<Type> GetAssignableTypes<T>()
        {
            return AssemblyManager.GetAssignableTypes<T>();
        }
    }
}