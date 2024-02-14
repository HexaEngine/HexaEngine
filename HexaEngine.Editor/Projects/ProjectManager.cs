namespace HexaEngine.Editor.Projects
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Archives;
    using HexaEngine.Dotnet;
    using HexaEngine.Scripts;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;

    public static class ProjectManager
    {
        private static bool loaded;
        private static FileSystemWatcher? watcher;
        private static bool scriptProjectChanged;

        public static readonly List<string> ReferencedAssemblyNames = [];

        public static readonly List<Assembly> ReferencedAssemblies = [];

        static ProjectManager()
        {
            ReferencedAssemblyNames.Add("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.D3D11, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.D3D12, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.Vulkan, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.OpenGL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.OpenAL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.XAudio, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("Silk.NET.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            foreach (string name in ReferencedAssemblyNames)
            {
                var assembly = Assembly.Load(name);
                ReferencedAssemblies.Add(assembly);
            }
        }

        public static string? CurrentProjectFolder { get; private set; }

        public static string? CurrentProjectFilePath { get; private set; }

        public static string? CurrentProjectAssetsFolder { get; private set; }

        public static bool ScriptProjectChanged => scriptProjectChanged;

        public static event Action<HexaProject?>? ProjectChanged;

        public static bool Loaded => loaded;

        public static void Load(string path)
        {
            CurrentProjectFilePath = path;

            if (loaded)
            {
                ProjectVersionControl.Unload();
            }

            loaded = true;

            CurrentProjectFolder = Path.GetDirectoryName(CurrentProjectFilePath);

            ProjectVersionControl.TryInit();

            SourceAssetsDatabase.Clear();

            FileSystem.RemoveSource(CurrentProjectAssetsFolder);
            CurrentProjectAssetsFolder = Path.Combine(CurrentProjectFolder, "assets");
            string solutionName = Path.GetFileName(CurrentProjectFolder);
            Directory.CreateDirectory(CurrentProjectAssetsFolder);
            FileSystem.AddSource(CurrentProjectAssetsFolder);
            Paths.CurrentProjectFolder = CurrentProjectAssetsFolder;
            ProjectHistory.AddEntry(solutionName, CurrentProjectFilePath);
            ProjectChanged?.Invoke(null);

            SourceAssetsDatabase.Init(CurrentProjectFolder);

            string projectPath = Path.Combine(CurrentProjectFolder, solutionName);

            watcher = new(projectPath);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
            _ = Task.Factory.StartNew(BuildScripts);

            FileSystem.FileCreated += FileSystemFileCreated;
            FileSystem.FileDeleted += FileSystemFileDeleted;
            FileSystem.FileRenamed += FileSystemFileRenamed;
        }

        private static void FileSystemFileRenamed(Core.IO.RenamedEventArgs args)
        {
        }

        private static void FileSystemFileDeleted(Core.IO.FileSystemEventArgs args)
        {
        }

        private static void FileSystemFileCreated(Core.IO.FileSystemEventArgs args)
        {
        }

        private static void Watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            scriptProjectChanged = true;
        }

        public static void Create(string path)
        {
            CurrentProjectFolder = path;
            GenerateProject();
            GenerateSolution();
        }

        private static void GenerateProject()
        {
            if (CurrentProjectFolder == null)
            {
                return;
            }

            string projectName = Path.GetFileName(CurrentProjectFolder);
            CurrentProjectFilePath = Path.Combine(CurrentProjectFolder, $"{projectName}.hexproj");

            FileSystem.RemoveSource(CurrentProjectAssetsFolder);
            CurrentProjectAssetsFolder = Path.Combine(CurrentProjectFolder, "assets");
            FileSystem.AddSource(CurrentProjectAssetsFolder);
            Paths.CurrentProjectFolder = CurrentProjectAssetsFolder;
            ProjectHistory.AddEntry(projectName, CurrentProjectFilePath);
            ProjectChanged?.Invoke(null);
        }

        private static void GenerateSolution()
        {
            if (CurrentProjectFolder == null)
            {
                return;
            }

            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string solutionPath = Path.Combine(CurrentProjectFolder, solutionName + ".sln");
            Dotnet.New(DotnetTemplate.Sln, CurrentProjectFolder, solutionName);
            Dotnet.New(DotnetTemplate.Classlib, Path.Combine(CurrentProjectFolder, solutionName));
            string projectPath = Path.Combine(CurrentProjectFolder, solutionName, $"{solutionName}.csproj");
            string projectFilePath = Path.Combine(CurrentProjectFolder, solutionName, $"{solutionName}.csproj");
            Dotnet.Sln(SlnCommand.Add, solutionPath, projectPath);
            Dotnet.AddDlls(projectFilePath, ReferencedAssemblies.ConvertAll(x => x.Location));
        }

        public static void OpenVisualStudio()
        {
            if (CurrentProjectFolder == null)
            {
                return;
            }

            string? solutionName = Path.GetFileName(CurrentProjectFolder);
            string solutionPath = Path.Combine(CurrentProjectFolder, solutionName + ".sln");
            ProcessStartInfo psi = new();
            psi.FileName = "cmd";
            psi.Arguments = $"/c start devenv \"{solutionPath}\"";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        public static Task BuildScripts()
        {
            AssemblyManager.Unload();
            if (CurrentProjectFolder == null)
            {
                return Task.CompletedTask;
            }

            if (!Build())
            {
                return Task.CompletedTask;
            }

            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string outputFilePath = Path.Combine(CurrentProjectFolder, solutionName, "bin", $"{solutionName}.dll");
            AssemblyManager.Load(outputFilePath);
            return Task.CompletedTask;
        }

        public static Task RebuildScripts()
        {
            AssemblyManager.Unload();
            if (CurrentProjectFolder == null)
            {
                return Task.CompletedTask;
            }

            if (!Rebuild())
            {
                return Task.CompletedTask;
            }

            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string outputFilePath = Path.Combine(CurrentProjectFolder, solutionName, "bin", $"{solutionName}.dll");
            AssemblyManager.Load(outputFilePath);
            return Task.CompletedTask;
        }

        public static Task CleanScripts()
        {
            AssemblyManager.Unload();
            if (CurrentProjectFolder == null)
            {
                return Task.CompletedTask;
            }

            Clean();

            return Task.CompletedTask;
        }

        private static bool Build()
        {
            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string projectFilePath = Path.Combine(CurrentProjectFolder, solutionName, $"{solutionName}.csproj");

            string output = Dotnet.Build(projectFilePath, Path.Combine(CurrentProjectFolder, solutionName, "bin"));
            bool failed = output.Contains("FAILED");
            Logger.Log(output);
            scriptProjectChanged = false;
            return !failed;
        }

        private static bool Rebuild()
        {
            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string projectFilePath = Path.Combine(CurrentProjectFolder, solutionName, $"{solutionName}.csproj");
            string output = Dotnet.Rebuild(projectFilePath, Path.Combine(CurrentProjectFolder, solutionName, "bin"));
            bool failed = output.Contains("FAILED");
            Logger.Log(output);
            scriptProjectChanged = false;
            return !failed;
        }

        private static void Clean()
        {
            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string projectFilePath = Path.Combine(CurrentProjectFolder, solutionName, $"{solutionName}.csproj");
            Logger.Log(Dotnet.Clean(projectFilePath));
            scriptProjectChanged = true;
        }

        public static Task Publish(PublishSettings settings)
        {
            var debugType = settings.StripDebugInfo ? DebugType.None : DebugType.Full;
            if (settings.StartupScene == null)
            {
                return Task.CompletedTask;
            }

            if (CurrentProjectFolder == null)
            {
                return Task.CompletedTask;
            }

            if (CurrentProjectFilePath == null)
            {
                return Task.CompletedTask;
            }

            if (CurrentProjectAssetsFolder == null)
            {
                return Task.CompletedTask;
            }

            for (int i = 0; i < settings.Scenes.Count; i++)
            {
                var scene = settings.Scenes[i];
                var path = Path.Combine(CurrentProjectAssetsFolder, scene);
                if (!File.Exists(path))
                {
                    return Task.CompletedTask;
                }
            }

            Logger.Info("Publishing Project");
            string buildPath = Path.Combine(CurrentProjectFolder, "build");
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
            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string scriptProjPath = Path.Combine(CurrentProjectFolder, solutionName);
            string scriptPublishPath = Path.Combine(scriptProjPath, "bin", "Publish");
            PublishOptions scriptPublishOptions = new()
            {
                Framework = "net8.0",
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
            {
                File.Copy(assemblyPdbPath, assemblyPdbBuildPath);
            }

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
            string appTempPath = Path.Combine(CurrentProjectFolder, ".build");
            string appTempPublishPath = Path.Combine(appTempPath, "publish");
            string appTempProjPath = Path.Combine(appTempPath, $"{solutionName}.csproj");
            string appTempProgramPath = Path.Combine(appTempPath, "Program.cs");
            if (Directory.Exists(appTempPath))
            {
                Directory.Delete(appTempPath, true);
            }
            Directory.CreateDirectory(appTempPath);
            Logger.Log(Dotnet.New(DotnetTemplate.Console, appTempPath, solutionName));
            Logger.Log(Dotnet.AddDlls(appTempProjPath,
                Path.GetFullPath("HexaEngine.dll"),
                Path.GetFullPath("HexaEngine.Core.dll"),
                Path.GetFullPath("HexaEngine.Mathematics.dll"),
                Path.GetFullPath("HexaEngine.D3D11.dll"),
                Path.GetFullPath("HexaEngine.D3D12.dll"),
                Path.GetFullPath("HexaEngine.Vulkan.dll"),
                Path.GetFullPath("HexaEngine.OpenGL.dll"),
                Path.GetFullPath("HexaEngine.OpenAL.dll"),
                Path.GetFullPath("HexaEngine.XAudio.dll"),
                Path.GetFullPath("Silk.NET.Core.dll")));
            if (settings.RuntimeIdentifier?.Contains("win") ?? false)
            {
                Dotnet.ChangeOutputType(appTempProjPath, "WinExe");
            }

            File.WriteAllText(appTempProgramPath, PublishProgram);
            PublishOptions appPublishOptions = new()
            {
                Framework = "net8.0",
                Profile = settings.Profile,
                RuntimeIdentifer = settings.RuntimeIdentifier,
                PublishReadyToRun = settings.ReadyToRun,
                PublishSingleFile = settings.SingleFile,
                SelfContained = true,
                DebugSymbols = !settings.StripDebugInfo,
                DebugType = debugType,
            };
            Logger.Log(Dotnet.Publish(appTempPath, appTempPublishPath, appPublishOptions));

            // copy native runtimes
            string localRuntimesPath = Path.GetFullPath("runtimes");
            string targetRuntimePath = Path.Combine(localRuntimesPath, settings.RuntimeIdentifier, "native");

            CopyDirectory(targetRuntimePath, appTempPublishPath, true);

            // copy binaries to build folder
            CopyDirectory(appTempPublishPath, buildPath, true);

            if (settings.StripDebugInfo)
            {
                DeleteFile(buildPath, "*.pdb", true);
            }

            Logger.Info("Published Project");

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
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

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

        private const string PublishProgram =
@"
namespace App
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Windows;

    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            Window window = new() { Flags = RendererFlags.None };
            Platform.Init(window, GraphicsBackend.D3D11);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}
";
    }
}