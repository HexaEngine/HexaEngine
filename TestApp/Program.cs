namespace TestApp
{
    using HexaEngine;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Windows;
    using HexaEngine.Editor.Projects;

    public static unsafe partial class Program
    {
        public static void Main()
        {
            FileSystem.Initialize();
            ProjectManager.Load("C:\\Users\\juna\\Desktop\\Data\\TestProject\\TestProject.hexproj");
        }
    }
}