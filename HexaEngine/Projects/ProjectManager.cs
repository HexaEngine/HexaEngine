namespace HexaEngine.Projects
{
    using HexaEngine.Dotnet;
    using HexaEngine.Editor.Projects;
    using HexaEngine.IO;
    using HexaEngine.Objects;
    using HexaEngine.Scripting;
    using HexaEngine.Windows;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class ProjectManager
    {
        public static readonly List<string> ReferencedAssemblyNames = new();

        public static readonly List<Assembly> ReferencedAssemblies = new();

        static ProjectManager()
        {
            ReferencedAssemblyNames.Add("HexaEngine.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            ReferencedAssemblyNames.Add("HexaEngine.IO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
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

        public static HexaProject? Project { get; set; }

        public static event Action<HexaProject> ProjectLoaded;

        public static void Load(string path)
        {
            CurrentProjectPath = path;
            Project = HexaProject.Load(CurrentProjectPath) ?? throw new Exception();
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
            ProjectLoaded?.Invoke(Project);
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
            ProjectLoaded?.Invoke(Project);
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

        public static void Update()
        {
            AssemblyManager.Unload();
            if (CurrentFolder == null) return;
            string solutionName = Path.GetFileName(CurrentFolder);
            Build();
            string outputFilePath = Path.Combine(CurrentFolder, "bin", $"{solutionName}.dll");
            AssemblyManager.Load(outputFilePath);
        }

        private static void Build()
        {
            string solutionName = Path.GetFileName(CurrentFolder);
            string projectFilePath = Path.Combine(CurrentFolder, solutionName, $"{solutionName}.csproj");
            Dotnet.Build(projectFilePath, Path.Combine(CurrentFolder, "bin"));
        }
    }

    public static class ComponentRegistry
    {
        private static readonly List<Type> _components = new();

        static ComponentRegistry()
        {
            Scripts = new();
            Assembly assembly = Assembly.GetAssembly(typeof(Window));
            _components = new(assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null));
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
            Assembly assembly = AssemblyManager.Load(path);
            foreach (var type in assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null))
            {
                Components.Add(type);
            }

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