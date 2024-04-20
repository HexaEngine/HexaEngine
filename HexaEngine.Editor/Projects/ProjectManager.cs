namespace HexaEngine.Editor.Projects
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Archives;
    using HexaEngine.Core.UI;
    using HexaEngine.Dotnet;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Tools;
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
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ProjectManager));
        private static bool loaded;
        private static FileSystemWatcher? watcher;
        private static bool scriptProjectChanged;
        private static bool scriptProjectBuildFailed;

        private static readonly SemaphoreSlim semaphore = new(1);

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

        public static HexaProject? CurrentProject { get; private set; }

        public static bool ScriptProjectChanged => scriptProjectChanged;

        public static bool ScriptProjectValid => !scriptProjectBuildFailed;

        public static bool ScriptProjectBuildFailed => scriptProjectBuildFailed;

        public static event ProjectUnloadedHandler? ProjectUnloaded;

        public static event ProjectLoadingHandler? ProjectLoading;

        public static event ProjectLoadFailedHandler? ProjectLoadFailed;

        public static event ProjectLoadedHandler? ProjectLoaded;

        public delegate void ProjectUnloadedHandler(string? projectFile);

        public delegate void ProjectLoadingHandler(string projectFile);

        public delegate void ProjectLoadFailedHandler(string projectFile, Exception exception);

        public delegate void ProjectLoadedHandler(HexaProject project);

        public static bool Loaded => loaded;

        public static Task Load(string path)
        {
            return Task.Run(() =>
            {
                semaphore.Wait();

                UnloadInternal();

                var popup = PopupManager.Show(new ProgressModal("Loading project...", "Please wait, loading project...", ProgressType.Bar));
                try
                {
                    ProjectLoading?.Invoke(path);

                    CurrentProjectFilePath = path;

                    loaded = true;

                    CurrentProjectFolder = Path.GetDirectoryName(CurrentProjectFilePath);

                    HexaProjectReader reader = new(path);

                    CurrentProject = reader.Read() ?? throw new FileNotFoundException($"Couldn't find project file '{path}'");

                    ProjectVersionControl.TryInit();

                    CurrentProjectAssetsFolder = Path.Combine(CurrentProjectFolder, "assets");
                    string solutionName = Path.GetFileName(CurrentProjectFolder);
                    Directory.CreateDirectory(CurrentProjectAssetsFolder);
                    FileSystem.AddSource(CurrentProjectAssetsFolder);
                    Paths.CurrentProjectFolder = CurrentProjectAssetsFolder;
                    ProjectHistory.AddEntry(solutionName, CurrentProjectFilePath);

                    SourceAssetsDatabase.Init(CurrentProjectFolder, popup);

                    watcher = new(Path.Combine(CurrentProjectFolder, solutionName));
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
                    watcher.Changed += WatcherChanged;
                    watcher.Created += WatcherChanged;
                    watcher.Deleted += WatcherChanged;
                    watcher.Renamed += WatcherChanged;
                    watcher.IncludeSubdirectories = true;
                    watcher.EnableRaisingEvents = true;

                    BuildScriptsAsync(true);
                }
                catch (Exception ex)
                {
                    // unload/reset project to keep a consistent state.
                    UnloadInternal();
                    Logger.Error($"Failed to load project. '{path}'");
                    Logger.Log(ex);
                    MessageBox.Show($"Failed to load project. '{path}'", ex.Message);
                    return;
                }
                finally
                {
                    popup.Close();
                }

                Logger.Info($"Loaded Project '{path}'");
                ProjectLoaded?.Invoke(CurrentProject);

                semaphore.Release();
            });
        }

        public static void Unload()
        {
            semaphore.Wait();

            UnloadInternal();

            semaphore.Release();
        }

        private static void UnloadInternal()
        {
            if (loaded)
            {
                ProjectUnloaded?.Invoke(CurrentProjectFilePath);
                FileSystem.RemoveSource(CurrentProjectAssetsFolder);
                ProjectVersionControl.Unload();
                SourceAssetsDatabase.Clear();
                CurrentProjectFilePath = null;
                CurrentProjectFolder = null;
                CurrentProjectAssetsFolder = null;
                CurrentProject = null;
                loaded = false;
            }
        }

        private static void WatcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            ReadOnlySpan<char> extension = Path.GetExtension(e.Name.AsSpan());

            if (extension.SequenceEqual(".meta", CharComparerIgnoreCase.Instance) || extension.SequenceEqual(".tmp", CharComparerIgnoreCase.Instance))
            {
                return;
            }

            ReadOnlySpan<char> currentDir = Path.GetDirectoryName(e.FullPath.AsSpan());
            while (!currentDir.IsEmpty)
            {
                var name = Path.GetFileName(currentDir);
                if (name.SequenceEqual("bin", CharComparerIgnoreCase.Instance) || name.SequenceEqual(".obj", CharComparerIgnoreCase.Instance))
                {
                    return;
                }

                currentDir = Path.GetDirectoryName(currentDir);
            }

            scriptProjectChanged = true;
        }

        public static Task Create(string path)
        {
            return Task.Run(async () =>
            {
                string projectFilePath;
                var popup = PopupManager.Show(new ProgressModal("Generating project...", "Please wait, generating project..."));
                try
                {
                    projectFilePath = GenerateProject(path);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to create project. '{path}'");
                    Logger.Log(ex);
                    MessageBox.Show($"Failed to create project. '{path}'", ex.Message);
                    return;
                }
                finally
                {
                    popup.Close();
                }

                await Load(projectFilePath);
            });
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        private static string GenerateProject(string path)
        {
            if (Directory.Exists(path) && !IsDirectoryEmpty(path))
            {
                throw new InvalidOperationException($"Directory '{path}' is not empty.");
            }

            string projectName = Path.GetFileName(path);
            string projectFilePath = Path.Combine(path, $"{projectName}.hexproj");
            HexaProject project = HexaProject.CreateNew();

            HexaProjectWriter writer = new(projectFilePath);
            writer.Write(project);
            writer.Close();

            var currentProjectAssetsFolder = Path.Combine(path, "assets");
            Directory.CreateDirectory(currentProjectAssetsFolder);

            GenerateSolution(path);

            return projectFilePath;
        }

        private static void GenerateSolution(string path)
        {
            string solutionName = Path.GetFileName(path);
            string solutionPath = Path.Combine(path, solutionName + ".sln");
            Dotnet.New(DotnetTemplate.Sln, path, solutionName);
            Dotnet.New(DotnetTemplate.Classlib, Path.Combine(path, solutionName));
            string projectPath = Path.Combine(path, solutionName, $"{solutionName}.csproj");
            string projectFilePath = Path.Combine(path, solutionName, $"{solutionName}.csproj");
            Dotnet.Sln(SlnCommand.Add, solutionPath, projectPath);
            Dotnet.AddDlls(projectFilePath, ReferencedAssemblies.ConvertAll(x => x.Location));
        }

        public static void SaveProjectFile()
        {
            if (CurrentProject == null || CurrentProjectFilePath == null)
            {
                return;
            }

            HexaProjectWriter writer = new(CurrentProjectFilePath);
            writer.Write(CurrentProject);
            writer.Close();
        }

        public static void OpenVisualStudio()
        {
            semaphore.Wait();
            if (CurrentProjectFolder == null)
            {
                return;
            }

            string? solutionName = Path.GetFileName(CurrentProjectFolder);
            string solutionPath = Path.Combine(CurrentProjectFolder, solutionName + ".sln");

            ProcessStartInfo psi = new();
            switch (EditorConfig.Default.ExternalTextEditorType)
            {
                case ExternalTextEditorType.VisualStudio:
                    {
                        psi.FileName = "cmd";
                        psi.Arguments = $"/c start /B devenv \"{solutionPath}\"";
                        psi.CreateNoWindow = true;
                        psi.UseShellExecute = false;
                    }
                    break;

                case ExternalTextEditorType.VSCode:
                    {
                        psi.FileName = "cmd";
                        psi.Arguments = $"/c start /B code \"{CurrentProjectFolder}\"";
                        psi.CreateNoWindow = true;
                        psi.UseShellExecute = false;
                    }
                    break;

                case ExternalTextEditorType.Custom:
                    {
                        var editorIndex = EditorConfig.Default.SelectedExternalTextEditor;
                        var editors = EditorConfig.Default.ExternalTextEditors;
                        var editor = editorIndex >= 0 && editorIndex < editors.Count ? editors[editorIndex] : null;
                        if (editor == null)
                        {
                            semaphore.Release();
                            return;
                        }
                        psi.FileName = editor.ProgramPath;
                        psi.Arguments = ArgumentsParser.Parse(editor.CommandLine, new Dictionary<string, string>()
                        {
                            { "solutionPath", $"\"{solutionPath}\"" },
                            { "solutionName", $"\"{solutionName}\"" },
                            { "projectFolder", $"\"{CurrentProjectFolder}\"" }
                        });
                        psi.CreateNoWindow = true;
                        psi.UseShellExecute = false;
                    }
                    break;

                default:
                    break;
            }

            try
            {
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open external text editor", ex.Message);
                Logger.Error("Failed to open external text editor");
                Logger.Log(ex);
            }

            semaphore.Release();
        }

        public static void BuildScripts(bool force = false)
        {
            semaphore.Wait();

            if (!scriptProjectChanged && !force)
            {
                semaphore.Release();
                return;
            }

            ProgressModal modal = new("Building Scripts", "Building Scripts ...", ProgressType.Spinner, ProgressFlags.NoOverlay | ProgressFlags.NoModal | ProgressFlags.BottomLeft, new(0.01f, 0.01f));
            PopupManager.Show(modal);

            ScriptAssemblyManager.Unload();
            if (CurrentProjectFolder == null)
            {
                ScriptAssemblyManager.SetInvalid(true);
                semaphore.Release();
                modal.Close();
                return;
            }

            if (!Build())
            {
                ScriptAssemblyManager.SetInvalid(true);
                scriptProjectBuildFailed = true;
                semaphore.Release();
                modal.Close();
                return;
            }

            scriptProjectBuildFailed = false;

            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string outputFilePath = Path.Combine(CurrentProjectFolder, solutionName, "bin", $"{solutionName}.dll");

            ScriptAssemblyManager.Load(outputFilePath);

            modal.Close();

            semaphore.Release();
        }

        public static Task BuildScriptsAsync(bool force = false)
        {
#nullable disable
            return Task.Factory.StartNew(obj => BuildScripts((bool)obj), force);
#nullable enable
        }

        public static Task CleanScriptsAsync()
        {
            return Task.Run(CleanScripts);
        }

        public static void CleanScripts()
        {
            semaphore.Wait();
            ScriptAssemblyManager.Unload();
            if (CurrentProjectFolder == null)
            {
                semaphore.Release();
                return;
            }

            Clean();

            semaphore.Release();
        }

        private static bool Build()
        {
            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string projectFilePath = Path.Combine(CurrentProjectFolder, solutionName, $"{solutionName}.csproj");

            string output = Dotnet.Build(projectFilePath, Path.Combine(CurrentProjectFolder, solutionName, "bin"));
            bool failed = output.Contains("FAILED");
            AnalyseLog(output);
            scriptProjectChanged = false;

            return !failed;
        }

        private static void Clean()
        {
            string solutionName = Path.GetFileName(CurrentProjectFolder);
            string projectFilePath = Path.Combine(CurrentProjectFolder, solutionName, $"{solutionName}.csproj");
            AnalyseLog(Dotnet.Clean(projectFilePath));
            scriptProjectChanged = true;
        }

        private static void AnalyseLog(string message)
        {
            string[] lines = message.Split('\n');
            foreach (string line in lines)
            {
                if (line.Contains("0 Warning(s)"))
                {
                    LoggerFactory.General.Info(line);
                }
                else if (line.Contains("0 Error(s)"))
                {
                    LoggerFactory.General.Info(line);
                }
                else
                {
                    LoggerFactory.General.Log(line);
                }
            }
        }

        public static Task Publish(PublishSettings settings)
        {
            semaphore.Wait();
            var debugType = settings.StripDebugInfo ? DebugType.None : DebugType.Full;
            if (settings.StartupScene == null)
            {
                semaphore.Release();
                return Task.CompletedTask;
            }

            if (CurrentProjectFolder == null)
            {
                semaphore.Release();
                return Task.CompletedTask;
            }

            if (CurrentProjectFilePath == null)
            {
                semaphore.Release();
                return Task.CompletedTask;
            }

            if (CurrentProjectAssetsFolder == null)
            {
                semaphore.Release();
                return Task.CompletedTask;
            }

            for (int i = 0; i < settings.Scenes.Count; i++)
            {
                var scene = settings.Scenes[i];
                var path = Path.Combine(CurrentProjectAssetsFolder, scene);
                if (!File.Exists(path))
                {
                    semaphore.Release();
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

            semaphore.Release();

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